# Module Inventory — Intelligence

2 aggregates: `TrainingJob` (AITraining), `Investigation` (AIInvestigations). All rows below are
reformatted from `docs/ingestion/lifecycle-coverage/intelligence-lifecycle.md` (directly verified by
full reads of both aggregates and their handlers); not independently re-verified this phase. Intelligence
does not appear to keep its SQL objects under source control in this repository (confirmed by the
prior discovery), so "Exists in SQL?" here means "a named stored procedure is called via Dapper," not
"a `.sql` file was found" — the procedure bodies are unverifiable from this codebase.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| TrainingJob | Submit (Create) | Yes, but bypassed — handler calls `_repository.CreateAsync` directly, never constructs `TrainingJob` | Yes — `intelligence.usp_TrainingJob_Create` (name only, body unverifiable) | **Live handler, dead domain class** | Directly verified | Domain guards never execute at runtime |
| TrainingJob | RegisterDataset | Domain method exists | No handler found calling it | Unreached | Directly verified (aggregate); pattern-inferred (no caller) | Possible additional dead domain method |
| TrainingJob | Start | Domain method exists, bypassed | Unclear whether "start" is even a distinct HTTP action vs. conflated with Create | **Live handler bypasses domain** | Directly verified (aggregate); handler-bypass directly verified | |
| TrainingJob | Complete | Domain method exists | Not confirmed whether any handler calls it | Unconfirmed | Pattern-inferred | |
| TrainingJob | Cancel | Yes, but bypassed — `CancelTrainingJobCommandHandler` calls `_repository.CancelAsync` directly | Yes — `usp_TrainingJob_Cancel` (name only) | **Live handler, dead domain class** — the headline finding of this cluster | Directly verified | Domain guard (terminal-state check, non-empty-reason) never executes; clearest "looks guarded, isn't" case in the solution |
| TrainingJob | RecordEvaluation | Yes, but bypassed — `EvaluateTrainingJobCommandHandler` calls `_repository.RecordEvaluationAsync` directly | Yes — proc name referenced | **Live handler, dead domain class** | Directly verified | |
| TrainingJob | Fail | Domain method exists | No handler found calling it | Unreached | Pattern-inferred | |
| TrainingJob | Delete | **No** | N/A | N/A | Directly verified | |
| Investigation | Open (Create) | Yes, but bypassed — `RunInvestigationCommandHandler` calls the repository's create method directly | Yes — proc referenced | **Live handler, dead domain class** | Directly verified | No event of any kind confirmed |
| Investigation | AddFinding | Domain method exists (guarded, 2 throw sites) | No handler confirmed calling it | Unconfirmed | Pattern-inferred | |
| Investigation | SubmitHypothesis | Domain method exists (guarded) | No handler confirmed calling it | Unconfirmed | Pattern-inferred | |
| Investigation | Cancel | Yes, but bypassed — `CancelInvestigationCommandHandler` calls `_repository.CancelAsync` directly | Yes — proc referenced | **Live handler, dead domain class** | Directly verified | Same bypass pattern as `TrainingJob.Cancel` |
| Investigation | Complete | Domain method exists (guarded) | No handler confirmed calling it | Unconfirmed | Pattern-inferred | |
| Investigation | Delete | **No** | N/A | N/A | Directly verified | |

**This module's headline pattern is distinct from every other module's**: transitions exist in both C#
(fully guarded) and are reachable via a live handler and a real stored procedure call — but the C# guard
and the SQL call never actually connect to each other, because the handler bypasses the aggregate
entirely. This is neither a "missing" nor an "orphaned" transition in the usual sense; it is a
**guard that is provably dead code at runtime**, which the master `inventory.md` treats as a distinct,
higher-priority category from ordinary Create-only gaps.
