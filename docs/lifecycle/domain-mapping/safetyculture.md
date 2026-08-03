# Domain Mapping — SafetyCulture

See `_shared-findings.md` §F1 (tenancy).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| CultureAssessment | Create | Starts open/unscored | Y (trivial) | — | None | Not enforced (F1) | Directly verified (inherited) | |
| CultureAssessment | RecordScore | Real range-check (0-100) on both score fields — Phase 1 confirmed positive control | **Y** — confirmed | Range validation confirmed | None | Not enforced (F1) | Directly verified (inherited) | Positive control |
| CultureAssessment | PublishReport | Guard presence confirmed; exact rule pattern-inferred | Y (presence); exact semantics pattern-inferred | — | None | Not enforced (F1) | Directly verified (guard presence, inherited) | |
| CultureAssessment | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| StopWorkEvent | Create/Investigate/Resolve | 3 guard + 4 event sites | Pattern-inferred Y | Investigation notes required (pattern-inferred) | A Stop-Work event plausibly should cross-check against `AccidentManagement.Accident`/`RiskManagement.Risk` if the underlying hazard is already tracked elsewhere — no confirmed link | Not enforced (F1) | Directly verified (counts, inherited) | Candidate cross-module gap, not confirmed this phase |
| StopWorkEvent | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

**Summary:** `CultureAssessment.RecordScore`'s 0-100 range check remains the clearest positive input-
validation control in this module; StopWorkEvent's lack of any confirmed cross-reference to
Accident/Risk tracking is a plausible (unconfirmed) cross-module gap.
