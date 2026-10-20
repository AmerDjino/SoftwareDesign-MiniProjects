# invoice-storage Specification

## Purpose
Stores invoices and answers the billing module's questions about them with identical behaviour in every store, so business rules can be tested against an in-memory store and run against a database.

## Requirements

### Requirement: Storing and loading invoices
Storage SHALL save a new invoice immediately and SHALL return it with every field intact when it is loaded by its id or by its exact number. Loading by an unknown id or number SHALL return nothing.

#### Scenario: Load a new invoice in a fresh session
- **GIVEN** the invoice `INV-2026-0001` for Acme d.o.o., 1250.50, due on 2026-10-31, has been added
- **WHEN** it is loaded by id and by number, each time through a new storage session
- **THEN** both loads return the same id, number, customer, amount, due date and payment date

#### Scenario: Unknown number
- **WHEN** an invoice is loaded by a number that was never stored
- **THEN** nothing is returned

### Requirement: Overdue follow-up list
Storage SHALL list the invoices that are overdue on a given date, as defined by the invoice-payments capability, oldest due date first.

#### Scenario: Mixed invoices
- **GIVEN** invoices due on 2026-08-23 (unpaid), 2026-09-07 (paid), 2026-10-06 (unpaid), 2026-10-07 (unpaid) and 2026-10-21 (unpaid)
- **WHEN** the overdue list for 2026-10-07 is requested
- **THEN** it contains exactly the invoices due on 2026-08-23 and 2026-10-06, in that order

### Requirement: Saving changes
A change to an invoice SHALL be stored only when that invoice is explicitly updated. Until then storage SHALL keep returning the previously stored state, whether the change was made to the object that was added or to one that was loaded.

#### Scenario: Unsaved and saved payment
- **GIVEN** an unpaid invoice has been added and then loaded
- **WHEN** both the added object and the loaded object are marked as paid on 2026-10-07 without an update
- **THEN** a new storage session still loads the invoice as unpaid
- **AND** after the loaded object is updated, a new session loads it as paid on 2026-10-07

### Requirement: Same behaviour in every store
Every invoice store SHALL meet the requirements of this capability in the same way; in particular the in-memory store used by tests SHALL behave like the database store.

#### Scenario: Shared contract
- **WHEN** the scenarios "Load a new invoice in a fresh session", "Mixed invoices" and "Unsaved and saved payment" are run against the in-memory store and against the database store
- **THEN** both stores pass all three
