# Refactor notes — 03 Repository Pattern

What first green looked like, what was refactored and why, what was deliberately left alone, and what is left for a manual pass. Every result line is copied from `dotnet test`.

## Starting point

### How first green was reached

1. **API stubs.** All 9 cases failed with `NotImplementedException`:
   `Failed!  - Failed:     9, Passed:     0, Skipped:     0, Total:     9, Duration: 625 ms - Invoicing.Tests.dll (net9.0)`
2. **First implementation, with the obvious fake.** `Invoice`, `InvoicePaymentService`, `InvoicingDbContext` and `EfInvoiceRepository` were written much as they are now. The in-memory repository was the version everyone writes first: a dictionary that holds the caller's own objects.

   ```csharp
   public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
       Task.FromResult(store.Invoices.GetValueOrDefault(id));

   public Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
   {
       store.Invoices.Add(invoice.Id, invoice);
       return Task.CompletedTask;
   }
   ```

   Result: `Failed!  - Failed:     1, Passed:     8, Skipped:     0, Total:     9, Duration: 7 s - Invoicing.Tests.dll (net9.0)`. Only `InMemoryInvoiceRepositoryTests.UpdateAsync_PersistsChanges_AndUnsavedChangesAreNotVisible` failed. Paying the freshly loaded invoice threw `Invoice INV-001 is already paid.`, because "loaded" was the very object the test had passed to `AddAsync` and marked paid without saving. The same test passed for EF Core. The contract suite caught the fake-repository trap on its first run.
3. **Experiment (reverted).** With that fake in place, I deleted the `UpdateAsync` call from `InvoicePaymentService`. All three service test cases still passed: a service that never saves looked correct.
4. **Fix, first green.** The repository copied invoices on the way in and on the way out through a private `Copy` helper:
   `Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 1 s - Invoicing.Tests.dll (net9.0)`
   Repeating the experiment now failed `MarkPaidAsync_UnpaidInvoice_MarksItPaidAndPersistsIt` (`Expected: 10/7/2026, Actual: null`), which is what the test is for.

### What first green looked like

- `Invoicing`: `Invoice`, `IInvoiceRepository`, `InvoicePaymentService` and `PaymentResult`, already in their final form.
- `InMemoryInvoiceStore` held a `Dictionary<Guid, Invoice>`; `InMemoryInvoiceRepository` (49 lines) called `Copy(...)` in all five methods.
- `EfInvoiceRepository` started each of its three queries with `db.Invoices.AsNoTracking()`.

## Refactorings

### 1. The in-memory store keeps immutable snapshots, not entities

**Smell.** The store held live, mutable `Invoice` objects, so the fake was correct only as long as every method remembered to call `Copy`. A reverted experiment showed how fragile that was: with the `Copy` calls in `GetByNumberAsync`, `ListOverdueAsync` and `UpdateAsync` deleted, all 9 tests still passed (`Passed!  - Failed:     0, Passed:     9, ...`). Three of the five copies that protect against the trap were guarded by nothing. Only the copies in `AddAsync` and `GetByIdAsync` were guarded: deleting either one on its own fails AC-3.

**Principle.** Make the mistake impossible to write, instead of relying on discipline and on tests that cannot cover every path.

**Change.** A new internal record, `InvoiceSnapshot`, is the stored form of an invoice. The store holds snapshots, and the repository converts at the boundary with `InvoiceSnapshot.Of(invoice)` and `snapshot.ToInvoice()`. Handing out a stored object, or storing a caller's object, is now a compile error, because a snapshot is not an `Invoice`.

Before:

```csharp
// InMemoryInvoiceStore
internal Dictionary<Guid, Invoice> Invoices { get; } = [];

// InMemoryInvoiceRepository
public Task<Invoice?> GetByNumberAsync(string number, CancellationToken cancellationToken = default)
{
    var stored = store.Invoices.Values.FirstOrDefault(invoice => invoice.Number == number);
    return Task.FromResult(stored is null ? null : Copy(stored));
}

private static Invoice Copy(Invoice invoice)
{
    var copy = new Invoice(invoice.Id, invoice.Number, invoice.CustomerName, invoice.Amount, invoice.DueDate);
    if (invoice.PaidOn is { } paidOn)
    {
        copy.MarkPaid(paidOn);
    }

    return copy;
}
```

After:

```csharp
// InvoiceSnapshot
internal sealed record InvoiceSnapshot(Guid Id, string Number, string CustomerName, decimal Amount, DateOnly DueDate, DateOnly? PaidOn)
{
    public static InvoiceSnapshot Of(Invoice invoice) =>
        new(invoice.Id, invoice.Number, invoice.CustomerName, invoice.Amount, invoice.DueDate, invoice.PaidOn);

    public Invoice ToInvoice()
    {
        var invoice = new Invoice(Id, Number, CustomerName, Amount, DueDate);
        if (PaidOn is { } paidOn)
        {
            invoice.MarkPaid(paidOn);
        }

        return invoice;
    }
}

// InMemoryInvoiceStore
internal Dictionary<Guid, InvoiceSnapshot> Invoices { get; } = [];

// InMemoryInvoiceRepository
public Task<Invoice?> GetByNumberAsync(string number, CancellationToken cancellationToken = default) =>
    Task.FromResult(store.Invoices.Values.FirstOrDefault(snapshot => snapshot.Number == number)?.ToInvoice());
```

The repository went from 49 to 32 lines, both `Get` methods became one-liners, and rebuilding an invoice from stored data lives in one place. Rebuilding still replays `MarkPaid` (see idea 4).

### 2. One place for "reads are detached" in the EF Core repository

**Smell.** Each of the three queries began with `db.Invoices.AsNoTracking()`. FR-5 depends on it: a tracking query returns the instance the same context is still tracking from an earlier `AddAsync`, so unsaved changes to that object would be visible. That is the trap again, inside EF Core, and a fourth query could easily forget the call.

**Principle.** State a policy once and give it a name.

**Change.** A private query root that every read starts from.

Before:

```csharp
public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
    db.Invoices.AsNoTracking().FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
```

After:

```csharp
// Every read starts here: untracked invoices are detached copies, the same as the in-memory implementation returns.
private IQueryable<Invoice> DetachedInvoices => db.Invoices.AsNoTracking();

public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
    DetachedInvoices.FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
```

### Considered and deliberately not changed

- **The overdue rule is written twice:** once as `Invoice.IsOverdueOn`, used by the in-memory repository, and once as the EF Core predicate `PaidOn == null && DueDate < asOf`, which translates fully to SQL (checked with `ToQueryString()`). A single owner would mean an expression-tree specification in the domain (`Expression<Func<Invoice, bool>>`), because EF Core can only translate expressions. `IsOverdueOn` would then either compile that expression on every call or stay as a second copy anyway. For one rule used by one query, that costs more than it saves. The duplication is guarded: AC-2 runs the same examples against both copies, including the boundary case "due today is not overdue". Idea 1 below says when to revisit this.
- **Service flow.** `MarkPaidAsync` already reads top to bottom as FR-7: not found, already paid, then pay and save. Nothing to improve.
- **Test helpers.** The contract tests and the service tests each have a one-line `NewInvoice` helper with different parameters. A shared builder would add a file to save one line. All test code is byte-for-byte the same as at first green.

## Result

- After refactoring: `Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 1 s - Invoicing.Tests.dll (net9.0)`
- `dotnet build RepositoryPattern.sln --no-incremental`: `0 Warning(s)`, `0 Error(s)`.
- Public API changes: none. Only `Invoicing.Infrastructure` internals changed: the type held by `InMemoryInvoiceStore`'s internal dictionary, plus the new internal `InvoiceSnapshot`. The `Invoicing` project and the tests are unchanged since first green.

## Not done — ideas for a further manual pass

Each idea is small and starts with a failing test, or ends with the contract suite proving that nothing changed. The behaviours quoted in ideas 2 and 3 were checked with a throwaway probe test that was deleted afterwards.

1. **Give the overdue rule one owner once a second query needs it,** for example a count of overdue invoices for a dashboard. Add `public static Expression<Func<Invoice, bool>> OverdueOn(DateOnly date)` to `Invoice` (written with `PaidOn == null`, because `IsPaid` is not a column), use it in both repositories and run the contract suite.
2. **Pin down the error contract, test-first.** Adding a duplicate number makes EF Core throw `DbUpdateException` (SQLite: `UNIQUE constraint failed: Invoices.Number`), while the in-memory repository accepts it. Updating an invoice that was never added makes EF Core throw `DbUpdateConcurrencyException`, while the in-memory repository silently inserts it. Write the two contract tests; they will fail for the in-memory implementation, which is the trap in another form. Then make both implementations throw the same exception, translating EF Core's exceptions inside `EfInvoiceRepository` so they do not leak through the interface.
3. **Make one repository instance safe for Add, then Get, then Update.** On a single `EfInvoiceRepository` this sequence throws `InvalidOperationException` ("another instance with the same key value for {'Id'} is already being tracked"), because `AddAsync` leaves the added object tracked. The in-memory repository handles it. Add the contract test, then fix EF Core (for example with `db.ChangeTracker.Clear()` after each save), or document the restriction on the interface.
4. **Rebuild invoices without replaying behaviour.** `InvoiceSnapshot.ToInvoice()` restores `PaidOn` by calling `MarkPaid`. That is fine while `MarkPaid` only sets a date. Once it gains rules or side effects (a domain event, or "paid on or after the issue date"), loading history would trigger them. Add an explicit restore path, such as a static `Invoice.Restore(...)` factory, and decide how public it should be.

## Lessons learned

- **A fake is only useful if it has the database's semantics.** The naive in-memory repository passed 8 of 9 tests and made a service that never saved look correct. Copy-in/copy-out semantics turned the same service test into a real check.
- **Contract tests make "swappable" a checked claim, but only for what they cover.** They caught the live-reference trap on the first run. They did not notice three unguarded copy paths or the duplicate-number divergence; that is why the snapshot type and ideas 2 and 3 exist.
- **Prefer types to discipline.** Storing `InvoiceSnapshot` instead of `Invoice` turned "remember to copy" into a compile error on every path, including paths no test exercises.
- **Persistence ignorance is enforced here, but it is not free.** The project graph keeps EF Core out of `Invoicing`. Even so, EF Core shaped the entity (a constructor it can bind, explicit mapping for get-only properties), and the overdue rule had to be written a second time in a form EF Core can translate.
- **Wrapping `DbContext` means choosing semantics.** Save-per-call with detached objects is easy to fake, but it gives up EF Core's unit of work. The change tracking it hides comes back as soon as one repository instance is reused (idea 3).
