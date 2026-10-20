# Proposal

## Why

The in-memory product store of `05-CqrsBasics` hands out the very `Product` object it stores. A command handler that changes a product has therefore already changed the stored product before it calls `SaveAsync`, which a real database never allows. Nothing fails today, because every handler checks before it changes anything and always saves. But this is the fake-repository trap that `03-RepositoryPattern` demonstrates: a handler that forgot to save, or failed halfway through a change, would still pass every test.

## What Changes

- The in-memory product store keeps an immutable snapshot of each product and builds a new `Product` for every lookup, so a change counts only once it is saved.
- `IProductRepository` documents that contract.
- Refactoring only: commands and queries behave exactly as before, so no spec changes (`skip_specs: true`).

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. The `warehouse-stock` behaviour is unchanged.

## Impact

- Mini-project: `05-CqrsBasics`, files `src/Inventory/WriteSide/InMemoryProductRepository.cs`, `Product.cs` and `IProductRepository.cs`.
- Documentation: a follow-up section in `05-CqrsBasics/docs/REFACTOR-NOTES.md` and a row in the README process log.
