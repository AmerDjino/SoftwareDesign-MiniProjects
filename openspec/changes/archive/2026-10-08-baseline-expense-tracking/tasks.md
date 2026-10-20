# Tasks

## 1. Specify the existing behaviour

- [x] 1.1 Turn FR-1 to FR-4 and BR-1 to BR-4 of `04-LayeredArchitecture/docs/SPEC.md` into requirements and scenarios in `specs/expense-tracking/spec.md` (the architecture rules AR-1 to AR-4 are design and stay in `docs/SPEC.md`); verify with `openspec validate baseline-expense-tracking --strict`

## 2. Trace every scenario to a test

- [x] 2.1 "Missing and unknown values", plus the missing-value halves of "Description missing or too long", "Amount out of range" (0.00), "Unknown category" and "Date missing": verified by `ExpenseEndpointsTests.PostExpense_WhenBusinessValidationFails_Returns400ValidationProblem`
- [x] 2.2 "Values out of range", plus "Description missing or too long" (201 characters), "Amount out of range" (5,000.01) and "Date in the future": verified by `ExpenseServiceTests.Submit_InvalidExpense_ReturnsValidationErrorsAndStoresNothing`
- [x] 2.3 "October 2026": verified by `ExpenseServiceTests.GetMonthlySummary_TotalsPerCategoryForRequestedMonthOnly`
- [x] 2.4 "Valid expense", "Combined category names", "Month 13" and "Month without expenses": verified by running the API and calling it (expect `201` with a `Location` header, `400` with a `category` error, `404`, and an empty summary with a grand total of 0); no automated test
- [x] 2.5 Record the manual-only scenarios of 2.4 in design.md; verify by reading its Risks section
- [x] 2.6 Confirm the layer rules in design.md stay guarded by `ArchitectureTests.Presentation_DoesNotReferenceDataAccess` and `ArchitectureTests.Layers_DoNotReferenceLayersAboveThem` (3 cases)
- [x] 2.7 Run `dotnet test 04-LayeredArchitecture/LayeredArchitecture.sln` and verify that all 7 test cases pass

## 3. Point readers to the living spec

- [x] 3.1 Link `openspec/specs/expense-tracking/spec.md` from `04-LayeredArchitecture/README.md` and from the header of `04-LayeredArchitecture/docs/SPEC.md`; verify that every link points to the spec path that archiving creates
