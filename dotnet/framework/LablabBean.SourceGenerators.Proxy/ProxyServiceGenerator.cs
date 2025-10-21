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
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

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
            if (modeArg.Kind == TypedConstantKind.Enum)
            {
                return modeArg.Value?.ToString();
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

        // Method signature
        sb.Append("        ");
        sb.Append("public ");

        // Return type
        sb.Append(method.ReturnType.ToDisplayString());
        sb.Append(" ");

        // Method name
        sb.Append(method.Name);

        // Parameters
        sb.Append("(");
        var parameters = method.Parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
                sb.Append(", ");

            var param = parameters[i];
            sb.Append(param.Type.ToDisplayString());
            sb.Append(" ");
            sb.Append(param.Name);
        }
        sb.AppendLine(")");

        // Method body
        sb.AppendLine("        {");

        // Generate delegation call
        var serviceCall = selectionMode switch
        {
            "One" => $"_registry.Get<{serviceType.ToDisplayString()}>(LablabBean.Plugins.Contracts.SelectionMode.One)",
            "HighestPriority" => $"_registry.Get<{serviceType.ToDisplayString()}>(LablabBean.Plugins.Contracts.SelectionMode.HighestPriority)",
            "All" => $"_registry.GetAll<{serviceType.ToDisplayString()}>()",
            _ => $"_registry.Get<{serviceType.ToDisplayString()}>()"
        };

        // Build argument list
        var args = string.Join(", ", parameters.Select(p => p.Name));

        if (method.ReturnsVoid)
        {
            sb.AppendLine($"            {serviceCall}.{method.Name}({args});");
        }
        else
        {
            sb.AppendLine($"            return {serviceCall}.{method.Name}({args});");
        }

        sb.AppendLine("        }");
    }
}
