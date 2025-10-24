namespace LablabBean.DependencyInjection.Tests.Unit;

public class ServiceProviderIsServiceTests
{
    public interface IFoo {}
    public class Foo : IFoo {}

    public interface IBar {}
    public class Bar : IBar {}

    [Fact]
    public void IsService_ReturnsTrue_ForLocalRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IFoo, Foo>();

        var container = services.BuildHierarchicalServiceProvider("Test");

        // Act
        var isp = (IServiceProviderIsService)container;

        // Assert
        isp.IsService(typeof(IFoo)).Should().BeTrue();
        isp.IsService(typeof(IBar)).Should().BeFalse();
    }

    [Fact]
    public void IsService_ReturnsTrue_WhenRegisteredInParent()
    {
        // Arrange parent with IFoo
        var parentServices = new ServiceCollection();
        parentServices.AddSingleton<IFoo, Foo>();
        var parent = parentServices.BuildHierarchicalServiceProvider("Parent");

        // Child has no local registration for IFoo
        var child = parent.CreateChildContainer(_ => { }, "Child");

        // Act
        var ispChild = (IServiceProviderIsService)child;

        // Assert
        ispChild.IsService(typeof(IFoo)).Should().BeTrue();
        ispChild.IsService(typeof(IBar)).Should().BeFalse();
    }
}
