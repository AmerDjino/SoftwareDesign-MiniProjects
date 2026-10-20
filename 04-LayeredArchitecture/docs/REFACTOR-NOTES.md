# 04 — Layered Architecture: Refactor Notes

How the first passing version was improved without changing its behaviour, and what is left for a manual pass.

## Starting point (first green)

The first green version was the simplest code that made the five tests pass:

- **Data Access** was already in its final shape: `InMemoryExpenseRepository` (a `List` guarded by a
  `System.Threading.Lock`) and `AddDataAccessLayer()`.
- **Business**: `ExpenseService.SubmitAsync` was one 49-line method. It checked all four rules inline, built the
  `ExpenseRecord`, saved it and assembled the returned `Expense`. `GetMonthlySummaryAsync` grouped the storage
  records directly. (The stub had used a normal constructor, because an unread primary-constructor parameter is
  error CS9113 under warnings-as-errors. First green switched to a primary constructor.)
- **Presentation**: `ExpenseEndpoints.PostExpense` did all of its mapping inline, and `Program.cs` held both route
  templates.

Result: `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 32 ms - Expenses.Tests.dll (net9.0)`.
Running the app (`dotnet run`, then curl) confirmed the parts no unit test covers: `201 Created` with a `Location`
header, the summary JSON, and `404` for month 13.

## Refactorings

Each step was made separately, and `dotnet test` stayed green after every one. Test code was not touched; the
test files are byte-identical to first green, checked by file hash.

### R1. Validation rules moved out of `ExpenseService`

- **Smell:** a long method with three jobs: checking the rules, translating to a storage record, and saving it.
- **Reason:** single responsibility inside the Business layer. The service orchestrates the use case; the rules
  live in one class you can read on its own. The validator is a pure function of `(expense, today)`, and the
  service still owns the clock.
- **Change:** new `internal static class ExpenseValidator`. The code moved first verbatim (tests green), then R2.

Before (first green, abridged):
```csharp
public async Task<SubmitExpenseResult> SubmitAsync(NewExpense expense, CancellationToken cancellationToken = default)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(expense.Description))
    {
        errors["Description"] = ["Description is required."];
    }
    // ... 30 more lines for Description length, Amount, Category and IncurredOn ...

    if (errors.Count > 0)
    {
        return SubmitExpenseResult.Invalid(errors);
    }

    var record = new ExpenseRecord(
        Guid.NewGuid(), expense.Description, expense.Amount, expense.Category.ToString(), expense.IncurredOn);
    await repository.AddAsync(record, cancellationToken);

    return SubmitExpenseResult.Success(
        new Expense(record.Id, record.Description, record.Amount, expense.Category, record.IncurredOn));
}
```

After (together with R3; the method went from 49 to 12 lines):
```csharp
public async Task<SubmitExpenseResult> SubmitAsync(NewExpense expense, CancellationToken cancellationToken = default)
{
    var errors = ExpenseValidator.Validate(expense, Today());
    if (errors.Count > 0)
    {
        return SubmitExpenseResult.Invalid(errors);
    }

    var record = expense.ToRecord(Guid.NewGuid());
    await repository.AddAsync(record, cancellationToken);
    return SubmitExpenseResult.Success(record.ToExpense());
}
```

### R2. Magic numbers and strings replaced

- **Smell:** `200` and `5000.00` each appeared twice, once in the check and once in the message. The category list
  in the message duplicated the enum. The error keys were string literals (`"IncurredOn"`) that *must* equal the
  `NewExpense` property names, because Presentation turns them into JSON names; nothing enforced that link.
- **Reason:** one source of truth for each fact, and a compiler-checked link between the error keys and the DTO.
  The messages are now built from the constants, so they are formatted with the invariant culture. With
  `bs-Latn-BA`, a plain `$"{5000.00m}"` renders as `5000,00`.
- **Change:** named constants, `nameof(NewExpense.X)` keys, category names taken from the enum.

Before:
```csharp
else if (expense.Amount > 5000.00m)
{
    errors["Amount"] = ["Amount must be at most 5000.00."];
}

if (!Enum.IsDefined(expense.Category))
{
    errors["Category"] = ["Category must be one of: Travel, Meals, Office, Training, Other."];
}
```

After:
```csharp
private const int MaxDescriptionLength = 200;
private const decimal MaxAmount = 5_000.00m;
// ...
else if (expense.Amount > MaxAmount)
{
    errors[nameof(NewExpense.Amount)] = [FormattableString.Invariant($"Amount must be at most {MaxAmount:0.00}.")];
}

if (!Enum.IsDefined(expense.Category))
{
    errors[nameof(NewExpense.Category)] =
        [$"Category must be one of: {string.Join(", ", Enum.GetNames<ExpenseCategory>())}."];
}
```

The messages are unchanged. Checked over HTTP with a 201-character description, 5000.01 and tomorrow's date.

### R3. One place for the Business ↔ Data Access mapping

- **Smell:** the storage decision "a category is saved as its name" (SPEC D-2) was spread over two methods:
  `ToString()` in `SubmitAsync` and `Enum.Parse` in `GetMonthlySummaryAsync`. The summary grouped storage records,
  so a business calculation depended on the storage format. The returned `Expense` was built half from the record
  and half from the DTO.
- **Reason:** AR-4 says the upper layer maps at the boundary; doing it in exactly one place keeps the storage
  format out of the business logic, which now runs on business models only.
- **Change:** new `internal static class ExpenseRecordMapping` with `ToRecord` and `ToExpense`.

Before:
```csharp
var categories = records
    .GroupBy(record => Enum.Parse<ExpenseCategory>(record.Category))
    .Select(group => new CategoryTotal(group.Key, group.Sum(record => record.Amount), group.Count()))
```

After:
```csharp
internal static class ExpenseRecordMapping
{
    public static ExpenseRecord ToRecord(this NewExpense expense, Guid id) =>
        new(id, expense.Description, expense.Amount, expense.Category.ToString(), expense.IncurredOn);

    public static Expense ToExpense(this ExpenseRecord record) =>
        new(record.Id, record.Description, record.Amount, Enum.Parse<ExpenseCategory>(record.Category), record.IncurredOn);
}

// in GetMonthlySummaryAsync
var categories = records
    .Select(record => record.ToExpense())
    .GroupBy(expense => expense.Category)
    .Select(group => new CategoryTotal(group.Key, group.Sum(expense => expense.Amount), group.Count()))
```

### R4. One place for the HTTP ↔ Business mapping

- **Smell:** `PostExpense` wrapped three translations around one business call. They were the request to
  `NewExpense` (including the names-only category rule, SPEC D-4, hidden in a lambda), the error keys to JSON
  names, and the `Expense` to the response. `GetMonthlySummary` had its own inline mapping.
- **Reason:** the same boundary rule as R3, at the upper boundary. A handler should show the flow
  *translate in → call Business → translate out*, and the D-4 and D-10 rules deserve named, documented methods.
- **Change:** new `internal static class ExpenseContractMapping` (`ToNewExpense`, `ToResponse`, `ToHttpErrors`,
  `ParseCategory`). It mirrors `ExpenseRecordMapping` one layer down. Both are `internal`, so no other layer
  can call them.

Before:
```csharp
var category = Enum.GetValues<ExpenseCategory>()
    .FirstOrDefault(value => string.Equals(value.ToString(), request.Category, StringComparison.OrdinalIgnoreCase));
var newExpense = new NewExpense(request.Description ?? "", request.Amount, category, request.IncurredOn);

var result = await service.SubmitAsync(newExpense, cancellationToken);

if (!result.Succeeded)
{
    var errors = result.Errors.ToDictionary(
        error => JsonNamingPolicy.CamelCase.ConvertName(error.Key),
        error => error.Value);
    return TypedResults.ValidationProblem(errors);
}

var expense = result.Expense;
var response = new ExpenseResponse(
    expense.Id, expense.Description, expense.Amount, expense.Category.ToString(), expense.IncurredOn);
return TypedResults.Created($"/expenses/{expense.Id}", response);
```

After:
```csharp
var result = await service.SubmitAsync(request.ToNewExpense(), cancellationToken);
if (!result.Succeeded)
{
    return TypedResults.ValidationProblem(result.Errors.ToHttpErrors());
}

var response = result.Expense.ToResponse();
return TypedResults.Created($"{BasePath}/{response.Id}", response);
```

### R5. Routes next to their handlers

- **Smell:** `Program.cs` knew every route template, and `/expenses` was written three times in two files
  (two templates plus the `Location` header).
- **Reason:** `Program.cs` is the composition root and should only compose (AR-3); the Presentation layer owns
  its URL space.
- **Change:** `MapExpenseEndpoints()` extension with a route group and one `BasePath` constant.

Before (`Program.cs`):
```csharp
app.MapPost("/expenses", ExpenseEndpoints.PostExpense);
app.MapGet("/expenses/summary/{year:int}/{month:int:range(1,12)}", ExpenseEndpoints.GetMonthlySummary);
```

After:
```csharp
// Program.cs
builder.Services.AddBusinessLayer();
var app = builder.Build();
app.MapExpenseEndpoints();

// ExpenseEndpoints.cs
public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder endpoints)
{
    var expenses = endpoints.MapGroup(BasePath);
    expenses.MapPost("", PostExpense);
    expenses.MapGet("/summary/{year:int}/{month:int:range(1,12)}", GetMonthlySummary);
    return endpoints;
}
```

### Considered and left alone

- `InMemoryExpenseRepository` and `SubmitExpenseResult`: no smell worth acting on.
- An `IExpenseService` interface: there is one implementation and the tests use the real service, so it would
  add a file and no value.
- `ToHttpErrors` still derives JSON names with `JsonNamingPolicy.CamelCase`. Since R2 the keys are `nameof`
  values, so this works as long as the HTTP contract and `NewExpense` use the same property names (see idea 4).

## Result

- `dotnet test`: `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 693 ms - Expenses.Tests.dll (net9.0)`.
- `dotnet build --no-incremental`: `0 Warning(s)`, `0 Error(s)`. A one-off build of a scratch copy with
  `EnforceCodeStyleInBuild` also found no unused usings (IDE0005) and no broken XML doc references.
- Public API: unchanged, except that `ExpenseEndpoints.MapExpenseEndpoints()` was added. The new classes
  `ExpenseValidator`, `ExpenseRecordMapping` and `ExpenseContractMapping` are `internal`.
- HTTP behaviour: the smoke test before and after the refactor gave the same status codes, headers, bodies and
  messages.
- Size: `ExpenseService.cs` went from 70 to 36 lines; the rules and mappings now sit in their own small files.

## Guard check: can the architecture tests fail?

The architecture cases were already green against the stubs, because no dependency existed yet that could break
a rule. A test you have never seen fail proves little, so after the refactor I broke the rules on purpose in a
scratch copy of the solution (the repository was not touched):

1. Added `builder.Services.AddDataAccessLayer();` to `Program.cs`. It **compiled**, because project references are
   transitive. The tests then failed:
   `Assert.DoesNotContain() Failure: Item found in collection`, `Collection: ["Business", "DataAccess"]`,
   `Found: "DataAccess"`, and the summary was
   `Failed!  - Failed:     1, Passed:     6, Skipped:     0, Total:     7`.
2. Added a project reference from Data Access to Business. Restore failed before anything compiled:
   `error MSB4006: There is a circular dependency in the target dependency graph`. As long as the downward chain
   of project references exists, the build itself already blocks upward project references. The
   `Layers_DoNotReferenceLayersAboveThem` cases mostly state the rule in executable form; they would only catch
   an upward dependency that MSBuild cannot see, such as a reference to a built DLL or a package.

## Not done: ideas for a further manual pass

1. **Let the compiler enforce AR-1 as well.** Add `<DisableTransitiveProjectReferences>true</DisableTransitiveProjectReferences>`
   to `Expenses.Presentation.csproj`. In the scratch copy the app still built, ran (`201`/`200`) and passed all
   tests, and the guard-check change no longer compiled: `using Expenses.DataAccess;` failed with CS0234
   (namespace not found). Decide whether you want both guards or one, and write down why.
2. **Add `GET /expenses/{id}`**, so the `Location` header resolves (SPEC D-9). You need `GetByIdAsync` on the
   repository, a service method returning `Expense?` and a handler returning `Results<Ok<ExpenseResponse>, NotFound>`.
   Notice how one small feature touches all three layers.
3. **Try the Clean/Onion inversion on a branch.** Move `IExpenseRepository` and `ExpenseRecord` into Business,
   make Data Access reference Business and implement the interface. The `DataAccess → Business` case of
   `Layers_DoNotReferenceLayersAboveThem` will fail: classic layering and Clean architecture disagree on exactly
   that edge. Then adjust the rule to the style you pick.
4. **Make the error-key translation explicit.** Rename `CreateExpenseRequest.IncurredOn` to `Date` and watch the
   `incurredOn` key stop matching anything the client sent. Replace the camelCase conversion in `ToHttpErrors`
   with an explicit map from `NewExpense` property names to JSON names.

## Lessons learned

- Strict layering is a rule about *compiled* dependencies, and SDK-style project references are transitive: the
  compiler let `Program.cs` call `AddDataAccessLayer()`. Without a test (or `DisableTransitiveProjectReferences`)
  the rule is only a convention.
- MSBuild rejects reference cycles, so in a straight chain of project references the upward rules come for free.
  The skip-layer rule is the one that really needs a guard. `GetReferencedAssemblies()` lists only the assemblies
  the compiled code uses, which is exactly what that rule is about.
- The per-layer DI entry point is what makes strict layering workable. If `AddBusinessLayer()` did not call
  `AddDataAccessLayer()`, `Program.cs` would have to register the repository itself, which is a skip-layer
  dependency.
- Mapping is the main running cost: one expense has three models and two mapping classes. In exchange, the
  storage format (category as text) and the HTTP format (camelCase keys, category names) never reach the rules.
- The classic weakness is visible in one line: `ExpenseService`'s constructor takes `IExpenseRepository` from the
  layer below, so Business cannot compile without Data Access. For a service this size that is acceptable; it is
  the edge Clean/Onion would invert.
