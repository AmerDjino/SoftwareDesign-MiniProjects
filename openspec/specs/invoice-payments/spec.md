# invoice-payments Specification

## Purpose
Decides when an invoice is overdue and records payments against invoice numbers, so that every invoice is paid exactly once and the follow-up list of overdue invoices is correct.

## Requirements

### Requirement: Overdue invoices
An invoice SHALL be overdue on a date when it is unpaid and its due date is before that date. The due date is the last day to pay, and a paid invoice SHALL never be overdue.

#### Scenario: Due the day before
- **WHEN** an unpaid invoice due on 2026-10-06 is checked on 2026-10-07
- **THEN** it is overdue

#### Scenario: Due on the day
- **WHEN** an unpaid invoice due on 2026-10-07 is checked on 2026-10-07
- **THEN** it is not overdue

#### Scenario: Paid after its due date
- **WHEN** an invoice due on 2026-09-07 and paid on 2026-09-27 is checked on 2026-10-07
- **THEN** it is not overdue

### Requirement: Recording a payment
Recording a payment for an invoice number SHALL mark the unpaid invoice with that number as paid on the given date, store it, and report it as paid.

#### Scenario: Unpaid invoice
- **GIVEN** the unpaid invoice `INV-001`
- **WHEN** a payment for `INV-001` on 2026-10-07 is recorded
- **THEN** the result is paid
- **AND** the stored invoice shows 2026-10-07 as its payment date

### Requirement: Rejected payments
Recording a payment SHALL be rejected as not found when no invoice has the number, and as already paid when the invoice was paid before. A rejected payment SHALL change no stored data, so an invoice keeps its first payment date.

#### Scenario: Unknown invoice number
- **GIVEN** `INV-002`, paid on 2026-10-01, is the only stored invoice
- **WHEN** a payment for `INV-404` is recorded
- **THEN** the result is not found
- **AND** `INV-002` still shows 2026-10-01 as its payment date

#### Scenario: Invoice already paid
- **GIVEN** `INV-002` was paid on 2026-10-01
- **WHEN** a payment for `INV-002` on 2026-10-07 is recorded
- **THEN** the result is already paid
- **AND** `INV-002` still shows 2026-10-01 as its payment date
