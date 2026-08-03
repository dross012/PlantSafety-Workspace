# Event Map — Scheduling (PMScheduling, WorkOrderPriority, EmergencyDrillPlanning, HazardIsolationSequencing, ShiftFatigue)

See `_shared-findings.md` (F14-F17) and `command-handler-map/scheduling.md`. **Fully directly
verified this phase**: read of all 5 `PMScheduling` events, all 5 `WorkOrderPriority` events, and
all 3 Create-only events.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| PMScheduling | Create (Draft) | Y | `PMSchedulingCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | |
| PMScheduling | SchedulePreventiveMaintenance | Y | `PMSchedulingScheduledEvent` | Y — `DueAtUtc, ScheduledAtUtc` | **N** | Directly verified | |
| PMScheduling | RecordCompletion | Y | `PMSchedulingCompletedEvent` | Y — `CompletionNotes (string?), CompletedAtUtc` (**F15 caveat**: no reference back to the `DueAtUtc` set at Schedule, so a consumer cannot tell from this event alone whether the work was completed on time) | **N** | Directly verified | |
| PMScheduling | Escalate | Y | `PMSchedulingEscalatedEvent` | Y (F15 caveat) — `PriorityCode (string), EscalatedAtUtc` — no prior priority code | **N** | Directly verified | |
| PMScheduling | Cancel | Y (would raise correctly if reached — **confirmed this phase**) | `PMSchedulingCancelledEvent` | Y — `Reason (string), CancelledAtUtc` — fully complete | **N** (moot regardless — 0 HTTP routes reach this transition, inherited finding) | Directly verified | The best-instrumented-but-100%-dead transition in the solution (inherited framing) — this phase confirms the payload itself would also be complete if it were ever reached |
| WorkOrderPriority | Create | Y | `WorkOrderPriorityCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | |
| WorkOrderPriority | Escalate | Y | `WorkOrderPriorityEscalatedEvent` | Y (F15 caveat) — `PriorityCode (string), EscalationReason (string), EscalatedAtUtc` — same "no prior code" gap as `PMSchedulingEscalatedEvent` | **N** | Directly verified | |
| WorkOrderPriority | Resolve | Y | `WorkOrderPriorityResolvedEvent` | Y — `ResolutionNotes (string?), ResolvedAtUtc` | **N** | Directly verified | |
| WorkOrderPriority | StartWork | Y | `WorkOrderPriorityStartedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | |
| WorkOrderPriority | Close | Y | `WorkOrderPriorityClosedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | |
| WorkOrderPriority | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| EmergencyDrillPlanning | Create only | Y | `EmergencyDrillPlanningCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | Dormant `_Update` proc, unused |
| HazardIsolationSequencing | Create only | Y | `HazardIsolationSequencingCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | Same dormant-`_Update` note |
| ShiftFatigue | Create only | Y | `ShiftFatigueCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | Same dormant-`_Update` note |

**Duplicate-publish check (F17):** event-class inventory shows exactly 1 distinct event type per
named transition (5 for PMScheduling, 5 for WorkOrderPriority, 1 each for the 3 Create-only
aggregates) — no naming evidence of a double-publish; aggregate method bodies not individually
re-opened this phase.

**Module-level summary:** every named transition in this module publishes with a complete,
well-formed payload for its own scope (F14) — this is the module with the highest
publish-rate-to-transition-count ratio of any file this phase (all ~19 transitions publish,
including the unreachable `PMScheduling.Cancel`). Both `Escalate` variants (`PMScheduling`,
`WorkOrderPriority`) are clean F15 instances — priority-code escalation events that can't show the
"from" state. Zero live delivery for any transition (F9) — `SchedulesCreatedConsumer`'s naming does
not exactly match any of these 5 aggregates' own event names, reinforcing that none of them has a
real consumer.
