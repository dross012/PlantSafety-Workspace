# Event Map — Scorecards

See `_shared-findings.md` (F14-F17) and `command-handler-map/scorecards.md`. **Fully directly
verified this phase**: read of `ScorecardCreatedEvent.cs` + `ScorecardCompilationEvents.cs`
(`ScorecardMetricRecordedEvent`, `ScorecardCompiledEvent`, `ScorecardPublishedEvent`,
`ScorecardDistributedEvent`, `ScorecardClosedEvent`) + `Scorecard.cs`'s 7 `AddDomainEvent` sites.

| Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Create (Draft) | Y | `ScorecardCreatedEvent` | Y — identity/actor/correlation only | **N** — live aggregate's own event has no consumer; `ScoreCardsCreatedConsumer` is bound to the **Analytics duplicate marker's** event | Directly verified | |
| RecordMetric | Y | `ScorecardMetricRecordedEvent` | Y — `KpiMeasurementId, Metric (KpiMetricDefinition), ComputedValue (decimal), RecordedAtUtc` — complete, carries the actual computed value | **N** | Directly verified | |
| CompleteCompilation | Y | `ScorecardCompiledEvent` | Y (**F15 caveat**) — `MetricCount (int), CompiledAtUtc` — a count, not the underlying values; a consumer wanting the actual compiled figures would need to separately fetch them, and there's no reference back to which specific `KpiMeasurementId`s fed the count | **N** | Directly verified | Real domain rule (rejects compiling with zero measurements, inherited) — enforced but not asserted in the payload |
| Publish | Y | `ScorecardPublishedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | |
| RecordDistribution | Y | `ScorecardDistributedEvent` | Y — `Channel (string), DistributedAtUtc` | **N** | Directly verified | Exception-message copy-paste bug (inherited) is cosmetic, doesn't affect this event's payload |
| Close | Y | `ScorecardClosedEvent` | Y — identity/timestamp/actor only (**F15 caveat**: no final/summary metric snapshot carried forward, despite this being the terminal state of a metrics-tracking aggregate) | **N** | Directly verified | The one place in this module where F15 has the most practical bite — a "Scorecard Closed" event that says nothing about the final scores |
| Archive | **N — confirmed no `AddDomainEvent` call** (silent-archive, F3 inherited, re-confirmed this phase: `Scorecard.cs`'s 7 `AddDomainEvent` sites map 1:1 to its 7 non-Archive methods, none for Archive) | N/A | N/A | N/A | Directly verified | Same audit/event divergence as `Risk.Archive` — SQL `AuditLog` row still fires (F8) despite zero event |
| Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |

**Duplicate-publish check (F17):** `Scorecard.cs` — 7 `AddDomainEvent` sites for 7 non-Archive
methods, 1:1, no duplicates (directly re-verified this phase).

**Module-level summary:** 6 of 7 non-Archive transitions publish with complete own-scope payloads;
`CompleteCompilation` and `Close` are this module's clearest F15 instances — both are exactly the
kind of "summarizing" transition where a consumer would want the underlying numbers, and neither
carries them. Zero live delivery (F9) — the one named consumer stub in the ingestion discovery's
list is bound to the unrelated `Analytics.ScoreCards` duplicate marker, not this live aggregate.
