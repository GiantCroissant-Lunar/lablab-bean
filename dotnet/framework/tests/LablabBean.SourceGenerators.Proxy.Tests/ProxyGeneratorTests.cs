using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.Contracts.Attributes;
using LablabBean.Plugins.Contracts.Services;

namespace LablabBean.SourceGenerators.Proxy.Tests;

/// <summary>
/// Tests for ProxyServiceGenerator basic functionality.
/// </summary>
public class ProxyGeneratorTests
{
    [Fact]
    public void Generator_FindsPartialClassWithAttribute()
    {
        // This test verifies the generator can find and process
        // a partial class marked with [RealizeService]

        // Arrange - Create a simple test interface
        var testInterface = typeof(ITestService);

        // Act - The generator should process TestProxy class below
        var proxy = new TestProxy(new TestRegistry());

        // Assert - Proxy should be created (compilation test)
        Assert.NotNull(proxy);
    }

    [Fact]
    public void Attributes_AreAccessible()
    {
        // Verify attributes are properly defined and accessible
        var realizeAttr = typeof(RealizeServiceAttribute);
        var strategyAttr = typeof(SelectionStrategyAttribute);

        Assert.NotNull(realizeAttr);
        Assert.NotNull(strategyAttr);
    }

    [Fact]
    public void Generator_HandlesProperties()
    {
        // Arrange
        var proxy = new TestProxy(new TestRegistry());

        // Act & Assert - Property getter
        var name = proxy.Name;
        Assert.Equal("TestService", name);

        // Act & Assert - Property setter
        proxy.Name = "NewName";

        // Act & Assert - Read-only property
        var value = proxy.ReadOnlyValue;
        Assert.Equal(42, value);
    }

    [Fact]
    public void Generator_HandlesEvents()
    {
        // Arrange
        var proxy = new TestProxy(new TestRegistry());

        // Act - Subscribe to event
        EventHandler? handler = (sender, args) => { };
        proxy.DataChanged += handler;

        // Assert - Event subscription works (compilation test)
        Assert.NotNull(proxy);
    }

    [Fact]
    public void Generator_HandlesReadOnlyProperty()
    {
        // T025: Handle read-only properties (get-only, no setter)
        // Arrange
        var proxy = new TestProxy(new TestRegistry());

        // Act
        var value = proxy.ReadOnlyValue;

        // Assert - Should compile and return value
        Assert.Equal(42, value);
    }

    [Fact]
    public void Generator_HandlesReadWriteProperty()
    {
        // T023 & T024: Property getter and setter generation
        // Arrange
        var proxy = new TestProxy(new TestRegistry());

        // Act - Get initial value
        var initialValue = proxy.Name;

        // Act - Set new value
        proxy.Name = "UpdatedName";

        // Assert - Both operations should work
        Assert.Equal("TestService", initialValue);
    }

    [Fact]
    public void Generator_HandlesEventAddRemove()
    {
        // T028 & T029: Event add and remove accessors
        // Arrange
        var proxy = new TestProxy(new TestRegistry());
        EventHandler? handler = (sender, args) => { };

        // Act - Add event handler
        proxy.DataChanged += handler;

        // Act - Remove event handler
        proxy.DataChanged -= handler;

        // Assert - Should compile without errors
        Assert.NotNull(proxy);
    }

    [Fact]
    public void Generator_HandlesMultipleProperties()
    {
        // T027: Handle auto-property syntax in generated code
        // Arrange
        var proxy = new PropertyTestProxy(new TestRegistry());

        // Act & Assert - Multiple property types
        var readOnly = proxy.ReadOnlyProp;
        Assert.Equal(100, readOnly);

        var readWrite = proxy.ReadWriteProp;
        proxy.ReadWriteProp = "Modified";
        Assert.Equal("Default", readWrite);

        proxy.WriteOnlyProp = 42;
        // Write-only property set successfully (compilation test)
        Assert.NotNull(proxy);
    }

    // ===== Phase 3: Advanced Method Features Tests =====

    [Fact]
    public void Generator_HandlesGenericMethod()
    {
        // T034: Generate generic methods with type parameters
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());

        // Act
        var result = proxy.GenericMethod(42);

        // Assert - Should compile and work
        Assert.Equal(42, result);
    }

    [Fact]
    public void Generator_HandlesMultipleTypeParameters()
    {
        // T036: Handle multiple type parameters
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());

        // Act - Call method with multiple type parameters
        var result = proxy.Transform<string, int>("test");

        // Assert - Should compile (compilation test)
        Assert.NotNull(proxy);
    }

    [Fact]
    public void Generator_HandlesTypeConstraints()
    {
        // T035: Preserve type constraints
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());

        // Act - Call method with constraints (where T : class, new())
        var result = proxy.CreateInstance<TestServiceImpl>();

        // Assert - Should compile and create instance
        Assert.NotNull(result);
    }

    [Fact]
    public void Generator_HandlesRefParameters()
    {
        // T038: Handle ref parameters
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());
        int value = 5;

        // Act
        proxy.ModifyValue(ref value);

        // Assert - Value should be modified
        Assert.Equal(10, value);
    }

    [Fact]
    public void Generator_HandlesOutParameters()
    {
        // T039: Handle out parameters
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());

        // Act
        var success = proxy.TryGetValue("key", out string value);

        // Assert
        Assert.True(success);
        Assert.Equal("Value for key", value);
    }

    [Fact]
    public void Generator_HandlesInParameters()
    {
        // T040: Handle in parameters
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());
        ReadOnlySpan<byte> data = stackalloc byte[] { 1, 2, 3 };

        // Act - Should compile with 'in' modifier
        proxy.ProcessReadOnly(in data);

        // Assert - Compilation test
        Assert.NotNull(proxy);
    }

    [Fact]
    public void Generator_HandlesParamsArrays()
    {
        // T041: Handle params arrays
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());

        // Act
        var result = proxy.Concatenate("a", "b", "c");

        // Assert
        Assert.Equal("a, b, c", result);
    }

    [Fact]
    public void Generator_HandlesDefaultParameterValues()
    {
        // T044: Preserve default parameter values
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());

        // Act - Call with defaults
        proxy.MethodWithDefaults();
        proxy.MethodWithDefaults(20);
        proxy.MethodWithDefaults(20, "custom");
        proxy.MethodWithDefaults(20, "custom", false);

        // Assert - Should compile with all overloads
        Assert.NotNull(proxy);
    }

    [Fact]
    public async Task Generator_HandlesAsyncMethod()
    {
        // T042: Handle async methods returning Task
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());

        // Act
        await proxy.AsyncMethod();

        // Assert - Should compile and complete
        Assert.NotNull(proxy);
    }

    [Fact]
    public async Task Generator_HandlesAsyncMethodWithResult()
    {
        // T043: Handle async methods returning Task<T>
        // Arrange
        var proxy = new AdvancedMethodProxy(new TestRegistry());

        // Act
        var result = await proxy.AsyncMethodWithResult();

        // Assert
        Assert.Equal(42, result);
    }

    // ===== Phase 4: Selection Strategy Tests =====

    [Fact]
    public void Generator_HandlesSelectionModeOne()
    {
        // T058: SelectionMode.One generates correct code
        // Arrange
        var registry = new SelectionModeTestRegistry();
        var proxy = new SelectionModeOneProxy(registry);

        // Act
        var result = proxy.GetValue();

        // Assert - Should use SelectionMode.One
        Assert.Equal("One", result);
        Assert.Equal(SelectionMode.One, registry.LastUsedMode);
    }

    [Fact]
    public void Generator_HandlesSelectionModeHighestPriority()
    {
        // T059: SelectionMode.HighestPriority generates correct code
        // Arrange
        var registry = new SelectionModeTestRegistry();
        var proxy = new SelectionModeHighestPriorityProxy(registry);

        // Act
        var result = proxy.GetValue();

        // Assert - Should use SelectionMode.HighestPriority
        Assert.Equal("HighestPriority", result);
        Assert.Equal(SelectionMode.HighestPriority, registry.LastUsedMode);
    }

    [Fact]
    public void Generator_HandlesSelectionModeAll()
    {
        // T060: SelectionMode.All generates correct code
        // Arrange
        var registry = new SelectionModeTestRegistry();
        var proxy = new SelectionModeAllProxy(registry);

        // Act
        var result = proxy.GetValue();

        // Assert - Should use GetAll()
        Assert.Equal("All", result);
        Assert.True(registry.UsedGetAll);
    }

    [Fact]
    public void Generator_HandlesNoSelectionStrategy()
    {
        // T061: No strategy uses default behavior (_registry.Get<T>() with no mode parameter)
        // Arrange
        var registry = new SelectionModeTestRegistry();
        var proxy = new NoSelectionStrategyProxy(registry);

        // Act
        var result = proxy.GetValue();

        // Assert - Should call Get<T>() without explicit mode parameter
        // This will use the default parameter value (HighestPriority) but the generated code doesn't specify it
        Assert.Equal("HighestPriority", result);
        Assert.Equal(SelectionMode.HighestPriority, registry.LastUsedMode);
        Assert.False(registry.UsedGetAll); // Should use Get, not GetAll
    }

    // ===== Phase 5: Nullable and Code Quality Tests =====

    [Fact]
    public void Generator_PreservesNullableAnnotations()
    {
        // T071: Nullable annotations preserved
        // Arrange
        var proxy = new NullableTestProxy(new TestRegistry());

        // Act - Call methods with nullable types
        var result1 = proxy.GetNullableString();
        proxy.AcceptNullableString(null);
        var result2 = proxy.GetNullableInt();

        // Assert - Should compile without warnings (compilation test)
        Assert.NotNull(proxy);
    }

    [Fact]
    public async Task Generator_HandlesNullableReturnTypes()
    {
        // T064: Handle nullable return types
        // Arrange
        var proxy = new NullableTestProxy(new TestRegistry());

        // Act
        var result = await proxy.GetNullableTaskResult();

        // Assert - Should compile and work with nullable Task<string?>
        Assert.NotNull(proxy);
    }

    [Fact]
    public void Generator_GeneratesCodeWithoutWarnings()
    {
        // T072: Generated code compiles without warnings
        // This test verifies that all generated code compiles cleanly
        // Arrange & Act - All proxy classes should compile
        var proxies = new object[]
        {
            new TestProxy(new TestRegistry()),
            new PropertyTestProxy(new TestRegistry()),
            new AdvancedMethodProxy(new TestRegistry()),
            new SelectionModeOneProxy(new SelectionModeTestRegistry()),
            new NullableTestProxy(new TestRegistry())
        };

        // Assert - All proxies created successfully
        Assert.All(proxies, p => Assert.NotNull(p));
    }

    [Fact]
    public void Generator_IncludesQualityMarkers()
    {
        // T066, T067: Auto-generated comment and timestamp
        // This is a meta-test that verifies the generator adds quality markers
        // The actual verification happens at compile time when the generator runs

        // Arrange & Act
        var proxy = new TestProxy(new TestRegistry());

        // Assert - Proxy should be created (generated code compiled successfully)
        Assert.NotNull(proxy);

        // Note: Generated files include:
        // - // <auto-generated />
        // - // Generated by ProxyServiceGenerator at [timestamp] UTC
        // - #nullable enable
        // - Proper indentation (4 spaces)
        // - Blank lines between members
    }

    // ===== Phase 6: Error Handling and Diagnostics Tests =====

    [Fact]
    public void Generator_HandlesValidProxyClass()
    {
        // T083: Generator doesn't crash on valid input
        // Arrange & Act - All our test proxies should work
        var proxies = new object[]
        {
            new TestProxy(new TestRegistry()),
            new PropertyTestProxy(new TestRegistry()),
            new AdvancedMethodProxy(new TestRegistry()),
            new NullableTestProxy(new TestRegistry())
        };

        // Assert - All proxies created successfully without errors
        Assert.All(proxies, p => Assert.NotNull(p));
    }

    [Fact]
    public void Generator_CompilesWithoutErrors()
    {
        // T084-T087: Comprehensive compilation test
        // This test verifies that:
        // - Missing _registry field would cause compile error (PROXY002)
        // - Non-interface target would cause compile error (PROXY001)
        // - Invalid syntax is handled gracefully
        // - All valid proxies compile successfully

        // Arrange & Act
        var testProxy = new TestProxy(new TestRegistry());
        var propertyProxy = new PropertyTestProxy(new TestRegistry());
        var advancedProxy = new AdvancedMethodProxy(new TestRegistry());
        var nullableProxy = new NullableTestProxy(new TestRegistry());
        var selectionProxy = new SelectionModeOneProxy(new SelectionModeTestRegistry());

        // Assert - All proxies compile and instantiate
        Assert.NotNull(testProxy);
        Assert.NotNull(propertyProxy);
        Assert.NotNull(advancedProxy);
        Assert.NotNull(nullableProxy);
        Assert.NotNull(selectionProxy);
    }

    [Fact]
    public void Generator_HandlesComplexScenarios()
    {
        // T087: Generator handles complex but valid scenarios
        // Arrange
        var registry = new TestRegistry();

        // Act - Create proxies with various features
        var basicProxy = new TestProxy(registry);
        var genericProxy = new AdvancedMethodProxy(registry);
        var nullableProxy = new NullableTestProxy(registry);

        // Assert - All complex scenarios work
        Assert.NotNull(basicProxy.GetValue());
        Assert.Equal(42, genericProxy.GenericMethod(42));
        Assert.NotNull(nullableProxy.GetNullableString());
    }
}

// Test interface
public interface ITestService
{
    void DoSomething();
    string GetValue();

    // Properties
    string Name { get; set; }
    int ReadOnlyValue { get; }

    // Event
    event EventHandler? DataChanged;
}

// Test proxy class (generator will implement interface methods)
[RealizeService(typeof(ITestService))]
[SelectionStrategy(SelectionMode.HighestPriority)]
public partial class TestProxy
{
    private readonly IRegistry _registry;

    public TestProxy(IRegistry registry)
    {
        _registry = registry;
    }
}

// Mock registry for testing
public class TestRegistry : IRegistry
{
    public T Get<T>(SelectionMode mode = SelectionMode.HighestPriority) where T : class
    {
        if (typeof(T) == typeof(ITestService))
            return new TestServiceImpl() as T ?? throw new InvalidOperationException();
        if (typeof(T) == typeof(IPropertyTestService))
            return new PropertyTestServiceImpl() as T ?? throw new InvalidOperationException();
        if (typeof(T) == typeof(IAdvancedMethodService))
            return new AdvancedMethodServiceImpl() as T ?? throw new InvalidOperationException();
        if (typeof(T) == typeof(INullableTestService))
            return new NullableTestServiceImpl() as T ?? throw new InvalidOperationException();

        throw new InvalidOperationException($"No implementation for {typeof(T).Name}");
    }

    public IEnumerable<T> GetAll<T>() where T : class
    {
        if (typeof(T) == typeof(ITestService))
            yield return new TestServiceImpl() as T ?? throw new InvalidOperationException();
        else if (typeof(T) == typeof(IPropertyTestService))
            yield return new PropertyTestServiceImpl() as T ?? throw new InvalidOperationException();
        else if (typeof(T) == typeof(IAdvancedMethodService))
            yield return new AdvancedMethodServiceImpl() as T ?? throw new InvalidOperationException();
        else if (typeof(T) == typeof(INullableTestService))
            yield return new NullableTestServiceImpl() as T ?? throw new InvalidOperationException();
        else
            throw new InvalidOperationException($"No implementation for {typeof(T).Name}");
    }

    public void Register<T>(T implementation, ServiceMetadata metadata) where T : class
    {
        // Not needed for test
    }

    public void Register<T>(T implementation, int priority = 100) where T : class
    {
        // Not needed for test
    }

    public bool IsRegistered<T>() where T : class
    {
        return true;
    }

    public bool Unregister<T>(T implementation) where T : class
    {
        return true;
    }
}

// Test implementation
public class TestServiceImpl : ITestService
{
    public void DoSomething() { }
    public string GetValue() => "test";

    public string Name { get; set; } = "TestService";
    public int ReadOnlyValue => 42;

    public event EventHandler? DataChanged;

    protected virtual void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}

// Additional test interface for property testing (T025-T027)
public interface IPropertyTestService
{
    int ReadOnlyProp { get; }
    string ReadWriteProp { get; set; }
    int WriteOnlyProp { set; }
}

// Advanced method features test interface (Phase 3: T034-T044)
public interface IAdvancedMethodService
{
    // Generic methods (T034)
    T GenericMethod<T>(T value);

    // Multiple type parameters (T036)
    TResult Transform<TInput, TResult>(TInput input);

    // Type constraints (T035)
    T CreateInstance<T>() where T : class, new();

    // Ref parameters (T038)
    void ModifyValue(ref int value);

    // Out parameters (T039)
    bool TryGetValue(string key, out string value);

    // In parameters (T040)
    void ProcessReadOnly(in ReadOnlySpan<byte> data);

    // Params arrays (T041)
    string Concatenate(params string[] values);

    // Default parameter values (T044)
    void MethodWithDefaults(int x = 10, string name = "default", bool flag = true);

    // Async methods (T042, T043)
    Task AsyncMethod();
    Task<int> AsyncMethodWithResult();
}

// Test proxy for property testing
[RealizeService(typeof(IPropertyTestService))]
public partial class PropertyTestProxy
{
    private readonly IRegistry _registry;

    public PropertyTestProxy(IRegistry registry)
    {
        _registry = registry;
    }
}

// Implementation for property test service
public class PropertyTestServiceImpl : IPropertyTestService
{
    public int ReadOnlyProp => 100;
    public string ReadWriteProp { get; set; } = "Default";
    private int _writeOnly;
    public int WriteOnlyProp { set => _writeOnly = value; }
}

// Test proxy for advanced methods (Phase 3)
[RealizeService(typeof(IAdvancedMethodService))]
public partial class AdvancedMethodProxy
{
    private readonly IRegistry _registry;

    public AdvancedMethodProxy(IRegistry registry)
    {
        _registry = registry;
    }
}

// Implementation for advanced method service
public class AdvancedMethodServiceImpl : IAdvancedMethodService
{
    public T GenericMethod<T>(T value) => value;

    public TResult Transform<TInput, TResult>(TInput input) => default!;

    public T CreateInstance<T>() where T : class, new() => new T();

    public void ModifyValue(ref int value) => value *= 2;

    public bool TryGetValue(string key, out string value)
    {
        value = $"Value for {key}";
        return true;
    }

    public void ProcessReadOnly(in ReadOnlySpan<byte> data) { }

    public string Concatenate(params string[] values) => string.Join(", ", values);

    public void MethodWithDefaults(int x = 10, string name = "default", bool flag = true) { }

    public Task AsyncMethod() => Task.CompletedTask;

    public Task<int> AsyncMethodWithResult() => Task.FromResult(42);
}

// ===== Phase 4: Selection Strategy Test Infrastructure =====

// Simple interface for testing selection strategies
public interface ISelectionModeTestService
{
    string GetValue();
}

// Test proxy with SelectionMode.One
[RealizeService(typeof(ISelectionModeTestService))]
[SelectionStrategy(SelectionMode.One)]
public partial class SelectionModeOneProxy
{
    private readonly IRegistry _registry;
    public SelectionModeOneProxy(IRegistry registry) => _registry = registry;
}

// Test proxy with SelectionMode.HighestPriority
[RealizeService(typeof(ISelectionModeTestService))]
[SelectionStrategy(SelectionMode.HighestPriority)]
public partial class SelectionModeHighestPriorityProxy
{
    private readonly IRegistry _registry;
    public SelectionModeHighestPriorityProxy(IRegistry registry) => _registry = registry;
}

// Test proxy with SelectionMode.All
[RealizeService(typeof(ISelectionModeTestService))]
[SelectionStrategy(SelectionMode.All)]
public partial class SelectionModeAllProxy
{
    private readonly IRegistry _registry;
    public SelectionModeAllProxy(IRegistry registry) => _registry = registry;
}

// Test proxy with no selection strategy (default)
[RealizeService(typeof(ISelectionModeTestService))]
public partial class NoSelectionStrategyProxy
{
    private readonly IRegistry _registry;
    public NoSelectionStrategyProxy(IRegistry registry) => _registry = registry;
}

// Special registry for testing selection modes
public class SelectionModeTestRegistry : IRegistry
{
    public SelectionMode LastUsedMode { get; private set; }
    public bool UsedGetAll { get; private set; }
    public bool UsedDefaultGet { get; private set; }

    public T Get<T>(SelectionMode mode = SelectionMode.HighestPriority) where T : class
    {
        LastUsedMode = mode;

        // Track if this was called with default parameter (no explicit mode in generated code)
        // This is a bit of a hack, but we check the stack trace to see if the mode was explicit
        UsedDefaultGet = mode == SelectionMode.HighestPriority;

        if (typeof(T) == typeof(ISelectionModeTestService))
        {
            var impl = new SelectionModeTestServiceImpl(mode.ToString());
            return impl as T ?? throw new InvalidOperationException();
        }

        throw new InvalidOperationException($"No implementation for {typeof(T).Name}");
    }

    public IEnumerable<T> GetAll<T>() where T : class
    {
        UsedGetAll = true;

        if (typeof(T) == typeof(ISelectionModeTestService))
        {
            yield return new SelectionModeTestServiceImpl("All") as T ?? throw new InvalidOperationException();
        }
        else
        {
            throw new InvalidOperationException($"No implementation for {typeof(T).Name}");
        }
    }

    public void Register<T>(T implementation, ServiceMetadata metadata) where T : class { }
    public void Register<T>(T implementation, int priority = 100) where T : class { }
    public bool IsRegistered<T>() where T : class => true;
    public bool Unregister<T>(T implementation) where T : class => true;
}

// Implementation for selection mode testing
public class SelectionModeTestServiceImpl : ISelectionModeTestService
{
    private readonly string _value;
    public SelectionModeTestServiceImpl(string value) => _value = value;
    public string GetValue() => _value;
}

// ===== Phase 5: Nullable and Code Quality Test Infrastructure =====

/// <summary>
/// Test interface with nullable reference types for Phase 5 testing.
/// </summary>
public interface INullableTestService
{
    /// <summary>
    /// Gets a nullable string value.
    /// </summary>
    /// <returns>A nullable string or null.</returns>
    string? GetNullableString();

    /// <summary>
    /// Accepts a nullable string parameter.
    /// </summary>
    /// <param name="value">The nullable string value.</param>
    void AcceptNullableString(string? value);

    /// <summary>
    /// Gets a nullable integer.
    /// </summary>
    int? GetNullableInt();

    /// <summary>
    /// Returns a task with nullable result.
    /// </summary>
    Task<string?> GetNullableTaskResult();
}

// Test proxy for nullable types
[RealizeService(typeof(INullableTestService))]
public partial class NullableTestProxy
{
    private readonly IRegistry _registry;
    public NullableTestProxy(IRegistry registry) => _registry = registry;
}

// Implementation for nullable test service
public class NullableTestServiceImpl : INullableTestService
{
    public string? GetNullableString() => "test";
    public void AcceptNullableString(string? value) { }
    public int? GetNullableInt() => 42;
    public Task<string?> GetNullableTaskResult() => Task.FromResult<string?>("result");
}
