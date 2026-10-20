# 01 — SOLID Principles

Checkout pricing for a small web shop: an order's subtotal, stackable discount rules (bulk purchase, loyalty tier, coupon) and a price breakdown that lists every discount and never goes below zero. Each SOLID principle has a concrete, tested place in the design. The calculator only orchestrates and is never edited to add a rule (S, O). Every rule honours one documented contract, checked by a contract test (L). The coupon rule depends on a one-method lookup interface (I). All collaborators are abstractions passed in by the caller (D).

## Artifacts

| Artifact | Location |
|----------|----------|
| Specification | [docs/SPEC.md](docs/SPEC.md) |
| Unit tests | [tests/Checkout.Tests](tests/Checkout.Tests) |
| Implementation | [src/Checkout](src/Checkout) |
| Refactor notes | [docs/REFACTOR-NOTES.md](docs/REFACTOR-NOTES.md) |
| Living spec (OpenSpec) | [openspec/specs/checkout-pricing/spec.md](../openspec/specs/checkout-pricing/spec.md) |

## How to run

From this folder:

```powershell
dotnet test
```

To see every test case by name (the two theories expand to 21 cases):

```powershell
dotnet test --logger "console;verbosity=normal"
```

There is no app; the library is exercised through its tests.

## Project layout

```text
01-SolidPrinciples/
  SolidPrinciples.sln
  docs/
    SPEC.md
    REFACTOR-NOTES.md
  src/Checkout/                      class library
    PriceCalculator.cs               runs the rules, builds the breakdown
    IDiscountRule.cs                 the rule contract
    BulkPurchaseDiscount.cs, LoyaltyDiscount.cs, CouponDiscount.cs
    ICouponLookup.cs, InMemoryCouponCatalog.cs, Coupon.cs
    Order.cs, OrderLine.cs, CustomerTier.cs
    PriceBreakdown.cs, AppliedDiscount.cs, Money.cs
  tests/Checkout.Tests/              xUnit
    PriceCalculatorTests.cs          tests 1, 2, 3 and 5
    DiscountRuleContractTests.cs     test 4: contract test, 3 rules x 5 orders
    SkuPromotionDiscount.cs          a rule from outside the library (test 3)
    FixedDiscountStub.cs             hand-written stub (test 5)
```

## Process log

| Step | What happened | `dotnet test` result |
|------|---------------|----------------------|
| 1. Specification | `docs/SPEC.md` written before any code: requirements with exact numbers, the SOLID mapping, five acceptance criteria, and the three details no test would pin down. | Not run (no code yet) |
| 2. Tests first | Five test methods written against the API in the spec. `dotnet build` failed: `Build FAILED`, 0 warnings, 8 errors, all CS0246 (type not found). | Not run (did not compile) |
| 3. Stubs (red) | Spec signatures with `throw new NotImplementedException()` bodies. All 21 cases failed, each with `NotImplementedException`. | `Failed!  - Failed:    21, Passed:     0, Skipped:     0, Total:    21, Duration: 279 ms - Checkout.Tests.dll (net9.0)` |
| 4. First green | Simplest implementation that passes. A mutation check on a scratch copy caught 8 of 11 deliberate bugs; the 3 that survived are the gaps the spec had already listed. | `Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 176 ms - Checkout.Tests.dll (net9.0)` |
| 5. Refactor | Rounding policy moved to `Money`; `PriceBreakdown` derives its own totals; loyalty rates became constructor data; bulk numbers got names. Test code unchanged; clean build with 0 warnings and 0 errors. | `Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 195 ms - Checkout.Tests.dll (net9.0)` |
| 6. OpenSpec baseline | Behaviour recorded as the [`checkout-pricing`](../openspec/specs/checkout-pricing/spec.md) spec through the OpenSpec change `baseline-checkout-pricing`: validated with `openspec validate --strict`, each scenario traced to its test, then archived. No code changed. | `Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 206 ms - Checkout.Tests.dll (net9.0)` |

Five test methods produce 21 test cases: test 1 has 3 rows, and test 4 (the contract test) has 15 (3 rules × 5 orders).

## Key takeaways

- The interfaces defined in the spec kept the calculator closed for modification from the first green: adding a rule never meant editing it. All the refactoring happened inside classes.
- A contract test makes Liskov substitution concrete: every rule is checked against the same promises (range, rounding, no side effects), not just the same method signature.
- SOLID has a price: 14 small types for logic that would fit in one 30-line method. It pays off here only because the scenario says discount policies change often.
