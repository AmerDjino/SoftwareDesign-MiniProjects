# Design

## Context

See proposal.md for why. `InMemoryProductRepository` keeps `Product` objects in a `ConcurrentDictionary` and returns them as they are. Each of the three command handlers loads a product, changes it, saves it and projects it into the read model.

## Goals / Non-Goals

**Goals:**
- Give the write-side store database semantics: a change is stored only by `SaveAsync`.
- Make handing out a stored object impossible to write, instead of a rule every method has to remember.

**Non-Goals:**
- Concurrency control, which stays out of scope (`docs/SPEC.md`, D-6).
- The read side, which already stores immutable `StockLevelView` records.
- The manual-pass ideas in `docs/REFACTOR-NOTES.md`, which are left for the developer.

## Decisions

- **Store an immutable snapshot, not the aggregate.** A private `ProductSnapshot` record inside the repository holds the SKU, name, reorder level and quantity on hand, and every lookup builds a fresh `Product` from it. Because the dictionary no longer holds `Product` objects, returning a stored instance does not compile. This is the same move as `InvoiceSnapshot` in `03-RepositoryPattern` (its refactoring 1). Alternative considered: copying the `Product` on the way in and on the way out. Rejected, because each method would have to remember the copy, the fragility 03 measured.
- **Restore without replaying behaviour.** `Product` gets an internal `Restore` factory that sets the quantity directly. Calling `Receive(quantity)` on load would also work today, but it runs business rules while loading, the problem idea 4 in `03-RepositoryPattern/docs/REFACTOR-NOTES.md` describes. `internal` keeps the factory out of the public API; only the store, in the same assembly, uses it.
- **Effect on the CQRS rules.** "A failed command changes nothing" (`warehouse-stock`, requirement Command results) now also holds when a future aggregate method fails after a partial change: an unsaved change cannot reach the write model.

## Risks / Trade-offs

- [Every lookup allocates a new `Product`] → Negligible at this size, and it is what an ORM does as well.
- [No permanent test for the detached semantics] → The snapshot type guarantees them, and a temporary test shows the before and after (tasks 2.1 and 2.4) without growing the project past its five test methods. Behaviour through commands and queries stays covered by the existing six test cases.
