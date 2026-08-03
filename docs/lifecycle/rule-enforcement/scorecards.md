# Rule Enforcement Validation — Scorecards

See `_shared-findings.md`. 1 aggregate (`Scorecard`), 7 transitions.

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Create (Draft) | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `ScorecardDapperRepository` confirmed hardcoded site | High | Audit-logged (F8) |
| RecordMetric | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| CompleteCompilation | **Pass** — real business rule: rejects compiling a scorecard with zero recorded measurements (inherited) | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| Publish | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| RecordDistribution | Pass (guard is real; only the exception *message* has a copy-paste bug comparing `Status.ToString()` to itself, cosmetic, not a guard bypass — inherited) | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| Close | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| Archive | Pass — status-guarded, confirmed no `AddDomainEvent` (silent archive, inherited) | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | N/A — silent, nothing enqueued | High | 4th confirmed silent-archive instance |
| Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
