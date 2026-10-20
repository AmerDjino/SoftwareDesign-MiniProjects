using Expenses.Business;
using Expenses.Presentation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Expenses.Tests;

public sealed class ExpenseEndpointsTests
{
    [Fact]
    public async Task PostExpense_WhenBusinessValidationFails_Returns400ValidationProblem()
    {
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero));
        var service = new ExpenseService(new SpyExpenseRepository(), clock);
        // What the handler receives for the body {"category": "Party"}: missing JSON fields bind to null, 0 and default.
        var request = new CreateExpenseRequest(Description: null, Amount: 0m, Category: "Party", IncurredOn: default);

        var result = await ExpenseEndpoints.PostExpense(request, service, CancellationToken.None);

        var problem = Assert.IsType<ValidationProblem>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.Equal(new[] { "amount", "category", "description", "incurredOn" }, problem.ProblemDetails.Errors.Keys.Order(StringComparer.Ordinal));
    }
}
