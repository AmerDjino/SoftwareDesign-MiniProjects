using Inventory.Abstractions;
using Inventory.ReadSide;

namespace Inventory.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void QueryHandlers_DependOnlyOnTheReadModel()
    {
        Type[] allowedDependencies = [typeof(IStockLevelReader)];

        var queryHandlers = typeof(IQueryHandler<,>).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.GetInterfaces().Any(IsQueryHandler))
            .ToList();
        var dependencies =
            from handler in queryHandlers
            from constructor in handler.GetConstructors()
            from parameter in constructor.GetParameters()
            select (Handler: handler.Name, Dependency: parameter.ParameterType);

        Assert.NotEmpty(queryHandlers);
        Assert.All(dependencies, dependency => Assert.Contains(dependency.Dependency, allowedDependencies));
    }

    private static bool IsQueryHandler(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryHandler<,>);
}
