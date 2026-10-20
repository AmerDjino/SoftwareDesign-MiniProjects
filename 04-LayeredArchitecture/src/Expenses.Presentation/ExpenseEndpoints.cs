using Expenses.Business;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Expenses.Presentation;

public static class ExpenseEndpoints
{
    private const string BasePath = "/expenses";

    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var expenses = endpoints.MapGroup(BasePath);
        expenses.MapPost("", PostExpense);
        expenses.MapGet("/summary/{year:int}/{month:int:range(1,12)}", GetMonthlySummary);
        return endpoints;
    }

    public static async Task<Results<Created<ExpenseResponse>, ValidationProblem>> PostExpense(
        CreateExpenseRequest request, ExpenseService service, CancellationToken cancellationToken)
    {
        var result = await service.SubmitAsync(request.ToNewExpense(), cancellationToken);
        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(result.Errors.ToHttpErrors());
        }

        var response = result.Expense.ToResponse();
        return TypedResults.Created($"{BasePath}/{response.Id}", response);
    }

    public static async Task<Ok<MonthlySummaryResponse>> GetMonthlySummary(
        int year, int month, ExpenseService service, CancellationToken cancellationToken)
    {
        var summary = await service.GetMonthlySummaryAsync(year, month, cancellationToken);
        return TypedResults.Ok(summary.ToResponse());
    }
}
