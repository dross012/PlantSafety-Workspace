# SQL Shape Audit — Scorecards

Phase 6 of 10. Covers `Scorecards.Scorecard` (1 aggregate, live). `Analytics.ScoreCards` (create-only
duplicate, SQL ownership ambiguous) covered in `analytics.md`.

## Worst issues in this module

1. **`RecordDistributionCommand.Channel` is validated (`MaximumLength(128)`) but never persisted at
   all** — `ScorecardCompilationCommandValidators.cs:77`: `RuleFor(x => x.Channel).NotEmpty().
   MaximumLength(128)`. `ScorecardDapperRepository.cs` read in full: no `Channel`-named column or
   `AddString` call exists anywhere in the file. A 4th instance (alongside `SafetyPolicies.Reject`/
   `Retire`'s `Reason`, `Scheduling.WorkOrderPriority.Escalate`'s `EscalationReason`, `PPE.PPEInventory.
   FlagForReplacement`'s `Reason`) of a fully-validated field being silently dropped before it reaches
   durable storage — "who was this scorecard distributed to/through" is unrecoverable from the
   `Scorecard` table itself, only from the (unconsumed) `ScorecardDistributedEvent`. Directly verified.
2. **`RecordDistribution` also carries the module's pre-existing exception-message copy-paste bug**
   (inherited) — cosmetic, not a data-shape issue, but co-located with issue #1 on the same transition.
3. **`ScorecardTitle`/`DataSourceModule` are genuinely safe** — validator (256/128) stricter than param
   (400/200) in both cases, confirmed by direct read. A positive control, notable because 2 of this
   module's 3 validated free-text fields are fine and the 3rd (`Channel`) isn't even attempted at the
   persistence layer — a "some fields get real care, one gets none at all" pattern rather than a uniform
   gap.
4. **`Archive` is a 4th confirmed silent-archive instance** (inherited) — guarded, no `AddDomainEvent`.
5. **No CHECK constraint found on `Scorecard.Status`/`Category`/`PeriodType`.**

## Method

Direct read this phase: `ScorecardCompilationCommandValidators.cs` (all `MaximumLength` rules),
`ScorecardDapperRepository.cs` (in full, every `AddString` call site across `AddAsync`/`UpdateAsync`/the
KPI child-table write).

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Scorecard.Create (Draft) | Yes — `ScorecardTitle` present | Not individually re-verified | Structurally possible, no CHECK found | No — validator (256) stricter than param (400) | Directly verified | Worst-issue #3 |
| Scorecard.RecordMetric | Yes — `DataSourceModule`, `Metric` (KPI child table) present | Not verified | Same | No — validator (128) stricter than param (200) | Directly verified | |
| Scorecard.CompleteCompilation | Field set not re-opened (no new string field expected) | Not verified | Same | N/A | Pattern-inferred | Real rule: rejects compiling with zero measurements (inherited) |
| Scorecard.Publish | Field set not re-opened | Not verified | Same | N/A | Pattern-inferred | |
| Scorecard.RecordDistribution | **No — `Channel` captured/validated, never persisted to any column** | N/A | N/A | N/A (field never reaches a string param) | Directly verified | Worst-issue #1 |
| Scorecard.Close | Field set not re-opened | Not verified | Same | N/A | Pattern-inferred | |
| Scorecard.Archive | Field set not re-opened | Not verified | Same | N/A | Directly verified (silent-archive, inherited); pattern-inferred (shape) | Worst-issue #4 |
| Scorecard.Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
