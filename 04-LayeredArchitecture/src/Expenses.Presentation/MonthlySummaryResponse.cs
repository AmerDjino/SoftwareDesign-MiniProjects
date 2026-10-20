namespace Expenses.Presentation;

public sealed record MonthlySummaryResponse(int Year, int Month, IReadOnlyList<CategoryTotalResponse> Categories, decimal GrandTotal);

public sealed record CategoryTotalResponse(string Category, decimal Total, int Count);
