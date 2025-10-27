using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace LablabBean.SourceGenerators.Proxy;

/// <summary>
/// Incremental source generator that creates proxy service implementations
/// for partial classes marked with [RealizeService] attribute.
/// </summary>
[Generator]
public class ProxyServiceGenerator : IIncrementalGenerator
{
    private const string RealizeServiceAttributeName = "LablabBean.Plugins.Contracts.Attributes.RealizeServiceAttribute";
    private const string SelectionStrategyAttributeName = "LablabBean.Plugins.Contracts.Attributes.SelectionStrategyAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register a syntax provider to find partial classes with [RealizeService] attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateClass(node),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate source
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        // Check if node is a class declaration with attributes
        if (node is not ClassDeclarationSyntax classDeclaration)
            return false;

        // Must have attributes
        if (classDeclaration.AttributeLists.Count == 0)
            return false;

        // Must be partial
        if (!classDeclaration.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword))
            return false;

        return true;
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // Check if class has [RealizeService] attribute
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
                if (symbol is IMethodSymbol attributeSymbol)
                {
                    var attributeClass = attributeSymbol.ContainingType;
                    var fullName = attributeClass.ToDisplayString();

                    if (fullName == RealizeServiceAttributeName)
                    {
                        return classDeclaration;
                    }
                }
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        foreach (var classDeclaration in classes)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken) as INamedTypeSymbol;

            if (classSymbol is null)
                continue;

            // Process this class
            ProcessClass(context, classSymbol, classDeclaration, compilation);
        }
    }

    private static void ProcessClass(
        SourceProductionContext context,
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax classDeclaration,
        Compilation compilation)
    {
        // Extract service interface type from [RealizeService(typeof(IService))]
        var serviceType = GetServiceTypeFromAttribute(classSymbol, compilation);
        if (serviceType is null)
        {
            // Report diagnostic: Could not determine service type
            return;
        }

        // Validate that service type is an interface
        if (serviceType.TypeKind != TypeKind.Interface)
        {
            // Report diagnostic: Service type must be an interface
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor(
                    "PROXY001",
                    "Service type must be an interface",
                    "The type '{0}' specified in [RealizeService] must be an interface",
                    "ProxyGenerator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                classDeclaration.GetLocation(),
                serviceType.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Validate that class has _registry field
        var hasRegistryField = classSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Any(f => f.Name == "_registry" && f.Type.Name == "IRegistry");

        if (!hasRegistryField)
        {
            // Report diagnostic: Missing _registry field
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor(
                    "PROXY002",
                    "Missing IRegistry field",
                    "The class '{0}' must have a field 'private readonly IRegistry _registry;'",
                    "ProxyGenerator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                classDeclaration.GetLocation(),
                classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Get selection strategy
        var selectionMode = GetSelectionStrategy(classSymbol, compilation);

        // Generate source
        var source = GenerateProxySource(classSymbol, serviceType, selectionMode);

        // Add source to compilation
        context.AddSource($"{classSymbol.Name}.g.cs", source);
    }

    private static ITypeSymbol? GetServiceTypeFromAttribute(INamedTypeSymbol classSymbol, Compilation compilation)
    {
        var realizeServiceAttribute = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == RealizeServiceAttributeName);

        if (realizeServiceAttribute is null)
            return null;

        // Get the typeof() argument
        if (realizeServiceAttribute.ConstructorArguments.Length > 0)
        {
            var typeArg = realizeServiceAttribute.ConstructorArguments[0];
            if (typeArg.Kind == TypedConstantKind.Type)
            {
                return typeArg.Value as ITypeSymbol;
            }
        }

        return null;
    }

    private static string? GetSelectionStrategy(INamedTypeSymbol classSymbol, Compilation compilation)
    {
        var selectionStrategyAttribute = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == SelectionStrategyAttributeName);

        if (selectionStrategyAttribute is null)
            return null; // No strategy specified, use default

        // Get the SelectionMode enum value
        if (selectionStrategyAttribute.ConstructorArguments.Length > 0)
        {
            var modeArg = selectionStrategyAttribute.ConstructorArguments[0];
            if (modeArg.Kind == TypedConstantKind.Enum && modeArg.Type is INamedTypeSymbol enumType)
            {
                // Get the enum member name from the value
                var enumValue = modeArg.Value;
                if (enumValue != null)
                {
                    foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>())
                    {
                        if (member.HasConstantValue && Equals(member.ConstantValue, enumValue))
                        {
                            return member.Name;
                        }
                    }
                }
            }
        }

        return null;
    }

    private static string GenerateProxySource(
        INamedTypeSymbol classSymbol,
        ITypeSymbol serviceType,
        string? selectionMode)
    {
        var sb = new StringBuilder();

        // File header
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine($"// Generated by ProxyServiceGenerator at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        // Namespace
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");

        // Class declaration
        sb.AppendLine($"    partial class {classSymbol.Name} : {serviceType.ToDisplayString()}");
        sb.AppendLine("    {");

        // Generate methods
        var methods = GetAllInterfaceMembers(serviceType).OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary);

        foreach (var method in methods)
        {
            GenerateMethod(sb, method, serviceType, selectionMode);
        }

        // Generate properties
        var properties = GetAllInterfaceMembers(serviceType).OfType<IPropertySymbol>();

        foreach (var property in properties)
        {
            GenerateProperty(sb, property, serviceType, selectionMode);
        }

        // Generate events
        var events = GetAllInterfaceMembers(serviceType).OfType<IEventSymbol>();

        foreach (var evt in events)
        {
            GenerateEvent(sb, evt, serviceType, selectionMode);
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static IEnumerable<ISymbol> GetAllInterfaceMembers(ITypeSymbol interfaceType)
    {
        var members = new List<ISymbol>(interfaceType.GetMembers());

        // Add members from base interfaces
        foreach (var baseInterface in interfaceType.AllInterfaces)
        {
            members.AddRange(baseInterface.GetMembers());
        }

        return members;
    }

    private static void GenerateMethod(
        StringBuilder sb,
        IMethodSymbol method,
        ITypeSymbol serviceType,
        string? selectionMode)
    {
        sb.AppendLine();

        // XML documentation (T068)
        var xmlDoc = method.GetDocumentationCommentXml();
        if (!string.IsNullOrWhiteSpace(xmlDoc))
        {
            GenerateXmlDocumentation(sb, xmlDoc, "        ");
        }

        // Method signature
        sb.Append("        ");
        sb.Append("public ");

        // Return type
        sb.Append(method.ReturnType.ToDisplayString());
        sb.Append(" ");

        // Method name
        sb.Append(method.Name);

        // Generic type parameters (T034)
        if (method.IsGenericMethod)
        {
            sb.Append("<");
            var typeParams = method.TypeParameters;
            for (int i = 0; i < typeParams.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(typeParams[i].Name);
            }
            sb.Append(">");
        }

        // Parameters
        sb.Append("(");
        var parameters = method.Parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");

            var param = parameters[i];

            // Parameter modifiers: ref/out/in/params (T038-T041)
            if (param.RefKind == RefKind.Ref)
                sb.Append("ref ");
            else if (param.RefKind == RefKind.Out)
                sb.Append("out ");
            else if (param.RefKind == RefKind.In)
                sb.Append("in ");

            if (param.IsParams)
                sb.Append("params ");

            sb.Append(param.Type.ToDisplayString());
            sb.Append(" ");
            sb.Append(param.Name);

            // Default parameter values (T044)
            if (param.HasExplicitDefaultValue)
            {
                sb.Append(" = ");
                if (param.ExplicitDefaultValue == null)
                {
                    if (param.Type.IsValueType)
                        sb.Append("default");
                    else
                        sb.Append("null");
                }
                else if (param.ExplicitDefaultValue is string strValue)
                {
                    sb.Append($"\"{strValue}\"");
                }
                else if (param.ExplicitDefaultValue is bool boolValue)
                {
                    sb.Append(boolValue ? "true" : "false");
                }
                else
                {
                    sb.Append(param.ExplicitDefaultValue.ToString());
                }
            }
        }
        sb.Append(")");

        // Type constraints (T035)
        if (method.IsGenericMethod)
        {
            foreach (var typeParam in method.TypeParameters)
            {
                var constraints = new List<string>();

                if (typeParam.HasReferenceTypeConstraint)
                    constraints.Add("class");
                if (typeParam.HasValueTypeConstraint)
                    constraints.Add("struct");
                if (typeParam.HasUnmanagedTypeConstraint)
                    constraints.Add("unmanaged");
                if (typeParam.HasNotNullConstraint)
                    constraints.Add("notnull");

                foreach (var constraintType in typeParam.ConstraintTypes)
                {
                    constraints.Add(constraintType.ToDisplayString());
                }

                if (typeParam.HasConstructorConstraint)
                    constraints.Add("new()");

                if (constraints.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append($"            where {typeParam.Name} : {string.Join(", ", constraints)}");
                }
            }
        }

        sb.AppendLine();

        // Method body
        sb.AppendLine("        {");

        // Generate delegation call
        var serviceCall = selectionMode switch
        {
            "One" => $"_registry.Get<{serviceType.ToDisplayString()}>(LablabBean.Plugins.Contracts.SelectionMode.One)",
            "HighestPriority" => $"_registry.Get<{serviceType.ToDisplayString()}>(LablabBean.Plugins.Contracts.SelectionMode.HighestPriority)",
            "All" => $"_registry.GetAll<{serviceType.ToDisplayString()}>().First()",
            _ => $"_registry.Get<{serviceType.ToDisplayString()}>()"
        };

        // Build argument list with ref/out/in modifiers (T038-T040)
        var argsList = new List<string>();
        foreach (var param in parameters)
        {
            var argPrefix = param.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => ""
            };
            argsList.Add($"{argPrefix}{param.Name}");
        }
        var args = string.Join(", ", argsList);

        // Add generic type arguments for generic methods (T034)
        var methodCall = method.Name;
        if (method.IsGenericMethod)
        {
            methodCall += "<" + string.Join(", ", method.TypeParameters.Select(tp => tp.Name)) + ">";
        }

        if (method.ReturnsVoid)
        {
            sb.AppendLine($"            {serviceCall}.{methodCall}({args});");
        }
        else
        {
            sb.AppendLine($"            return {serviceCall}.{methodCall}({args});");
        }

        sb.AppendLine("        }");
    }

    private static void GenerateProperty(
        StringBuilder sb,
        IPropertySymbol property,
        ITypeSymbol serviceType,
        string? selectionMode)
    {
        sb.AppendLine();

        // XML documentation (T068)
        var xmlDoc = property.GetDocumentationCommentXml();
        if (!string.IsNullOrWhiteSpace(xmlDoc))
        {
            GenerateXmlDocumentation(sb, xmlDoc, "        ");
        }

        // Property signature
        sb.Append("        ");
        sb.Append("public ");

        // Property type
        sb.Append(property.Type.ToDisplayString());
        sb.Append(" ");

        // Property name
        sb.AppendLine(property.Name);
        sb.AppendLine("        {");

        // Generate service call
        var serviceCall = selectionMode switch
        {
            "One" => $"_registry.Get<{serviceType.ToDisplayString()}>(LablabBean.Plugins.Contracts.SelectionMode.One)",
            "HighestPriority" => $"_registry.Get<{serviceType.ToDisplayString()}>(LablabBean.Plugins.Contracts.SelectionMode.HighestPriority)",
            "All" => $"_registry.GetAll<{serviceType.ToDisplayString()}>()",
            _ => $"_registry.Get<{serviceType.ToDisplayString()}>()"
        };

        // Getter
        if (property.GetMethod is not null)
        {
            sb.AppendLine($"            get => {serviceCall}.{property.Name};");
        }

        // Setter
        if (property.SetMethod is not null)
        {
            sb.AppendLine($"            set => {serviceCall}.{property.Name} = value;");
        }

        sb.AppendLine("        }");
    }

    private static void GenerateEvent(
        StringBuilder sb,
        IEventSymbol evt,
        ITypeSymbol serviceType,
        string? selectionMode)
    {
        sb.AppendLine();

        // XML documentation (T068)
        var xmlDoc = evt.GetDocumentationCommentXml();
        if (!string.IsNullOrWhiteSpace(xmlDoc))
        {
            GenerateXmlDocumentation(sb, xmlDoc, "        ");
        }

        // Event signature
        sb.Append("        ");
        sb.Append("public event ");

        // Event type
        sb.Append(evt.Type.ToDisplayString());
        sb.Append(" ");

        // Event name
        sb.Append(evt.Name);
        sb.AppendLine();
        sb.AppendLine("        {");

        // Generate service call
        var serviceCall = selectionMode switch
        {
            "One" => $"_registry.Get<{serviceType.ToDisplayString()}>(LablabBean.Plugins.Contracts.SelectionMode.One)",
            "HighestPriority" => $"_registry.Get<{serviceType.ToDisplayString()}>(LablabBean.Plugins.Contracts.SelectionMode.HighestPriority)",
            "All" => $"_registry.GetAll<{serviceType.ToDisplayString()}>()",
            _ => $"_registry.Get<{serviceType.ToDisplayString()}>()"
        };

        // Add accessor
        sb.AppendLine($"            add => {serviceCall}.{evt.Name} += value;");

        // Remove accessor
        sb.AppendLine($"            remove => {serviceCall}.{evt.Name} -= value;");

        sb.AppendLine("        }");
    }

    private static void GenerateXmlDocumentation(StringBuilder sb, string? xmlDoc, string indent)
    {
        if (string.IsNullOrWhiteSpace(xmlDoc))
            return;

        // Parse XML documentation and convert to /// comments
        // Simple implementation: extract summary, param, returns tags
        var lines = xmlDoc!.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            // Skip XML declaration and member tags
            if (trimmed.StartsWith("<?xml") || trimmed.StartsWith("<member"))
                continue;

            // Convert to /// comment format
            sb.Append(indent);
            sb.Append("/// ");
            sb.AppendLine(trimmed);
        }
    }
}
