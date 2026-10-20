# Proposal

## Why

The invoicing behaviour of `03-RepositoryPattern` is written down only in its `docs/SPEC.md`, which was written before the code and is kept as a record of that step. Later changes, such as defining what storage does with a duplicate invoice number, need a living spec to propose deltas against, so this change records the behaviour that the code and its tests already have.

## What Changes

- Add the `invoice-payments` spec: when an invoice is overdue, and how a payment is recorded or rejected.
- Add the `invoice-storage` spec: the contract every invoice store meets (storing and loading, the overdue list, saving changes, and identical behaviour of the in-memory and database stores).
- Link both specs from `03-RepositoryPattern/README.md` and from the header of `03-RepositoryPattern/docs/SPEC.md`.
- No code, test or behaviour changes.

## Capabilities

### New Capabilities
- `invoice-payments`: the overdue rule and recording payments against invoice numbers.
- `invoice-storage`: storing invoices and answering the billing module's questions about them, identically in every store.

### Modified Capabilities

None.

## Impact

- Mini-project: `03-RepositoryPattern` (documentation only).
- After archiving: `openspec/specs/invoice-payments/spec.md` and `openspec/specs/invoice-storage/spec.md`.
