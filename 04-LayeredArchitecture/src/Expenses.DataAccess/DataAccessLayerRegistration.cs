using Microsoft.Extensions.DependencyInjection;

namespace Expenses.DataAccess;

public static class DataAccessLayerRegistration
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services) =>
        services.AddSingleton<IExpenseRepository, InMemoryExpenseRepository>();
}
