# Event Map — RiskManagement

See `_shared-findings.md` (F14-F17) and `command-handler-map/riskmanagement.md`. **Fully directly
verified this phase**: read of `Risk.cs` (8 `AddDomainEvent` sites) + all 8 event `record`
definitions (`RiskCreatedEvent`, `RiskScoredEvent`, `RiskPrioritizedEvent`,
`RiskControlMeasureAppliedEvent`, `RiskEffectivenessValidatedEvent`, `RiskReviewCycleStartedEvent`,
`RiskReviewCompletedEvent`, `RiskClosedEvent`).

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| Risk | Create | Y | `RiskCreatedEvent` | Y — identity/actor/correlation only, correct for Create | **N** — `Risk`'s own `RiskCreatedEvent` has zero consumer; the named `RiskManagementCreatedConsumer` is bound to the **Operational duplicate marker's** event, not this one (confirmed, `command-handler-map/riskmanagement.md`) | Directly verified | |
| Risk | ScoreRisk | Y | `RiskScoredEvent` | Y (**F15 caveat**) — `Score (RiskScore), Level (RiskLevel), ScoredAtUtc` — **no prior score/level carried**, so a consumer cannot tell from this event alone whether risk went up or down | **N** | Directly verified | The clearest F15 instance in this module: a "RiskScored" event that structurally cannot answer "did the risk get worse?" without a separate query |
| Risk | Prioritize | Y | `RiskPrioritizedEvent` | Y (F15 caveat) — `Priority (RiskPriority), PrioritizedAtUtc` — no prior priority | **N** | Directly verified | |
| Risk | ApplyControlMeasure | Y | `RiskControlMeasureAppliedEvent` | Y — `ControlMeasureId, HierarchyLevel (ControlHierarchyLevel), AppliedAtUtc` — complete for its own scope; the domain guard's "justification required for lower-effectiveness control" rule (inherited finding) is enforced but **the justification text itself is not in the event payload**, only the fact that a measure was applied | **N** | Directly verified | |
| Risk | ValidateEffectiveness | Y | `RiskEffectivenessValidatedEvent` | Y — **best payload in this module**: `ResidualScore, ResidualLevel, IsAcceptable (bool), ValidatedAtUtc` — a consumer gets the final risk state and an explicit acceptability verdict without needing to re-derive it | **N** | Directly verified | |
| Risk | StartReviewCycle | Y | `RiskReviewCycleStartedEvent` | Y — `StartedAtUtc` only | **N** | Directly verified | |
| Risk | CompleteReview | Y | `RiskReviewCompletedEvent` | Y (F15 caveat) — `NextReviewDueUtc (DateTime), ReviewedAtUtc` — no summary of what the review concluded beyond scheduling the next one | **N** | Directly verified | |
| Risk | Close | Y | `RiskClosedEvent` | Y — `ClosedBy (UserId), ClosedAtUtc` — actor captured explicitly (not just generic `Actor`) | **N** | Directly verified | |
| Risk | Archive | **N — confirmed no `AddDomainEvent` call** (silent-archive, F3 inherited) | N/A | N/A | N/A (silent transition) | Directly verified (inherited) | Still captured in SQL `AuditLog` per F8 despite raising zero event — the audit/event mechanisms diverge for this one row |
| Risk | Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| Operational.RiskManagement (duplicate marker) | Create | Would publish if reached, but **no write SQL proc exists** under this name (Phase 1 finding) | `RiskManagementCreatedEvent` | Unconfirmed — would likely fail/no-op at the SQL layer before an event body is even worth assessing | **N** — own controller confirmed GET-only, so this row never executes in production | Directly verified (inherited) | |

**Duplicate-publish check (F17):** `Risk.cs` — 8 `AddDomainEvent` sites for 8 non-Archive methods,
1:1 (directly re-verified this phase), no duplicates.

**Module-level summary:** 7 of 8 reachable, non-Archive transitions publish (Archive is the sole
confirmed silent one); every published event's own-scope fields are complete (F14) but every one is
also subject to F15 — `ScoreRisk`/`Prioritize` are this module's sharpest examples of a consumer
being structurally unable to compute a before/after delta. Zero live delivery for any transition
(F9); the one named consumer stub in the ingestion discovery's list is bound to a different,
unrelated duplicate-marker aggregate's event, reinforcing that the live `Risk` aggregate has no
consumer of any kind, not even a misdirected one.
