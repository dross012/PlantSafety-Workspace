# SQL Shape Audit — Reporting

Phase 6 of 10. **Per-transition SQL shape analysis does not apply to this module.** Per
`module-inventory/reporting.md` and `command-handler-map/reporting.md` (inherited from
`docs/ingestion/module-flow/reporting.md`, not re-derived this phase), Reporting has no domain
aggregate of any kind, no repository/DbContext write path, and no lifecycle to begin with — there is no
Create transition for the module's other transitions to be compared against, and therefore nothing for
this phase's field-set/nullability/enum-drift/truncation-risk questions to be asked of.

## Why the standard shape checks are inapplicable here

Reporting is 5 query-style endpoints behind 1 controller. Every one of them either renders catalog/
metadata already persisted by some *other* module, or generates a PDF/export in-memory and streams it
back in the same HTTP response. Nothing is written to any database from within this module:

- **Field-Set Match?** — N/A. There is no command, no domain entity, and no stored procedure parameter
  list on the write side to compare — Reporting only reads (from other modules' tables, via other
  modules' read procedures) and renders.
- **Nullability Match?** — N/A, for the same reason: no column is ever written by this module.
- **Enum Drift?** — N/A. Reporting has no enum-shaped field of its own that could drift from a
  SQL-side CHECK constraint; any enum values it displays belong to and are validated by the module that
  owns the underlying data.
- **Truncation Risk?** — N/A. No `AddString`/Dapper parameter-binding write call exists anywhere in
  this module to carry a truncation risk.

## What Reporting's real problem is instead (inherited, not re-verified this phase)

Per the parent discovery, Reporting's one defining problem is **missing authentication on its
endpoints**, not lifecycle or shape coverage. This file exists to close out the 25-file `sql-shape`
index consistently with the other 24 modules, not to report a shape finding — there genuinely is none
to report for this module, mirroring `module-inventory/reporting.md`'s and
`command-handler-map/reporting.md`'s identical framing.
