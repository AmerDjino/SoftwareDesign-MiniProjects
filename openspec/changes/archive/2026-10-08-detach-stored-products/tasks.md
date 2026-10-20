# Tasks

## 1. Baseline

- [x] 1.1 Run `dotnet test 05-CqrsBasics/CqrsBasics.sln` before changing anything and verify that all 6 test cases pass

## 2. Detach stored products

- [x] 2.1 Add a temporary test that changes a saved product and a loaded product without saving them, then loads the product again; verify that it fails against the current store, which proves the trap
- [x] 2.2 Add an internal `Product.Restore` factory that sets the quantity without replaying `Receive`; verify that the build has 0 warnings
- [x] 2.3 Store `ProductSnapshot` records in `InMemoryProductRepository` and return restored products, and document the detached-copy contract on `IProductRepository`; verify that the temporary test and the 6 existing test cases pass
- [x] 2.4 Delete the temporary test; verify that `dotnet test 05-CqrsBasics/CqrsBasics.sln` passes all 6 test cases and that a full rebuild has 0 warnings

## 3. Record the refactoring

- [x] 3.1 Add a follow-up section with before/after code and the test results of 2.1 to 2.4 to `05-CqrsBasics/docs/REFACTOR-NOTES.md`, and a row to the process log in `05-CqrsBasics/README.md`; verify that the recorded summary lines match the test runs
