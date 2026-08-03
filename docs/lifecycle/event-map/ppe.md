# Event Map — PPE (PPEAssignment, PPEInventory, PPECategory, PPETrainingFit, PPEZoneMapping)

See `_shared-findings.md` (F14-F17) and `command-handler-map/ppe.md`. **Fully directly verified this
phase**: read of all 5 `PPEAssignment` events, all 5 `PPEInventory` events, and all 3 Create-only
events.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| PPEAssignment | Create | Y | `PPEAssignmentCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | |
| PPEAssignment | Assign (Issue) | Y | `PPEAssignmentIssuedEvent` | Y — identity/timestamp/actor only (**F15-adjacent gap**: no field naming which employee/worker the PPE was assigned to, or which PPE item/category — a consumer would need to already know the aggregate's other stored state to make use of this event at all) | **N** | Directly verified | The thinnest "verb" event payload found this phase relative to how much the transition name implies should be captured |
| PPEAssignment | RecordFitTest | Y | `PPEAssignmentFitTestRecordedEvent` | Y — **best payload in this module**: `Passed (bool), Notes (string?), TestedAtUtc, FitTestExpiresAtUtc (UtcTimestamp?)` — forwards the next expiry date, genuinely useful for a compliance-tracking consumer | **N** | Directly verified | Possible overlap with `EmployeeSafety.Ppe.RecordFitTest` (inherited, unconfirmed duplicate) |
| PPEAssignment | FlagForReplacement | Y | `PPEAssignmentFlaggedForReplacementEvent` | Y — `Reason (string), FlaggedAtUtc` | **N** | Directly verified | |
| PPEAssignment | Retire | Y | `PPEAssignmentRetiredEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | |
| PPEAssignment | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| PPEInventory | Create | Y | `PPEInventoryCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | |
| PPEInventory | IssueUnits | Y | `PPEInventoryUnitsIssuedEvent` | Y — **best payload in this module**: `QuantityIssued (int), RemainingAvailable (int), LowStockTriggered (bool), IssuedAtUtc` — `LowStockTriggered` is an explicit computed flag, exactly the kind of field that lets a downstream consumer react without re-deriving business logic | **N** | Directly verified | |
| PPEInventory | ReceiveStock | Y | `PPEInventoryStockReceivedEvent` | Y — `QuantityReceived (int), NewStockQuantity (int), ReceivedAtUtc` — carries the new running total, a partial F15 mitigation (a consumer diffing successive events could reconstruct quantity received without needing an explicit "old quantity" field) | **N** | Directly verified | |
| PPEInventory | FlagForReplacement | Y | `PPEInventoryFlaggedForReplacementEvent` | Y — `Reason (string), FlaggedAtUtc` | **N** | Directly verified | |
| PPEInventory | Retire | Y | `PPEInventoryRetiredEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | Best-guarded transition of the PPE group (6 guard sites, inherited) — guard density doesn't translate into a richer payload here |
| PPEInventory | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| PPECategory | Create only | Y | `PPECategoryCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | Dormant `_Update` proc, unused |
| PPETrainingFit | Create only | Y | `PPETrainingFitCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | Same dormant-`_Update` note |
| PPEZoneMapping | Create only | Y | `PPEZoneMappingCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | Same dormant-`_Update` note |

**Duplicate-publish check (F17):** event-class inventory shows exactly 1 distinct event type per
named transition (5 for PPEAssignment, 5 for PPEInventory, 1 each for the 3 Create-only aggregates)
— no naming evidence of a double-publish.

**Module-level summary:** `PPEInventory.IssueUnits`'s `LowStockTriggered` field is one of the best
individual payload designs found in this entire phase; `PPEAssignment.Assign`'s payload is
conversely the thinnest relative to what the transition name implies (no assignee/item reference at
all). All 5 aggregates are F8-audited (SQL AuditLog), matching Scheduling/Observability as the
best-covered feature areas by that metric — but zero of the ~19 transitions here reach any event
consumer (F9); the solution's only "PPEManagement"-named consumer stub is bound to an unrelated
Operational-namespace marker.
