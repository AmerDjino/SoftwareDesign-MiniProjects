# Refactor notes: 01 SOLID Principles

What changed between the first green implementation and the final code, and why.

## Starting point

First green was the simplest code that passed the five test methods (21 test cases):

```text
Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 176 ms - Checkout.Tests.dll (net9.0)
```

The boundaries were already the ones the spec defined (`IDiscountRule`, `ICouponLookup`, constructor injection), so the calculator never needed an edit to make a test pass. The smells were inside the classes:

- the rounding policy was written out in full in two rules;
- `PriceBreakdown` was a passive four-value record: the calculator computed the cap and the total, and any code could build a breakdown whose numbers disagree;
- `LoyaltyDiscount` held its rates in a switch;
- `BulkPurchaseDiscount` used the bare numbers `10` and `0.10m`.

**Are the tests worth trusting?** Before refactoring, I applied 11 deliberate bugs (mutations) one at a time to a scratch copy of the code and ran the tests each time. Eight were caught by the test that should catch them: an unrounded percentage in either rule, `> 10` instead of `>= 10`, a coupon not capped at the subtotal, an ignored coupon minimum, a case-sensitive catalog, a missing total cap, and zero amounts being listed. Three survived, and they are exactly the gaps already recorded in SPEC.md §6: `ToEven` instead of `AwayFromZero`, a different Silver rate, and a coupon that returns 0 instead of the subtotal when it exceeds the subtotal.

## R1: The rounding policy gets one owner

**Smell.** `Math.Round(x, 2, MidpointRounding.AwayFromZero)` appeared in `BulkPurchaseDiscount` and `LoyaltyDiscount`, and a third time in the test project's `SkuPromotionDiscount`.

**Reason.** Single Responsibility, and DRY in its original sense: "how this shop rounds money" is one business decision. If it changes (banker's rounding, a currency without cents), every rule must change in step. Two call sites would not normally justify an extraction, but this is one decision written twice, not two pieces of code that happen to look alike. And since the `IDiscountRule` contract requires rounding, every future rule is another call site.

**Change.** A new public `Money.Round(decimal)`. Both rules call it, and the `IDiscountRule` contract now refers to it. It is public because `IDiscountRule` is a public extension point: a rule written in another assembly needs the same policy to honour the contract.

```csharp
// Before (BulkPurchaseDiscount; LoyaltyDiscount had the same call)
return Math.Round(bulkLinesTotal * 0.10m, 2, MidpointRounding.AwayFromZero);

// After
public static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
return Money.Round(bulkLinesTotal * Rate);
```

## R2: PriceBreakdown owns its totals

**Smell.** The cap invariant (`TotalDiscount = min(sum, Subtotal)`, `Total = Subtotal − TotalDiscount`) lived in the calculator, while `PriceBreakdown` accepted any four numbers. `new PriceBreakdown(100m, [], 30m, 50m)` compiled and described an impossible price.

**Reason.** Single Responsibility: the calculator's job is to run rules, while the arithmetic of a breakdown is a fact about the breakdown. Also, make invalid states unrepresentable: a type that derives its totals cannot contradict itself. Computed properties also stay correct on a `with` copy, where stored totals would be copied unchanged.

**Change.**

```csharp
// Before (PriceCalculator.Calculate, end of the method)
var totalDiscount = Math.Min(discounts.Sum(discount => discount.Amount), subtotal);
return new PriceBreakdown(subtotal, discounts, totalDiscount, subtotal - totalDiscount);

// After (PriceCalculator.Calculate)
return new PriceBreakdown(order.Subtotal, discounts);

// After (PriceBreakdown)
public sealed record PriceBreakdown(decimal Subtotal, IReadOnlyList<AppliedDiscount> Discounts)
{
    public decimal TotalDiscount => Math.Min(Discounts.Sum(discount => discount.Amount), Subtotal);

    public decimal Total => Subtotal - TotalDiscount;
}
```

This changed a public constructor. No test changed, because the tests only read breakdowns and never build one.

## R3: Loyalty rates become data

**Smell.** The rates were part of the control flow:

```csharp
// Before
var rate = order.Tier switch
{
    CustomerTier.Silver => 0.03m,
    CustomerTier.Gold => 0.05m,
    _ => 0m,
};
return Math.Round(order.Subtotal * rate, 2, MidpointRounding.AwayFromZero);
```

**Reason.** Open/Closed at the level of one rule. The mechanism ("a percentage of the subtotal by tier") is stable; the rate table is what marketing changes. When the table is passed in, the caller (composition root, configuration) can change rates without editing the rule.

**Change.** A constructor that takes `IReadOnlyDictionary<CustomerTier, decimal>`. The parameterless constructor keeps the FR-6 rates, so existing callers and tests are unaffected. The rule copies the table, so a caller that later changes its own dictionary cannot change the rule's results (contract: same order, same amount).

```csharp
// After
public decimal CalculateDiscount(Order order) =>
    Money.Round(order.Subtotal * _ratesByTier.GetValueOrDefault(order.Tier));
```

**Trade-off.** This is the most debatable change here. The switch was readable, and the gain stays small until rates actually come from configuration. It also has a real cost: the contract (`0 ≤ amount ≤ subtotal`) used to be guaranteed by code and now depends on data. A configured rate of 1.5 or −0.1 would break it, and the constructor does not validate rates, because that would be new, untested behaviour. See idea 1 below.

## R4: Bulk numbers get names

**Smell.** `line.Quantity >= 10` and `bulkLinesTotal * 0.10m`.

**Change.** `private const int MinimumQuantity = 10;` and `private const decimal Rate = 0.10m;`.

**Why not configurable like R3?** The loyalty table grows with the set of tiers. The bulk rule has two scalars and nothing asks for them to vary. Names are enough until a requirement says otherwise.

## Considered and not done

- **Caching `Order.Subtotal`.** It is recomputed on every access (the calculator and the loyalty and coupon rules all read it). Caching would be premature for a handful of lines, and it is a trap with records: a value cached at construction is copied unchanged by `order with { Lines = ... }`, so the copy would report the old subtotal.
- **Rounding in `CouponDiscount`.** It returns `min(Amount, Subtotal)` without rounding; with valid data (SPEC §7) both values already have at most 2 decimals. Adding `Money.Round` there would change behaviour for invalid data, which is not a refactoring.
- **LINQ instead of the loop in `PriceCalculator.Calculate`.** Equivalent; a matter of taste. The loop stayed.

## Result

- `dotnet test` after the refactor:

  ```text
  Passed!  - Failed:     0, Passed:    21, Skipped:     0, Total:    21, Duration: 195 ms - Checkout.Tests.dll (net9.0)
  ```

- `dotnet build SolidPrinciples.sln --no-incremental`: 0 warnings, 0 errors.
- Test code: unchanged.
- Public API changes:
  - `PriceBreakdown` is constructed from `(Subtotal, Discounts)`; `TotalDiscount` and `Total` are computed, read-only properties.
  - New static class `Money` with `Round(decimal)`.
  - `LoyaltyDiscount` has a new constructor that takes a rate table; the parameterless constructor is unchanged.
- A second mutation run on the refactored code still catches a missing cap in `PriceBreakdown` and rounding to 3 decimals in `Money`; `ToEven` still survives (known gap).

## Not done: ideas for a further manual pass

1. **Validate configured loyalty rates.** Make the rate-table constructor of `LoyaltyDiscount` reject rates below 0 or above 1 with `ArgumentOutOfRangeException`, test first. This closes the contract hole that R3 opened.
2. **Put the custom rule under the contract.** Add `SkuPromotionDiscount` to the rules in `DiscountRuleContractTests` (renaming `BuiltInRules` to `Rules`) and switch it to `Money.Round`. Every new rule should pass the same contract test; that is how LSP is enforced in practice.
3. **Close the three known gaps** from SPEC §6 with exact-value tests: Silver 3% of 100.00 = 3.00; Gold 5% of 2.50 = 0.125, which must become 0.13 (`ToEven` would give 0.12); a 10.00 coupon on a 3.98 order gives 3.98. This goes beyond the exercise's budget of five test methods, which is fine once the exercise is done.
4. **Fail fast on contract violations.** Make `PriceCalculator` throw `InvalidOperationException` naming the rule when a rule returns a negative amount or more than the subtotal; today a negative amount is silently dropped by the `> 0` filter. Test it with a deliberately broken stub, then decide whether the runtime check earns its place next to the contract test.
5. **Try an exclusivity rule**, such as "a coupon cannot be combined with the loyalty discount". It does not fit `IDiscountRule`, because a rule sees only the order, not the other rules' results. Sketch what would have to change (a policy step after the rules? a composite rule?). This is where the abstraction stops being open for extension.

## Lessons learned

- **Good boundaries do not guarantee good insides.** The spec gave first green SOLID-shaped interfaces, yet the refactor still found a duplicated policy, a misplaced invariant and hard-coded data.
- **LSP can be tested.** One theory over every rule × edge-case order caught the unrounded percentages and the uncapped coupon in the mutation check: bugs that would otherwise surface as a wrong total for one particular order.
- **SRP is about reasons to change, not class size.** `Money` is one line and still earns its place, because "how we round" is a decision with its own reason to change. The cap moved to `PriceBreakdown` because it is a fact about the totals, not about running rules.
- **Configurability moves a guarantee from code to data** (R3). Without validation, the contract the rest of the design relies on becomes an assumption.
- **OCP holds only along the axis the abstraction anticipated.** A new independent rule costs one class and no edits (test 3). A rule that depends on other rules' results would need a new abstraction.
