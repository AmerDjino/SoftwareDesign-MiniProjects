## Purpose

Prices an order for a small web shop: the subtotal, stackable discounts from independent discount rules, and a breakdown that names every discount and never lets the total drop below zero.

## ADDED Requirements

### Requirement: Order subtotal
A line total SHALL be the unit price multiplied by the quantity, and an order's subtotal SHALL be the sum of its line totals. An order without lines SHALL have a subtotal of 0.00.

#### Scenario: Several lines
- **WHEN** an order has the lines 9 × 1.50 and 1 × 25.00
- **THEN** its subtotal is 38.50

#### Scenario: No lines
- **WHEN** an order has no lines
- **THEN** its subtotal is 0.00

### Requirement: Discount breakdown
Pricing SHALL apply every configured discount rule to the original order, in the order the rules were configured. Each rule that grants more than 0.00 SHALL be listed with its name and amount, in that order, and rules that grant 0.00 SHALL NOT be listed. Discounts SHALL add up and SHALL NOT compound.

#### Scenario: No rule applies
- **GIVEN** the built-in rules and a Standard customer
- **AND** an order of 9 × 1.50 and 1 × 25.00 with no coupon code, the unknown code `WELCOME10`, or the code `SAVE5`, whose minimum subtotal is 50.00
- **WHEN** the order is priced
- **THEN** no discount is listed
- **AND** the total discount is 0.00 and the total equals the subtotal, 38.50

#### Scenario: Several rules apply
- **GIVEN** the rules loyalty, bulk purchase and coupon, configured in that order, and the coupon `SAVE5` (5.00 off, minimum subtotal 50.00)
- **AND** a Gold customer's order of 10 × 2.00 and 1 × 80.00 with the coupon code `save5`
- **WHEN** the order is priced
- **THEN** the breakdown lists `Loyalty` 5.00, `Bulk purchase` 2.00 and `Coupon` 5.00, in that order
- **AND** the subtotal is 100.00, the total discount 12.00 and the total 88.00

### Requirement: Total discount cap
The total discount SHALL be the sum of the listed amounts, capped at the subtotal, and the total SHALL be the subtotal minus the total discount, so the total is never negative. Listed amounts SHALL stay as each rule granted them.

#### Scenario: Discounts exceed the subtotal
- **GIVEN** two rules that grant 30.00 and 25.00
- **WHEN** an order with a subtotal of 40.00 is priced
- **THEN** both amounts are listed unchanged
- **AND** the total discount is 40.00 and the total is 0.00

### Requirement: Bulk purchase discount
The bulk purchase rule, listed as `Bulk purchase`, SHALL grant 10% of the line total of every line with a quantity of 10 or more.

#### Scenario: Line at the threshold
- **WHEN** an order contains the line 10 × 2.00
- **THEN** the bulk purchase rule grants 2.00

#### Scenario: Line below the threshold
- **WHEN** no line of an order has more than 9 units
- **THEN** the bulk purchase rule grants nothing

### Requirement: Loyalty discount
The loyalty rule, listed as `Loyalty`, SHALL grant a percentage of the subtotal that depends on the customer tier: Standard 0%, Silver 3%, Gold 5%.

#### Scenario: Gold customer
- **WHEN** a Gold customer's order has a subtotal of 100.00
- **THEN** the loyalty rule grants 5.00

#### Scenario: Silver customer
- **WHEN** a Silver customer's order has a subtotal of 100.00
- **THEN** the loyalty rule grants 3.00

#### Scenario: Standard customer
- **WHEN** a Standard customer's order is priced
- **THEN** the loyalty rule grants nothing

### Requirement: Coupon discount
The coupon rule, listed as `Coupon`, SHALL grant a coupon's fixed amount when the order's coupon code matches a known coupon, ignoring letter case, and the subtotal is at least the coupon's minimum subtotal. The amount SHALL never exceed the subtotal. In every other case the rule SHALL grant nothing.

#### Scenario: Coupon applies
- **GIVEN** the coupon `SAVE5`: 5.00 off with a minimum subtotal of 50.00
- **WHEN** an order with a subtotal of 100.00 carries the code `save5`
- **THEN** the coupon rule grants 5.00

#### Scenario: Coupon does not apply
- **WHEN** an order has no code, an unknown code, or the code of a coupon whose minimum subtotal it does not reach
- **THEN** the coupon rule grants nothing

#### Scenario: Coupon larger than the subtotal
- **GIVEN** the coupon `TENOFF`: 10.00 off with no minimum subtotal
- **WHEN** an order with a subtotal of 3.98 carries the code `TENOFF`
- **THEN** the coupon rule grants 3.98

### Requirement: Money rounding
Every amount a rule computes as a percentage SHALL be rounded to 2 decimals, with midpoints rounded away from zero.

#### Scenario: Midpoint
- **WHEN** a Gold customer's order has a subtotal of 2.50, so 5% is 0.125
- **THEN** the loyalty rule grants 0.13

### Requirement: Discount rule contract
Every discount rule, built in or supplied by a caller, MUST grant an amount between 0.00 and the order's subtotal with at most 2 decimals, and MUST grant the same amount whenever it is given the same order.

#### Scenario: Built-in rules on edge-case orders
- **WHEN** each built-in rule prices, twice each, an empty order, an order below the coupon minimum, an order smaller than its coupon, a bulk order and a Gold customer's order
- **THEN** every amount is at least 0.00, at most the order's subtotal and has at most 2 decimals
- **AND** the second amount equals the first

### Requirement: Additional discount rules
Pricing SHALL accept additional discount rules supplied by the caller and apply them exactly like the built-in rules.

#### Scenario: Rule supplied by the caller
- **GIVEN** a caller-supplied rule that grants 20% off every `MUG` line
- **WHEN** an order of 2 × 12.50 `MUG` and 1 × 30.00 `TEAPOT` is priced with only that rule
- **THEN** the breakdown lists that rule with 5.00
- **AND** the total is 50.00
