# Tasks

## 1. Specify the existing behaviour

- [x] 1.1 Turn FR-1 to FR-10 of `01-SolidPrinciples/docs/SPEC.md` into requirements and scenarios in `specs/checkout-pricing/spec.md`; verify with `openspec validate baseline-checkout-pricing --strict`

## 2. Trace every scenario to a test

- [x] 2.1 "Several lines", "No rule applies", "Line below the threshold", "Standard customer" and "Coupon does not apply": verified by `PriceCalculatorTests.Calculate_NoApplicableRules_TotalEqualsSubtotal` (3 cases)
- [x] 2.2 "Several rules apply", "Line at the threshold", "Gold customer" and "Coupon applies": verified by `PriceCalculatorTests.Calculate_SeveralApplicableRules_StacksDiscountsAndListsEachOne`
- [x] 2.3 "Discounts exceed the subtotal": verified by `PriceCalculatorTests.Calculate_DiscountsExceedSubtotal_TotalDiscountIsCappedAndTotalIsZero`
- [x] 2.4 "Built-in rules on edge-case orders": verified by `DiscountRuleContractTests.DiscountRules_AnyOrder_HonourTheDiscountContract` (15 cases)
- [x] 2.5 "Rule supplied by the caller": verified by `PriceCalculatorTests.Calculate_CustomRule_IsAppliedWithoutChangingTheCalculator`
- [x] 2.6 Record the scenarios without a dedicated test ("No lines", "Silver customer", "Coupon larger than the subtotal", "Midpoint") in design.md; verify by reading its Risks section
- [x] 2.7 Run `dotnet test 01-SolidPrinciples/SolidPrinciples.sln` and verify that all 21 test cases pass

## 3. Point readers to the living spec

- [x] 3.1 Link `openspec/specs/checkout-pricing/spec.md` from `01-SolidPrinciples/README.md` and from the header of `01-SolidPrinciples/docs/SPEC.md`; verify that every link points to the spec path that archiving creates
