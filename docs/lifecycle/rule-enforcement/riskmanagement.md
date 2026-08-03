# Rule Enforcement Validation — RiskManagement

See `_shared-findings.md`. `Risk` (live) vs `Operational.RiskManagement` (dead duplicate).

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Risk — Create (Identify hazard) | Pass | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2) — `RiskDapperRepository` is a confirmed hardcoded-Enqueue site | High | Audit-logged (F8) |
| Risk — ScoreRisk | Pass | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2) | High | |
| Risk — Prioritize | Pass | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2) | High | |
| Risk — ApplyControlMeasure | **Pass** — real cross-field rule: justification required for lower-effectiveness control-hierarchy choice (inherited) | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2) | High | Positive control |
| Risk — ValidateEffectiveness | Pass | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2) | High | |
| Risk — StartReviewCycle / CompleteReview | Pass | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2) | High | |
| Risk — Close | Pass | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2) | High | |
| Risk — Archive | Pass — status-guarded | Pass (inherited) | Pass | Pass | Fail (F1) | N/A — silent archive, no `AddDomainEvent` call, so no Enqueue call to assess | High | Same silent-archive shape as Accident |
| Risk — Cancel / Delete | N/A — does not exist | N/A | N/A | N/A | N/A | N/A | High | |
| Operational.RiskManagement — Create | **Fail** — marker aggregate, no real guard body | Fail — no write-side SQL exists to reach, question moot | N/A | N/A — no reachable transition | Fail (F1, pattern) | N/A — dead code, own controller confirmed GET-only, no write proc exists (`usp_RiskManagement_List` only) | High | Dead duplicate — every criterion is moot because nothing can persist a mutation here at all |
