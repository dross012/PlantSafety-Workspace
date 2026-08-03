# Domain Mapping — RiskManagement

See `_shared-findings.md` §F1 (tenancy), §F3 (silent archive).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Risk | Create (Identify hazard) | New record starts unscored | Y (trivial) | Required fields via constructor | None | Not enforced (F1) | Directly verified (inherited) | |
| Risk | ScoreRisk | Must be identified/unscored (pattern-inferred) | Pattern-inferred Y | Score range check plausible, not independently reconfirmed | None | Not enforced (F1) | Directly verified (inherited) | |
| Risk | Prioritize | Depends on ScoreRisk having run | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| Risk | ApplyControlMeasure | Real cross-field rule: justification required for lower-effectiveness control hierarchy (Phase 1 finding) | **Y** — confirmed | Justification text required when downgrading control hierarchy | None | Not enforced (F1) | Directly verified (inherited) | Positive control |
| Risk | ValidateEffectiveness | Control measure must exist first (pattern-inferred) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| Risk | StartReviewCycle / CompleteReview | Paired guard (must Start before Complete) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| Risk | Close | Should require review completion (pattern-inferred) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| Risk | Archive | Must be `Closed` | Y (guard), **N** for event — silent archive, same pattern as Accident (Phase 1 finding) | None | None | Not enforced (F1) | Directly verified (inherited) | 2nd confirmed silent-archive instance |
| Risk | Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| Operational.RiskManagement (dup marker) | Create | Factory-only, no lifecycle | N/A | None found | None | Not enforced (F1) | Directly verified (inherited controller read; SQL cross-ref new Phase 1) | **Confirmed dead**: GET-only controller AND no write-proc SQL path at all — a `Create` call, if ever wired, has nowhere to persist |
| Operational.RiskManagement | Update/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

**Summary:** No newly-disconfirmed invariants this phase beyond the inherited silent-archive gap;
Operational.RiskManagement remains the clearest confirmed-dead duplicate in the solution.
