# SQL Shape Audit — RiskManagement

Phase 6 of 10. Covers `RiskManagement.Aggregates.Risk` (live). `Operational.RiskManagement.Aggregates.
RiskManagement` (create-only duplicate, GET-only controller, no write proc found at all per
`module-inventory/riskmanagement.md`) has no write path to shape-audit.

## Worst issues in this module

1. **Only 2 validator files exist for this module** (`CreateRiskCommandValidator.cs` and one other) —
   `CreateRiskCommandValidator.cs` covers `RiskTitle` (`MaximumLength(256)`), `Location` (`MaximumLength
   (256)`), `HazardSource` (`MaximumLength(128)`); none of the other 7 non-Create transitions
   (ScoreRisk/Prioritize/ApplyControlMeasure/ValidateEffectiveness/StartReviewCycle/CompleteReview/Close)
   have a dedicated validator file, meaning any free-text field on those transitions (e.g. a
   control-measure justification, a review-cycle note) is unvalidated.
2. **`RiskTitle`/`Location`/`HazardSource` param sizes checked and confirmed safe** — direct read of
   `RiskDapperRepository.cs` finds `AddString("RiskTitle", ..., 400)`, `AddString("Location", ..., 400)`,
   `AddString("HazardSource", ..., 400)` — all comfortably wider than the 256/256/128-char validator caps.
   Unlike Auditing/Checklists/SafetyCulture, this module does **not** exhibit the 256-vs-200 mismatch —
   a positive control, directly verified.
3. **`Archive` is a 3rd confirmed silent-archive instance** (inherited) — guarded, never calls
   `AddDomainEvent`, same pattern as Accident/Scorecard.
4. **No CHECK constraint found on `Risk`'s status/level-shaped columns.**
5. **`Operational.RiskManagement` duplicate has no write proc at all** (inherited, re-confirmed this
   phase's SQL check) — a `Create` call against this class, if ever wired, would have nowhere to persist.

## Method

Not individually re-opened this phase beyond `CreateRiskCommandValidator.cs`'s field list (already
captured in the solution-wide validator sweep) and the inherited Ingestion Discovery trace (`Risk.Create`
directly verified there). The 7 non-Create transitions' field-level shape is **pattern-inferred** from
`command-handler-map/_shared-findings.md` F10's consolidated-handler convention, not individually
re-opened.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Risk.Create (Identify hazard) | Yes — `RiskTitle`, `Location`, `HazardSource` all validated | Not individually re-verified this phase | Structurally possible, no CHECK found | **No** — validator (256/256/128) stricter than param sizes (400/400/400) | Directly verified | Worst-issue #2 (resolved safe) |
| Risk.ScoreRisk | Field set not re-opened | Not verified | Same | Not individually re-verified — no validator file found for this transition | Pattern-inferred | |
| Risk.Prioritize | Same | Not verified | Same | Not individually re-verified | Pattern-inferred | F15: event carries new `Priority` only, no "escalated from" value (inherited) |
| Risk.ApplyControlMeasure | Same | Not verified | Same | Not individually re-verified | Pattern-inferred | Real cross-field rule: justification required for lower-effectiveness control hierarchy (inherited) |
| Risk.ValidateEffectiveness | Same | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| Risk.StartReviewCycle / CompleteReview | Same | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| Risk.Close | Same | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| Risk.Archive | Same | Not verified | Same | Not individually re-verified | Directly verified (silent-archive, inherited); pattern-inferred (shape) | Worst-issue #3 |
| Risk.Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
| Operational.RiskManagement.Create (duplicate) | N/A — no write proc exists | N/A | N/A | N/A — write would fail/no-op before any field-shape question applies | Directly verified (inherited) | Worst-issue #5 |
