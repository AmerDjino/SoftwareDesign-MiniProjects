# Tasks

## 1. Specify the existing behaviour

- [x] 1.1 Turn BR-1 to BR-3 and FR-1 to FR-7 of `03-RepositoryPattern/docs/SPEC.md` into requirements and scenarios in `specs/invoice-payments/spec.md` and `specs/invoice-storage/spec.md` (FR-8, persistence ignorance, and FR-9, the database schema, are design and stay in `docs/SPEC.md`); verify with `openspec validate baseline-invoicing --strict`

## 2. Trace every scenario to a test

- [x] 2.1 "Load a new invoice in a fresh session": verified by `AddAsync_ThenLoadFromFreshRepository_ReturnsSameInvoiceByIdAndByNumber`, run for both stores
- [x] 2.2 "Mixed invoices", "Due the day before", "Due on the day" and "Paid after its due date": verified by `ListOverdueAsync_ReturnsOnlyUnpaidInvoicesPastDueDate_OrderedByDueDate`, run for both stores
- [x] 2.3 "Unsaved and saved payment": verified by `UpdateAsync_PersistsChanges_AndUnsavedChangesAreNotVisible`, run for both stores
- [x] 2.4 "Shared contract": verified by the inheritance of the three contract tests in `InMemoryInvoiceRepositoryTests` and `EfInvoiceRepositoryTests` (6 test cases)
- [x] 2.5 "Unpaid invoice": verified by `InvoicePaymentServiceTests.MarkPaidAsync_UnpaidInvoice_MarksItPaidAndPersistsIt`
- [x] 2.6 "Unknown invoice number", "Invoice already paid" and, for the in-memory store only, "Unknown number": verified by `InvoicePaymentServiceTests.MarkPaidAsync_UnknownOrAlreadyPaidInvoice_ReturnsFailureAndChangesNothing` (2 cases)
- [x] 2.7 Record the remaining gaps (the error contract, "Unknown number" for the database store) in design.md; verify by reading its Risks section
- [x] 2.8 Run `dotnet test 03-RepositoryPattern/RepositoryPattern.sln` and verify that all 9 test cases pass

## 3. Point readers to the living specs

- [x] 3.1 Link `openspec/specs/invoice-payments/spec.md` and `openspec/specs/invoice-storage/spec.md` from `03-RepositoryPattern/README.md` and from the header of `03-RepositoryPattern/docs/SPEC.md`; verify that every link points to the spec path that archiving creates
