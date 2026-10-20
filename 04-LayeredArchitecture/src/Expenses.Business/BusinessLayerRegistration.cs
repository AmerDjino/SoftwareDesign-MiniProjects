using Expenses.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Expenses.Business;

public static class BusinessLayerRegistration
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        services.AddDataAccessLayer();
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ExpenseService>();
        return services;
    }
}
