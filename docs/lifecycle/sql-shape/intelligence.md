# SQL Shape Audit — Intelligence

Phase 6 of 10. Covers `TrainingJob` (AITraining) and `Investigation` (AIInvestigations). Per
`command-handler-map/intelligence.md`, this module's headline pattern is unique: the handler bypasses
the domain aggregate entirely and calls the repository directly, so any shape mismatch between the
*domain* aggregate's fields and the SQL parameters is moot — the domain object is never constructed on
the write path at all. This phase's shape check therefore compares the **command's** field set (which
the bypassing handler does use) against the repository's Dapper parameter list, directly read this
phase in `TrainingJobRepository.cs`. Like DataLake, Intelligence keeps no `.sql` files under source
control here, so proc bodies are unverifiable; unlike DataLake, the stored procedure **names** are
centralized in a `private static class Sp` block, directly confirmed.

## Worst issues in this module

1. **Intelligence's parameter-binding pattern also does not exhibit PlantSafety's silent-truncation
   bug.** Direct read of `TrainingJobRepository.cs`'s `CreateAsync`/`CancelAsync`/`RecordEvaluationAsync`
   shows plain anonymous-object Dapper binding (`new { ModelName = modelName, ... }`) — no explicit
   `size` argument anywhere, same "auto-size from value" behavior as DataLake's pattern, and the same
   structural conclusion: no confirmed silent-truncation risk, only the (unconfirmed, proc-body-
   dependent) opposite risk of a loud SQL-side overflow error. This is the 2nd module this phase
   confirms does not share PlantSafety's specific `AddString(name,value,size)` defect.
2. **Every one of `TrainingJob`'s and `Investigation`'s writable transitions is guarded by a
   FluentValidation validator with real content-level rules** — `CancelTrainingJobCommandValidator.cs`
   (directly read) requires `TrainingJobId`/`CancelledByUserId` non-empty, `CancellationReason`
   non-empty **and** `MaximumLength(2000)`, `ApiVersionToken` non-empty, `RequestedAtUtc` a real
   timestamp. This is materially stronger than the AccidentManagement-style gap this document set has
   otherwise found (`CreateCorrectiveActionCommand.Description` with no validator, `accidentmanagement.md`
   #3) — Intelligence's command layer does not appear to share PlantSafety's uneven validator coverage.
3. **The domain-bypass pattern (F-equivalent of `command-handler-map/intelligence.md`'s headline
   finding) makes "Field-Set Match?" a two-part question that the rest of this document set's rows don't
   have to ask**: (a) does the *command* match the *repository parameters* — checked below, generally
   Yes; and (b) does the *domain aggregate's* method signature (which is never invoked) match either side
   — structurally irrelevant, since dead code's shape can't drift against a live path it never touches.
   Row-level notes below flag both where useful.
4. **`RecordEvaluationAsync`'s `Passed` (bool) and `EvaluationSummary` (string, no `MaximumLength` found
   in a not-fully-opened validator this phase) travel with no confirmed cap** — `EvaluateTrainingJobCommandValidator.cs`
   exists per directory listing but was not opened this phase to confirm whether `EvaluationSummary` has
   a `MaximumLength` rule; flagged as an open item rather than a confirmed gap, since the repository call
   itself (directly read) shows no fixed-size binding either way.
5. **The one event that is raised correctly on the one non-bypassing handler
   (`DataLakeTransformationCompletedIntegrationEvent`, `TransformDataCommandHandler`) still never
   delivers** — `NullOutboxPublisher` marks the row `Processed=true` without publishing (inherited,
   `command-handler-map/intelligence.md`). Not a shape issue, but the closest this module comes to an
   end-to-end-attempted write, and it actively misreports success — worth restating here since it's the
   one transition in this module where the domain method plausibly *does* execute.

## Method

Direct read of `TrainingJobRepository.cs` in full (`FindAsync`/`CreateAsync`/`CancelAsync`/`ListAsync`/
`RecordEvaluationAsync`) and `CancelTrainingJobCommandValidator.cs` in full. `InvestigationRepository.cs`
and the other Intelligence validators listed by directory search (`RunInvestigationCommandValidator.cs`,
`SubmitHypothesisCommandValidator.cs`, `StartTrainingJobCommandValidator.cs`,
`EvaluateTrainingJobCommandValidator.cs`) were not individually opened this phase; their rows are
pattern-inferred from the `CancelTrainingJobCommandValidator`/`TrainingJobRepository` example and the
inherited `command-handler-map/intelligence.md` bypass finding.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| TrainingJob.Submit (Create) | Yes — `CreateAsync`'s 5 params (`ModelName`, `DatasetHash`, `RequestedByUserId`, `RequestedAtUtc`, `CorrelationId`) match the command's field set 1:1 per the handler's direct pass-through (domain `TrainingJob.Create`'s own signature is never invoked — bypass, see worst-issue #3) | Yes, all 5 non-nullable at the C# call site | N/A — no enum-shaped field on Create | **No** — Dapper anonymous-object binding, no fixed-size cap (worst-issue #1) | Directly verified (repository) | Domain guards on `TrainingJob.Create` never execute (inherited) |
| TrainingJob.RegisterDataset | N/A — no handler found calling it | N/A | N/A | N/A | Directly verified (inherited) | Fully dead domain method |
| TrainingJob.Start | Not individually re-opened this phase | Not verified | Not verified | Pattern-inferred No | Pattern-inferred | Same bypass pattern (inherited) |
| TrainingJob.Complete | Not confirmed whether any handler calls it | Unconfirmed | Unconfirmed | Unconfirmed | Unconfirmed (inherited) | |
| TrainingJob.Cancel | Yes — `CancelAsync`'s 5 params match `CancelTrainingJobCommand`'s field set 1:1 (directly read both sides) | Yes | N/A | **No** — same anonymous-object binding; `CancellationReason` additionally capped client-side at `MaximumLength(2000)` (worst-issue #2) | Directly verified (repository + validator, both read in full) | Best-evidenced row in this file — domain guard (terminal-state check) still never executes despite the strong C#-side validation, per the bypass pattern |
| TrainingJob.RecordEvaluation | Yes — `RecordEvaluationAsync`'s 5 params match the command's likely field set (`TrainingJobId`, `Passed`, `EvaluationSummary`, `EvaluatedAtUtc`, `CorrelationId`), command side not directly opened | Yes for the 4 non-string fields; `EvaluationSummary`'s cap unconfirmed (worst-issue #4) | N/A | **Unconfirmed** — no fixed-size binding in the repository call itself, but `EvaluationSummary`'s validator rule not opened this phase | Directly verified (repository); validator not opened | Open follow-up: confirm `EvaluateTrainingJobCommandValidator.cs`'s `EvaluationSummary` rule |
| TrainingJob.Fail | No handler found calling it | N/A | N/A | N/A | Pattern-inferred (inherited) | |
| TrainingJob.Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| Investigation.Open (Create) | Not individually re-opened this phase (`InvestigationRepository.cs` not read) | Not verified | Not verified | Pattern-inferred No (same repository-base pattern) | Pattern-inferred | Bypass pattern inherited; no event of any kind confirmed even in the bypassed path |
| Investigation.AddFinding / SubmitHypothesis / Complete | Domain methods exist (guarded), no handler confirmed calling any of them | Not verified | Not verified | Pattern-inferred No | Pattern-inferred (inherited) | |
| Investigation.Cancel | Not individually re-opened (same bypass shape as `TrainingJob.Cancel`, not confirmed field-for-field) | Not verified | Not verified | Pattern-inferred No | Pattern-inferred | |
| Investigation.Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
