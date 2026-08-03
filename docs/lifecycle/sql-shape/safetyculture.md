# SQL Shape Audit — SafetyCulture

Phase 6 of 10. Covers `CultureAssessment` (the write path the Ingestion Discovery's shape-audit added
specifically for this reason) and `StopWorkEvent`. Not to be confused with `Analytics.Culture` (a
separate, unrelated, zero-SQL-confirmed marker — see `analytics.md`).

## Worst issues in this module

1. **`CreateCultureAssessmentCommand.AssessmentTitle` — confirmed truncation risk, same 256-vs-200
   pattern.** `CreateCultureAssessmentCommandValidator.cs:21`: `MaximumLength(256)`.
   `CultureAssessmentDapperRepository.cs:90`: `.AddString("AssessmentTitle", aggregate.AssessmentTitle,
   200)`. Directly verified.
2. **`CreateStopWorkEventCommand.WorkArea` — confirmed truncation risk, same pattern, a 6th solution-wide
   instance.** `CreateStopWorkEventCommandValidator.cs:21`: `MaximumLength(256)`.
   `StopWorkEventDapperRepository.cs:90`: `.AddString("WorkArea", aggregate.WorkArea, 200)`. Directly
   verified. This is the **fourth distinct command** (after `AuditEngagement`, `AuditFinding`,
   `ChecklistTemplate`) found this phase with the identical 256-vs-200 shape — strong evidence this is a
   **systemic convention mismatch** (someone standardized command validators on 256 for "title-shaped"
   fields at some point, while the persistence layer's `AddString` calls for the same conceptual field
   were written against an older or different 200-char convention), not four independent one-off typos.
3. **`RelatedRiskManagementHazardId` (SafetyCulture.CultureAssessment, `UNIQUEIDENTIFIER NULL`, per the
   inherited ingestion shape-audit) has no FK constraint and was not confirmed this phase to be populated
   by any live transition** — a candidate dead/optional cross-reference field, consistent with F15's
   observation that cross-module references throughout PlantSafety are stored on faith with no
   referential-integrity enforcement.
4. **`OverallCultureScore`/`LeadershipEngagementScore` (`DECIMAL(5,2) NULL`)** — generous headroom
   (caps at 999.99) for what is presumably a 0–100 score, with no CHECK constraint enforcing the valid
   range; not a truncation risk (numeric, not string) but the same "smart-enum/range trusted only at the
   app layer" shape as everywhere else in the module. Inherited, not re-verified this phase.
5. **No CHECK constraint found on either aggregate's `Status` column.**

## Method

Direct read this phase: `CreateCultureAssessmentCommandValidator.cs`,
`CreateStopWorkEventCommandValidator.cs`, `CultureAssessmentDapperRepository.cs`,
`StopWorkEventDapperRepository.cs` (`AddString` call sites). Cross-referenced against the solution-wide
sweep used in `auditing.md`/`checklists.md`.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| CultureAssessment.Create | Yes | `OverallCultureScore`/`LeadershipEngagementScore` nullable in both command and column (inherited) — matches | Structurally possible, no CHECK found | **Yes — confirmed** (`AssessmentTitle` 256 validator vs 200 param) | Directly verified | Worst-issue #1 |
| CultureAssessment.RecordScore | Yes (numeric fields, range-checked 0–100 — a positive control, inherited) | Yes | N/A (numeric) | N/A | Directly verified (inherited) | |
| CultureAssessment.PublishReport | Yes (`Summary` field per `PublishCultureAssessmentReportCommandValidator.cs:21`, `MaximumLength(4096)`) | Not verified | Same | No — column size for `Summary` not confirmed smaller than 4096 this phase | Directly verified (validator), pattern-inferred (column) | |
| CultureAssessment.Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
| StopWorkEvent.Create | Yes (`WorkArea`, `HazardDescription` 2000) | Not verified | Same | **Yes — confirmed** (`WorkArea` 256 validator vs 200 param) | Directly verified | Worst-issue #2 |
| StopWorkEvent.Investigate | Yes (`Findings`, `MaximumLength(2048)` per `InvestigateStopWorkEventCommandValidator.cs:21`) | Not verified | Same | No — column size not confirmed smaller than 2048 this phase | Directly verified (validator), pattern-inferred (column) | |
| StopWorkEvent.Resolve | Yes (`ResolutionSummary`, `MaximumLength(2048)` per `ResolveStopWorkEventCommandValidator.cs:21`) | Not verified | Same | No — same as Investigate | Directly verified (validator), pattern-inferred (column) | |
| StopWorkEvent.Archive/Cancel/Delete | N/A | N/A | N/A | N/A | Directly verified | |
