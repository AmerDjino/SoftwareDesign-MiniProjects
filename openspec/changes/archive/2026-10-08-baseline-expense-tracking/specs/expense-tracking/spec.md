## Purpose

An HTTP API that lets employees record work expenses and gives finance a monthly total per expense category.

## ADDED Requirements

### Requirement: Submit an expense
`POST /expenses` SHALL accept a description, an amount, a category name and the date the expense was incurred. A valid expense SHALL be stored under a new id and returned with `201 Created` and a `Location` header of `/expenses/{id}`.

#### Scenario: Valid expense
- **GIVEN** today is 2026-10-07
- **WHEN** a client posts `Train to Mostar`, 120.00, `Travel`, incurred on 2026-10-01
- **THEN** the response is `201 Created` with the `Location` header `/expenses/{id}`
- **AND** the body repeats the expense with its new id

### Requirement: Validation response
Every validation rule SHALL be checked on each submission, and all violations SHALL be returned together as a `400 Bad Request` validation problem whose errors are keyed by the JSON property names `description`, `amount`, `category` and `incurredOn`. An invalid expense SHALL NOT be stored.

#### Scenario: Missing and unknown values
- **WHEN** a client posts only `{"category": "Party"}`
- **THEN** the response is `400 Bad Request`
- **AND** its errors are keyed `amount`, `category`, `description` and `incurredOn`

#### Scenario: Values out of range
- **GIVEN** today is 2026-10-07
- **WHEN** an expense with a 201-character description, an amount of 5,000.01, an undefined category and the date 2026-10-08 is submitted
- **THEN** it is rejected with one error for each of the four fields
- **AND** nothing is stored

### Requirement: Description rule
The description SHALL be present, SHALL contain more than whitespace and SHALL be at most 200 characters long.

#### Scenario: Description missing or too long
- **WHEN** the description is missing or has 201 characters
- **THEN** the submission is rejected with a `description` error

### Requirement: Amount rule
The amount SHALL be greater than 0.00 and at most 5,000.00.

#### Scenario: Amount out of range
- **WHEN** the amount is 0.00 or 5,000.01
- **THEN** the submission is rejected with an `amount` error

### Requirement: Category rule
The category SHALL be one of `Travel`, `Meals`, `Office`, `Training` and `Other`, matched by name ignoring letter case. Numbers and combinations such as `Travel, Meals` SHALL be rejected.

#### Scenario: Unknown category
- **WHEN** the category is `Party`
- **THEN** the submission is rejected with a `category` error

#### Scenario: Combined category names
- **WHEN** the category is `Travel, Meals`
- **THEN** the submission is rejected with a `category` error

### Requirement: Date rule
The date the expense was incurred SHALL be present and SHALL NOT be later than today, the current UTC date.

#### Scenario: Date in the future
- **GIVEN** today is 2026-10-07
- **WHEN** the expense was incurred on 2026-10-08
- **THEN** the submission is rejected with an `incurredOn` error

#### Scenario: Date missing
- **WHEN** the request has no date
- **THEN** the submission is rejected with an `incurredOn` error

### Requirement: Monthly summary
`GET /expenses/summary/{year}/{month}` SHALL return, for the expenses incurred in that month, the total and the number of expenses per category, only for categories that have expenses, ordered by total descending (ties in the order Travel, Meals, Office, Training, Other), and the grand total. Expenses from other months, including the same month of another year, SHALL be ignored.

#### Scenario: October 2026
- **GIVEN** expenses of 120.00 Travel (2026-10-01), 45.25 Meals (2026-10-02), 350.00 Training (2026-10-06), 80.50 Travel (2026-10-07), 300.00 Office (2026-09-30) and 900.00 Training (2025-10-15)
- **WHEN** the summary for October 2026 is requested
- **THEN** it lists Training 350.00 (1 expense), Travel 200.50 (2) and Meals 45.25 (1), in that order
- **AND** the grand total is 595.75

#### Scenario: Month without expenses
- **WHEN** the summary is requested for a month with no expenses
- **THEN** it lists no categories and the grand total is 0.00

### Requirement: Summary month range
The summary route SHALL match only months 1 to 12, and any other month SHALL get `404 Not Found`.

#### Scenario: Month 13
- **WHEN** a client requests `/expenses/summary/2026/13`
- **THEN** the response is `404 Not Found`
