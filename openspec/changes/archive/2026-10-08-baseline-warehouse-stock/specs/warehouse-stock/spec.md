## Purpose

Keeps stock levels per product for a small warehouse: registering products, booking deliveries and shipments, and answering the stock-lookup and reorder screens.

## ADDED Requirements

### Requirement: Product registration
Registering a product SHALL create it with 0 on hand. A SKU SHALL identify exactly one product, compared ordinally ignoring letter case. Registering a SKU that already exists SHALL fail with `Product '{SKU}' is already registered.`, showing the SKU as first registered, and SHALL change nothing.

#### Scenario: Duplicate SKU in any letter case
- **GIVEN** `BOLT-M6` ("Hex bolt M6", reorder level 10) is registered
- **WHEN** `BOLT-M6` or `bolt-m6` is registered again as "Carriage bolt M6" with reorder level 50
- **THEN** the registration fails with `Product 'BOLT-M6' is already registered.`
- **AND** `BOLT-M6` is still "Hex bolt M6" with 0 on hand and reorder level 10

### Requirement: Receiving stock
Receiving stock SHALL add the quantity to the quantity on hand of a registered product. Receiving stock for an unregistered SKU SHALL fail with `Product '{SKU}' is not registered.`

#### Scenario: Delivery
- **GIVEN** `BOLT-M6` has 25 on hand
- **WHEN** 15 more are received
- **THEN** the command succeeds and `BOLT-M6` has 40 on hand

#### Scenario: Unregistered SKU
- **WHEN** stock is received for a SKU that was never registered
- **THEN** the command fails with `Product '{SKU}' is not registered.`

### Requirement: Shipping stock
Shipping SHALL subtract the quantity from the quantity on hand. Shipping more than is on hand SHALL fail with `Cannot ship {quantity} of '{SKU}': only {on hand} on hand.` and change nothing. Shipping for an unregistered SKU SHALL fail with `Product '{SKU}' is not registered.`

#### Scenario: Shipment within stock
- **GIVEN** `WASHER-M6` has 30 on hand
- **WHEN** 27 are shipped
- **THEN** 3 are left on hand

#### Scenario: Shipment larger than stock
- **GIVEN** `BOLT-M6` has 5 on hand
- **WHEN** 8 are shipped
- **THEN** the command fails with `Cannot ship 8 of 'BOLT-M6': only 5 on hand.`
- **AND** both the stored product and its stock level still show 5 on hand

### Requirement: Command results
Every command SHALL report only success, or failure with one error message, and SHALL return no other data. A failed command SHALL change nothing.

#### Scenario: Results carry no data
- **WHEN** a product is registered, received or shipped
- **THEN** the result says only whether the command succeeded and, if it failed, why

### Requirement: Malformed commands
Registering a product with a blank SKU or name or a negative reorder level, and receiving or shipping a quantity below 1 for a registered product, SHALL be rejected as a caller error before any state changes. Receiving and shipping SHALL check that the SKU is registered before checking the quantity.

#### Scenario: Zero quantity
- **WHEN** 0 units of a registered product are received or shipped
- **THEN** the command is rejected as a caller error
- **AND** the stock is unchanged

### Requirement: Stock level lookup
Looking up a SKU SHALL return its stock-level row (the SKU as registered, the name, the quantity on hand, the reorder level and a low-stock flag) reflecting every successful command so far, or nothing for an unregistered SKU.

#### Scenario: Lookup after deliveries
- **GIVEN** `BOLT-M6` ("Hex bolt M6", reorder level 10) has received 25 and then 15
- **WHEN** `BOLT-M6` is looked up
- **THEN** the row is `BOLT-M6`, "Hex bolt M6", 40 on hand, reorder level 10, not low on stock

### Requirement: Low-stock list
A product SHALL be low on stock when its quantity on hand is at or below its reorder level. The low-stock list SHALL contain exactly those products, lowest quantity on hand first, then by SKU.

#### Scenario: Reorder screen
- **GIVEN** `BOLT-M6` has 11 on hand (reorder level 10), `NUT-M6` 10 (10), `WASHER-M6` 3 (20) and `SCREW-M4` 0 (5)
- **WHEN** the low-stock list is requested
- **THEN** it contains `SCREW-M4`, `WASHER-M6` and `NUT-M6`, in that order, as full stock-level rows

### Requirement: Queries change nothing
Looking up a stock level and listing low stock SHALL NOT change any product or stock level.

#### Scenario: Repeated queries
- **WHEN** the stock level and the low-stock list are requested any number of times
- **THEN** every product and stock level stays as the last successful command left it
