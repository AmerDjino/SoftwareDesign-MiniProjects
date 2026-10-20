# 05 — CQRS basics: refactor notes

What the first working version looked like, what changed during refactoring and why, and what is left for a manual pass.

## Starting point

The first implementation was the straightforward version of the spec. All six test cases passed on its first run:

`Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 734 ms - Inventory.Tests.dll (net9.0)`

What it looked like:

- `Dispatcher.QueryAsync` built the closed handler type with `MakeGenericType` on every call and invoked it through `dynamic`.
- Failure messages were string literals at the point of failure. "Product '…' is not registered." was written twice (receive and ship handlers), and `Product.Ship` formatted its message with `string.Create(CultureInfo.InvariantCulture, …)` in the middle of a business rule.
- Each command handler spelled out the command flow: load (or fail), change, save, project.

Two checks before changing anything. Both changes were temporary and reverted, and the file hashes were compared with the first-green snapshot.

- **Can the architecture test fail?** It had never been red, because the stubs already had the right constructors. Giving `GetStockLevelHandler` an extra `IProductRepository` parameter made it fail and name the culprit: `Item: Tuple ("GetStockLevelHandler", typeof(Inventory.WriteSide.IProductRepository))`.
- **Is the `dynamic` call safe?** Implementing `HandleAsync` explicitly in `GetStockLevelHandler` is valid C# and compiles, but every query to that handler then failed at runtime: `RuntimeBinderException: 'Inventory.ReadSide.GetStockLevelHandler' does not contain a definition for 'HandleAsync'` (`Failed: 4, Passed: 2`).

## Refactorings

### 1. A typed query invoker instead of `dynamic`

**Problem.** The `dynamic` call is resolved by name at runtime. The compiler cannot check it: renaming `IQueryHandler.HandleAsync` would still compile. The runtime binder cannot see explicit interface implementations (the check above), and failures surface as `RuntimeBinderException` rather than a clear error. The closed handler type was also rebuilt with reflection on every call.

**Reason.** Keep reflection as small as possible and let the compiler check the call. The caller passes an `IQuery<TResult>`, so the query's own type is only known at runtime; MediatR solves the same problem with the same wrapper pattern.

**Change.** A new internal pair in `Dispatching/QueryHandlerInvoker.cs`: an abstract `QueryHandlerInvoker<TResult>` and one sealed `QueryHandlerInvoker<TQuery, TResult>`. Reflection now runs once per query type to create the invoker, which is cached in a `ConcurrentDictionary`. Inside the invoker the handler is resolved and called with full static typing. The invoker holds no state and receives the `IServiceProvider` as an argument, so one cached instance can serve every container and scope without capturing one.

Before (`Dispatcher`):

```csharp
public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(query);
    var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
    dynamic handler = _services.GetRequiredService(handlerType);
    return await handler.HandleAsync((dynamic)query, cancellationToken);
}
```

After (`Dispatcher`, then the call inside `QueryHandlerInvoker<TQuery, TResult>`):

```csharp
public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(query);
    var invoker = QueryHandlerInvoker<TResult>.For(query.GetType());
    return invoker.InvokeAsync(query, _services, cancellationToken);
}
```

```csharp
var handler = services.GetRequiredService<IQueryHandler<TQuery, TResult>>();
return handler.HandleAsync((TQuery)query, cancellationToken);
```

Repeating the explicit-implementation check against the new code gives `Passed: 6`. One observable difference: `QueryAsync` is no longer an `async` method, so a `null` query or a missing handler now throws when `QueryAsync` is called instead of when its task is awaited. `SendAsync` already behaved this way, and callers that await the call see no difference. Not claimed: a performance gain (nothing was measured) or AOT-friendliness (`MakeGenericType` is still reflection).

### 2. Failure messages in one place

**Problem.** The same user-facing message existed twice, and the aggregate's shipping rule was interrupted by culture-aware string formatting.

**Reason.** One source of truth for text the spec defines exactly (its failure-message table), and an aggregate that reads as rules.

**Change.** A new internal static class `WriteSide/ProductErrors.cs` with `AlreadyRegistered`, `NotRegistered` and `InsufficientStock`. It mirrors the spec's table and owns the one number format.

Before:

```csharp
// Product.Ship
return CommandResult.Failure(string.Create(
    CultureInfo.InvariantCulture,
    $"Cannot ship {quantity} of '{Sku}': only {QuantityOnHand} on hand."));

// ReceiveStockHandler and ShipStockHandler
return CommandResult.Failure($"Product '{command.Sku}' is not registered.");
```

After:

```csharp
// Product.Ship
return CommandResult.Failure(ProductErrors.InsufficientStock(Sku, quantity, QuantityOnHand));

// ReceiveStockHandler and ShipStockHandler
return CommandResult.Failure(ProductErrors.NotRegistered(command.Sku));
```

The tests still assert the literal texts, so they confirm that no message changed in the move.

### 3. Explain the shared read-store registration

**Problem.** `AddInventory` registers `InMemoryStockLevelStore` once and forwards both read-model interfaces to it. That looks redundant, and the obvious "simplification" (`AddSingleton<IStockLevelReader, InMemoryStockLevelStore>()` plus the same for the writer) creates two stores. Tried temporarily: the container still builds, `ValidateOnBuild` included, but all five scenario test cases fail because the queries read an empty store (`Actual: null`).

**Change.** A one-line comment; no code change. The tests already guard this, but the comment stops the mistake before it is made.

```csharp
// One store instance behind both interfaces, so the rows the projection writes are the rows the queries read.
services.AddSingleton<InMemoryStockLevelStore>();
services.AddSingleton<IStockLevelReader>(provider => provider.GetRequiredService<InMemoryStockLevelStore>());
services.AddSingleton<IStockLevelWriter>(provider => provider.GetRequiredService<InMemoryStockLevelStore>());
```

### Considered and rejected

- **One helper for the shared command steps** (load or fail, change, save, project) in `ReceiveStockHandler` and `ShipStockHandler`. There are only two copies. A helper would take a `Func<Product, CommandResult>`; because `Receive` returns `void`, the receive handler would need a wrapping lambda, or `Receive` would have to return a result that can never be a failure, which bends the domain API to suit a helper. The explicit sequence is also the clearest picture of the command side, which is what this project is for.
- **Moving the low-stock rule into `Product`** (for example an `IsLowStock` property). The write model never uses it, so adding it would put a screen concern into the write model, the very thing CQRS separates. The rule stays in the projection, where the read model's shape is decided.

## Result

- `dotnet test` after refactoring: `Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 947 ms - Inventory.Tests.dll (net9.0)`
- `dotnet build` of the solution (full rebuild): `0 Warning(s)`, `0 Error(s)`.
- Test code unchanged.
- Public API unchanged. New internal types: `QueryHandlerInvoker<TResult>`, `QueryHandlerInvoker<TQuery, TResult>` and `ProductErrors`. The library no longer uses `dynamic`.
- Green runs of the refactored code took between 113 ms and 947 ms, so the durations say nothing about performance.

## Follow-up refactoring (2026-10-08): detached products in the write store

Done after the exercise, as the OpenSpec change `detach-stored-products` (archived under `openspec/changes/archive/` at the repo root). It is not one of the manual-pass ideas below; those are all still open.

**Smell.** `InMemoryProductRepository` returned the very `Product` object it stored, so a handler's change reached the stored product before `SaveAsync` was called. Nothing failed, because every handler checks before changing anything and always saves. But this is the fake-repository trap that `03-RepositoryPattern` demonstrates: a handler that forgot to save, or failed halfway through a change, would still pass every test.

**Reason.** The write model's store should behave like a database: a change counts only once it is saved. If the store holds a different type than the one it hands out, sharing a stored object becomes a compile error instead of a rule to remember. This is the same move as `InvoiceSnapshot` in 03.

**Change.** The store keeps a private `ProductSnapshot` record and builds a fresh `Product` on every lookup, through a new internal `Product.Restore` that sets the quantity without replaying `Receive`. `IProductRepository` now documents the detached-copy contract.

Before:

```csharp
private readonly ConcurrentDictionary<string, Product> _products = new(StringComparer.OrdinalIgnoreCase);

public Task<Product?> FindAsync(string sku, CancellationToken cancellationToken) =>
    Task.FromResult(_products.GetValueOrDefault(sku));

public Task SaveAsync(Product product, CancellationToken cancellationToken)
{
    _products[product.Sku] = product;
    return Task.CompletedTask;
}
```

After:

```csharp
// Snapshots, not Product objects: a caller's changes reach the store only through SaveAsync, as with a database.
private readonly ConcurrentDictionary<string, ProductSnapshot> _products = new(StringComparer.OrdinalIgnoreCase);

public Task<Product?> FindAsync(string sku, CancellationToken cancellationToken) =>
    Task.FromResult(_products.GetValueOrDefault(sku)?.ToProduct());

public Task SaveAsync(Product product, CancellationToken cancellationToken)
{
    _products[product.Sku] = ProductSnapshot.Of(product);
    return Task.CompletedTask;
}

private sealed record ProductSnapshot(string Sku, string Name, int ReorderLevel, int QuantityOnHand)
{
    public static ProductSnapshot Of(Product product) =>
        new(product.Sku, product.Name, product.ReorderLevel, product.QuantityOnHand);

    public Product ToProduct() => Product.Restore(Sku, Name, ReorderLevel, QuantityOnHand);
}
```

**Checked with a temporary test**, deleted afterwards so the project keeps its five test methods. The test saves a product, loads it, changes both the saved and the loaded object without saving, and loads it again.

- Before the refactoring the test failed (`Expected: 0, Actual: 12`): both changes had reached the store. Summary: `Failed!  - Failed:     1, Passed:     6, Skipped:     0, Total:     7, Duration: 56 ms - Inventory.Tests.dll (net9.0)`
- After it: `Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 84 ms - Inventory.Tests.dll (net9.0)`
- With the temporary test deleted: `Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 73 ms - Inventory.Tests.dll (net9.0)`, and a full rebuild has `0 Warning(s)`, `0 Error(s)`.

The six existing test cases did not change. Public API: unchanged apart from the extra sentence in the `IProductRepository` documentation; `Product.Restore` is internal.

## Not done — ideas for a further manual pass

1. **Split `IDispatcher`** into `ICommandSender` (`SendAsync`) and `IQuerySender` (`QueryAsync`), both implemented by `Dispatcher` and registered like the read store (one instance, two interfaces). A screen that only reads can then depend on something that cannot change state.
2. **Check the other direction too.** Turn `QueryHandlers_DependOnlyOnTheReadModel` into a `[Theory]` that also asserts that command handlers never take `IStockLevelReader`: decisions belong on the write model, and once projections are asynchronous the read model can be stale. A theory keeps the project at five test methods.
3. **Try the projection as a decorator.** Write `ProjectingProductRepository : IProductRepository`, which saves through `InMemoryProductRepository` and then calls the projection; register it in `AddInventory` and remove the projection from the three handlers. Then decide which you prefer: the read model can no longer be forgotten, but its update is no longer visible in the handler.
4. **Give failures a code.** Add an enum (`NotRegistered`, `AlreadyRegistered`, `InsufficientStock`) to `CommandResult`, set from `ProductErrors`, so an API layer can map failures to HTTP status codes without parsing text.
5. **Shrink the public surface.** Make the handlers, stores, `Product`, the projection and `Dispatcher` internal, so callers can only use the messages, `IDispatcher`, `StockLevelView`, `CommandResult` and `AddInventory`. `ShipStock_MoreThanOnHand_FailsAndLeavesStockUnchanged` reads `IProductRepository` directly, so decide whether that interface stays public or the test project gets `InternalsVisibleTo`.

## Lessons learned

- **Cheap reads are paid for at write time.** `IsLowStock` is computed once, when the projection writes the row, so both query handlers are one-line pass-throughs. The logic did not disappear; it moved to the projection.
- **The dispatcher's reflection is the price of a friendly call site.** `QueryAsync(IQuery<TResult>)` lets callers write `await dispatcher.QueryAsync(new GetStockLevel("BOLT-M6"))` and get a typed result, so the dispatcher has to recover the query's runtime type. Commands are routed by their compile-time type and need no reflection.
- **Green tests did not prove the dispatcher sound.** The `dynamic` version passed everything yet failed on a legal handler shape. A deliberate "what if" experiment found it.
- **Architecture tests guard shape, not behaviour.** The dependency test passed against the stubs and never went red on its own; breaking the rule on purpose was the only way to see that it works.
- **Test through the real composition.** Two store singletons instead of one compiled and passed `ValidateOnBuild`, yet every scenario failed. Only tests that write through one side and read through the other notice that the two models are not connected.
