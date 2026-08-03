# Command-Handler Map — Analytics

See `_shared-findings.md` for F7 (Outbox universal), F8 (none of these 4 markers are on the
23-aggregate SQL-audit list), F9 (event delivery, [EVT-A]), F10. All 4 aggregates are single-method
(Create only) marker aggregates per Phase 1's inventory — this file is short by construction, not by
omission. Command/handler names pattern-inferred; not individually opened this phase.

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Culture (Analytics) | Create only | `CreateCultureCommand` | matching handler | `AddAsync` — **would have no confirmed SQL destination** (no proc found under exact or loose match, inherited) | `CultureCreatedEvent` | Y (row written to Outbox regardless of whether the SQL write itself would succeed) | **N** | Directly verified (SQL absence, inherited) | Candidate zero-SQL orphan, distinct from `SafetyCulture.CultureAssessment` (do not confuse) |
| Engagement | Create only | `CreateEngagementCommand` | matching handler | `AddAsync` — no SQL found | `EngagementCreatedEvent` | Y | **N** | Directly verified (SQL absence, inherited) | Candidate zero-SQL orphan |
| PredictiveAnalytics | Create only | `CreatePredictiveAnalyticsCommand` | matching handler | `AddAsync` — only a read proc (`usp_PredictiveAnalytics_List`) confirmed, no write proc | `PredictiveAnalyticsCreatedEvent` | Y | **N** | Directly verified (SQL, inherited) | Would likely fail/no-op at the SQL layer if ever invoked (same shape as the 10 emerging-createonly orphans, see `emerging-createonly.md`) |
| ScoreCards (Analytics duplicate) | Create only | `CreateScoreCardsCommand` | matching handler | `AddAsync` — SQL ownership ambiguous vs. top-level `Scorecards.Scorecard` (inherited, unresolved) | `ScoreCardsCreatedEvent` | Y | **N** | Directly verified (method presence, inherited); SQL ownership unconfirmed | 3rd confirmed naming collision (after Alert, Dashboard) — if this marker is ever invoked, it may be silently writing into the live `Scorecard` table or nowhere at all |

**Event delivery reality (F9 [EVT-A]):** all 4 events have named consumer stubs
(`CultureCreatedConsumer`, `DashboardCreatedConsumer`'s siblings pattern — specifically
`EngagementCreatedConsumer`, `PredictiveAnalyticsCreatedConsumer`, `ScoreCardsCreatedConsumer`; see
`publishing-map.md` lines 62-66/`consumption-map.md` lines 25-29), all **unregistered**
(`AddConsumer<T>()` never called) and all subject to the same `OutboxProcessor`-publishes-the-wrong-
CLR-type defect. Net: zero live delivery, and for 2 of the 4 (`Culture`, `Engagement`) the underlying
SQL write itself has no confirmed destination even before the event-delivery question arises.
