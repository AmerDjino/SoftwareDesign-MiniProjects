using Expenses.Business;
using Expenses.DataAccess;

namespace Expenses.Tests;

public sealed class ExpenseServiceTests
{
    private static readonly FixedTimeProvider Clock = new(new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Submit_InvalidExpense_ReturnsValidationErrorsAndStoresNothing()
    {
        var repository = new SpyExpenseRepository();
        var service = new ExpenseService(repository, Clock);
        var invalid = new NewExpense(
            Description: new string('x', 201),
            Amount: 5_000.01m,
            Category: (ExpenseCategory)99,
            IncurredOn: new DateOnly(2026, 10, 8));

        var result = await service.SubmitAsync(invalid);

        Assert.False(result.Succeeded);
        Assert.Equal(new[] { "Amount", "Category", "Description", "IncurredOn" }, result.Errors.Keys.Order(StringComparer.Ordinal));
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task GetMonthlySummary_TotalsPerCategoryForRequestedMonthOnly()
    {
        var service = new ExpenseService(new InMemoryExpenseRepository(), Clock);
        await SubmitValidAsync(service, "Train to Mostar", 120.00m, ExpenseCategory.Travel, new DateOnly(2026, 10, 1));
        await SubmitValidAsync(service, "Team lunch", 45.25m, ExpenseCategory.Meals, new DateOnly(2026, 10, 2));
        await SubmitValidAsync(service, "C# course", 350.00m, ExpenseCategory.Training, new DateOnly(2026, 10, 6));
        await SubmitValidAsync(service, "Taxi to the airport", 80.50m, ExpenseCategory.Travel, new DateOnly(2026, 10, 7));
        await SubmitValidAsync(service, "Monitor", 300.00m, ExpenseCategory.Office, new DateOnly(2026, 9, 30));
        await SubmitValidAsync(service, "Conference ticket", 900.00m, ExpenseCategory.Training, new DateOnly(2025, 10, 15));

        var summary = await service.GetMonthlySummaryAsync(2026, 10);

        CategoryTotal[] expected =
        [
            new(ExpenseCategory.Training, 350.00m, 1),
            new(ExpenseCategory.Travel, 200.50m, 2),
            new(ExpenseCategory.Meals, 45.25m, 1),
        ];
        Assert.Equal(expected, summary.Categories);
        Assert.Equal(595.75m, summary.GrandTotal);
    }

    private static async Task SubmitValidAsync(
        ExpenseService service, string description, decimal amount, ExpenseCategory category, DateOnly incurredOn)
    {
        var result = await service.SubmitAsync(new NewExpense(description, amount, category, incurredOn));
        Assert.True(result.Succeeded, $"Arrange failed: '{description}' was rejected.");
    }
}
