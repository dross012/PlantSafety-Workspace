# SQL Shape Audit — AccidentManagement

Phase 6 of 10 (Lifecycle Coverage Audit). Per-transition DTO/domain→SQL shape, going deeper than
the Ingestion Discovery's aggregate-level shape audit (`docs/ingestion/shape-audit/plantsafety-shape.md`),
which already traced this feature area's Create path in full. This file extends that trace to every
non-Create transition using `usp_Accident_Update`.

## Worst issues in this module

1. **`ConcurrencyToken` is a dead optimistic-concurrency column** on every transition, not just Create —
   `usp_Accident_Update` regenerates it (`NEWID()`) on every one of the 6 non-Create transitions below,
   but nothing ever reads/compares the prior value first. Two concurrent `AssignInvestigator` calls (or
   any other transition) race silently, last-write-wins. Directly verified (inherited from ingestion
   shape-audit, re-confirmed applicable to every `_Update`-routed transition this phase, not just Create).
2. **`UpdatedBy` is always the original `CreatedBy`, never the actual acting user**, on every non-Create
   transition — `AccidentDapperRepository.cs:151` binds `@UpdatedBy` to `aggregate.CreatedBy.Value`
   unconditionally. `AssignInvestigator`/`RecordRootCause`/`CreateCorrectiveAction`/
   `CompleteCorrectiveAction`/`FileRegulatoryReport`/`CloseInvestigation`/`Archive` all share this one
   `UpdateAsync` call site, so every one of them writes a false "who updated this" value. Directly
   verified.
3. **`CorrectiveAction.Description` (2000 chars) has no confirmed validator this phase** —
   `AccidentInvestigationCommandValidators.cs` was checked for every other investigation-lifecycle field
   (`Notes` 4096, `Description` [AssignInvestigator's, 1024], `CompletionEvidence` 1024,
   `RegulatoryAgency`/`ReferenceNumber` 128 each) and all of those are present and stricter than their
   column, but `CreateCorrectiveActionCommand.Description` itself has no matching `RuleFor(...).
   MaximumLength(...)` found in this file — a real (if modest, 2000-char) truncation-risk gap, distinct
   from the well-validated fields around it in the same command family.
4. **`Category`/`Severity`/`RootCause` have zero CHECK constraints** — the smart-enum `.Code` value is
   the only thing keeping `NVARCHAR` columns in sync with what `AccidentSeverity.From`/equivalent can
   parse; a future code rename silently orphans historic rows at read time. Structurally possible,
   directly verified absent (inherited).
5. **No transition in this module is exposed to the classic silent-truncation bug** — every field this
   phase re-checked has a validator stricter than or equal to its column/param size (the positive
   pattern the Top-5 ingestion finding already called out for `AccidentTitle`/`Location`), **except**
   `CorrectiveAction.Description` above.

## Method

`Accident.Create`/all 6 investigation-lifecycle transitions traced in the Ingestion Discovery's
shape-audit (Create) plus this phase's read of `AccidentInvestigationCommandValidators.cs` in full and
re-confirmation that all 6 non-Create transitions route through the single `usp_Accident_Update`
(command-handler-map, inherited F10). Outbox provenance corruption (F1/Top-5 #1: `"system"`/
`Guid.Empty` on every `AddAsync`/`UpdateAsync` call) and the module-wide AuditLog coverage (F8: Yes,
best-effort/anonymous, since Accident is 1 of the 23 covered aggregates) apply identically to every row
below and are not repeated per-row.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Create (Report Accident) | Yes — command/validator/column shapes align 1:1 for every field traced (inherited full trace) | Yes, all traced fields | Structurally possible (no CHECK on Severity/Category/RootCause), not confirmed drifted | No — validator (256) stricter than column (400) for Title/Location; ample headroom elsewhere | Directly verified (inherited) | See Top-5 #4/#5 in the ingestion shape-audit |
| AssignInvestigator | Yes (`Notes` field only; matches `usp_Accident_Update`'s param list) | Yes | N/A (no enum field changes) | No — `Notes` validator `MaximumLength(4096)` vs param size, not individually resized this phase but no smaller cap found | Directly verified (validator), pattern-inferred (param size) | Routes through shared `usp_Accident_Update` — see Top-5 #1/#2 above |
| RecordRootCause | Yes (`Description` field) | Yes — `RootCause`/`RootCauseNotes` nullable in both C# and SQL (`NULL` columns), consistent | Structurally possible on `RootCause` code | No — validator `MaximumLength(1024)` | Directly verified | |
| CreateCorrectiveAction (child entity) | Yes (`Description`, `VerificationSteps`) | Yes | N/A | **Unconfirmed / likely gap** — no validator found this phase for `CreateCorrectiveActionCommand.Description` (2000-char column, no matching `RuleFor`) | Directly verified (validator absence) | Worst-issue #3 |
| CompleteCorrectiveAction | Yes (`CompletionEvidence`) | Yes | N/A | No — validator `MaximumLength(1024)` | Directly verified | Does not re-check parent `Accident.Status` before completing — inherited functional gap, not a shape issue |
| FileRegulatoryReport | Yes (`RegulatoryAgency`, `ReferenceNumber`) | Yes | N/A | No — validator `MaximumLength(128)` each, ample headroom to the 200/100-char columns | Directly verified | |
| CloseInvestigation | Yes (no new fields beyond `Status`) | Yes | Structurally possible on `Status` (no CHECK found) | N/A (no string field) | Directly verified | Best-guarded transition in the solution (enforces `RegulatoryFilingRequired`) |
| Archive | Yes (`Status` only) | Yes | Same as above | N/A | Directly verified | Silent-archive — no `AddDomainEvent` (inherited) |
