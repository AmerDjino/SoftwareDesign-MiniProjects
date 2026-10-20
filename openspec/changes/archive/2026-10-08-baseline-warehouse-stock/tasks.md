# Tasks

## 1. Specify the existing behaviour

- [x] 1.1 Turn FR-1 to FR-8, BR-1 to BR-4 and the failure-message table of `05-CqrsBasics/docs/SPEC.md` into requirements and scenarios in `specs/warehouse-stock/spec.md` (FR-9, dispatcher routing, is design and stays in `docs/SPEC.md`); verify with `openspec validate baseline-warehouse-stock --strict`

## 2. Trace every scenario to a test

- [x] 2.1 "Delivery" and "Lookup after deliveries": verified by `StockScenarioTests.ReceiveStock_ThenGetStockLevel_ReturnsUpdatedQuantity`
- [x] 2.2 "Shipment larger than stock": verified by `StockScenarioTests.ShipStock_MoreThanOnHand_FailsAndLeavesStockUnchanged`
- [x] 2.3 "Reorder screen" and "Shipment within stock": verified by `StockScenarioTests.GetLowStockProducts_ReturnsProductsAtOrBelowReorderLevel_LowestQuantityFirst`
- [x] 2.4 "Duplicate SKU in any letter case": verified by `StockScenarioTests.RegisterProduct_DuplicateSku_Fails` (2 cases)
- [x] 2.5 "Results carry no data": verified by tasks 2.1, 2.2 and 2.4, which compare whole command results, and by the result type, which has no data member
- [x] 2.6 "Repeated queries": guarded structurally by `ArchitectureTests.QueryHandlers_DependOnlyOnTheReadModel` (see design.md)
- [x] 2.7 Record the scenarios without an automated test ("Unregistered SKU", unregistered shipping, "Zero quantity", unregistered lookup, the SKU tie-break) in design.md; verify by reading its Risks section
- [x] 2.8 Run `dotnet test 05-CqrsBasics/CqrsBasics.sln` and verify that all 6 test cases pass

## 3. Point readers to the living spec

- [x] 3.1 Link `openspec/specs/warehouse-stock/spec.md` from `05-CqrsBasics/README.md` and from the header of `05-CqrsBasics/docs/SPEC.md`; verify that every link points to the spec path that archiving creates
