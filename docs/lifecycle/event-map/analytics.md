# Event Map — Analytics (Culture, Engagement, PredictiveAnalytics, ScoreCards markers)

See `_shared-findings.md` (F14-F17) and `command-handler-map/analytics.md`. All 4 aggregates are
single-method (Create only) marker aggregates. Event bodies directly read this phase
(`CultureCreatedEvent`, `DashboardCreatedEvent`, `EngagementCreatedEvent`,
`PredictiveAnalyticsCreatedEvent`, `ScoreCardsCreatedEvent` — all confirmed identity/actor/
correlation-only payloads, no exceptions).

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| Culture (Analytics) | Create only | Y | `CultureCreatedEvent` | Y — identity/actor/correlation only, correct for a Create-only marker | **N (EVT-A)** — `CultureCreatedConsumer` exists but unregistered, wrong CLR type from Outbox regardless | Directly verified | Candidate zero-SQL orphan (inherited) — event still writes to Outbox even if the underlying SQL write would fail |
| Engagement | Create only | Y | `EngagementCreatedEvent` | Y — same minimal-but-correct shape | **N (EVT-A)** | Directly verified | Candidate zero-SQL orphan |
| PredictiveAnalytics | Create only | Y | `PredictiveAnalyticsCreatedEvent` | Y — same shape | **N (EVT-A)** | Directly verified | Only a read proc confirmed at SQL layer (inherited) |
| ScoreCards (Analytics duplicate) | Create only | Y | `ScoreCardsCreatedEvent` | Y — same shape | **N (EVT-A)** | Directly verified | SQL ownership ambiguous vs. top-level `Scorecards.Scorecard` (inherited, unresolved) — if this marker is ever invoked and the ambiguity resolves toward the live table, this event would misleadingly represent a write path that doesn't cleanly belong to it |
| All 4 above | Update/Delete/Cancel/status | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | Structurally absent — these are Create-only by design, not partial coverage |

**Duplicate-publish check (F17):** 1 event per aggregate, 1 `AddDomainEvent` call per Create factory
— no possibility of a duplicate in a single-method aggregate.

**Module-level summary:** payloads are uniformly minimal but complete for what a Create-only marker
needs (F14) — there is nothing more a "Created" event on a single-method aggregate could plausibly
carry, so F15's "no old value" caveat doesn't meaningfully apply here (there is no prior state to
have missed). All 4 have named, unregistered consumer stubs subject to the standard
unregistered-plus-wrong-CLR-type double defect (F9) — zero live delivery, compounded for `Culture`/
`Engagement` by the underlying SQL write itself having no confirmed destination.
