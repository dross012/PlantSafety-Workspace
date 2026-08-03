# Domain Mapping — Intelligence

Not independently re-verified this phase — inherited from the prior Ingestion Discovery's full
handler reads. Included here for completeness since Intelligence defines the reference case (F2)
against which this phase tested PlantSafety's new clusters.

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| TrainingJob | Submit (Create) | Domain guards exist in `TrainingJob.Create` | **N — confirmed dead**: handler calls `_repository.CreateAsync` directly, never constructs `TrainingJob` | N/A — guard never executes | None | Not enforced (never reached) | Directly verified (inherited) | |
| TrainingJob | RegisterDataset | Domain method exists | Unreached — no handler found calling it | N/A | None | Not enforced | Directly verified (aggregate); pattern-inferred (no caller) | |
| TrainingJob | Start | Domain method exists | **N — confirmed dead**, bypassed same as Create | N/A | None | Not enforced | Directly verified (inherited) | |
| TrainingJob | Complete | Domain method exists | Unconfirmed whether any handler calls it | N/A | None | Not enforced | Pattern-inferred | |
| TrainingJob | Cancel | Terminal-state check, non-empty reason required — real guard confirmed to exist | **N — confirmed dead**: `CancelTrainingJobCommandHandler` calls `_repository.CancelAsync` directly; guard never executes at runtime | N/A | None | Not enforced | Directly verified (inherited) | Clearest "looks guarded, isn't" case in the solution |
| TrainingJob | RecordEvaluation | Domain method exists | **N — confirmed dead**, same bypass shape | N/A | None | Not enforced | Directly verified (inherited) | |
| TrainingJob | Fail | Domain method exists | Unreached — no handler found calling it | N/A | None | Not enforced | Pattern-inferred | |
| TrainingJob | Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| Investigation | Open (Create) | — | **N — confirmed dead**: `RunInvestigationCommandHandler` calls the repository's create method directly | N/A | None | Not enforced | Directly verified (inherited) | No event of any kind confirmed |
| Investigation | AddFinding | Domain method exists (guarded, 2 throw sites) | Unconfirmed whether any handler calls it | 2 guard sites confirmed present | None | Not enforced | Pattern-inferred | |
| Investigation | SubmitHypothesis | Domain method exists (guarded) | Unconfirmed | — | None | Not enforced | Pattern-inferred | |
| Investigation | Cancel | Domain guard exists | **N — confirmed dead**: `CancelInvestigationCommandHandler` calls `_repository.CancelAsync` directly | N/A | None | Not enforced | Directly verified (inherited) | Same bypass pattern as `TrainingJob.Cancel` |
| Investigation | Complete | Domain method exists (guarded) | Unconfirmed | — | None | Not enforced | Pattern-inferred | |
| Investigation | Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

**Summary — reference case for F2:** every reachable mutating transition in this module (5 of 13)
is confirmed dead code from a guard-enforcement perspective — the C# guard is real and would fire,
but the live handler bypasses the aggregate entirely and calls the repository directly. This phase's
direct evidence (F2) confirms this specific shape does **not** recur in the 4 newly-examined
PlantSafety clusters (ContractorSafety/EmployeeSafety/MachineSafety/SafetyProcedures) or in Auditing
— Intelligence remains the sole confirmed instance of "guard reached-and-bypassed"; F4's
`Operational.Alerts.Alert` and `PMScheduling.Cancel` are a related-but-distinct category (guard
never reached at all, no route exists, rather than reached-and-ignored).
