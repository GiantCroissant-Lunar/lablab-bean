namespace LablabBean.DependencyInjection.Tests.Unit;

public class OpenGenericsTests
{
    public interface IRepository<T> {}
    public class Repository<T> : IRepository<T> {}

    [Fact]
    public void OpenGeneric_Resolves_FromLocal()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));

        var provider = services.BuildHierarchicalServiceProvider("Root");

        // Act
        var repoInt = provider.GetService<IRepository<int>>();
        var repoString = provider.GetService<IRepository<string>>();

        // Assert
        repoInt.Should().NotBeNull();
        repoString.Should().NotBeNull();
        repoInt.Should().NotBeSameAs(repoString);
    }

    [Fact]
    public void OpenGeneric_Resolves_FromParent()
    {
        // Arrange
        var parentServices = new ServiceCollection();
        parentServices.AddSingleton(typeof(IRepository<>), typeof(Repository<>));
        var parent = parentServices.BuildHierarchicalServiceProvider("Parent");

        var child = parent.CreateChildContainer(_ => { }, "Child");

        // Act
        var repo = child.GetService<IRepository<Guid>>();

        // Assert
        repo.Should().NotBeNull();
    }
}
