# Event Map — Dashboards (Dashboard, Widget)

See `_shared-findings.md` (F14-F17) and `command-handler-map/dashboards.md` (esp. inherited F3 —
`Dashboard.Archive` confirmed non-silent). **Fully directly verified this phase**: read of all 5
`Dashboard` events and all 4 `Widget` events.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| Dashboard | Create | Y | `DashboardCreatedEvent` | Y — adds `Category (DashboardCategory)` beyond identity/actor | **N (EVT-A)** — `DashboardCreatedConsumer` exists but unregistered, and would get the wrong CLR type from OutboxProcessor anyway | Directly verified | |
| Dashboard | AddWidget | Y | `DashboardWidgetAddedEvent` | Y — `WidgetId (Guid — untyped, unlike the strongly-typed `WidgetId` used within the Widget aggregate's own events), AddedAtUtc` | **N** | Directly verified | Same untyped-Guid-cross-reference pattern seen in `AuditEngagementFindingRecordedEvent` and `AutomatedIncidentDetectionEscalatedToIncidentEvent` — a recurring, if minor, typing inconsistency wherever one aggregate's event references another aggregate/child entity's ID |
| Dashboard | Archive | **Y — confirmed raised** (F3, inherited, re-confirmed this phase) | `DashboardArchivedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | One of the (now likely 4, alongside `PermitTemplate`/`Operational.Alerts.Alert`/possibly `PermitWorkflow`) confirmed non-silent Archives — "non-silent" here means only that an Outbox row exists, not that anything downstream happens |
| Dashboard | Publish | Y | `DashboardPublishedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | |
| Dashboard | RemoveWidget | Y | `DashboardWidgetRemovedEvent` | Y — `WidgetId (Guid, untyped), RemovedAtUtc` | **N** | Directly verified | Same untyped-Guid pattern as AddWidget |
| Dashboard | SetDefaultFilters | Pattern-inferred Y | Unconfirmed name (not found as a distinct `*.cs` file in this phase's directory listing of `Dashboards/Dashboard/Events/` — only 5 files exist: Created, Archived, Published, WidgetAdded, WidgetRemoved) | **Possible finding: this transition may have no dedicated event class at all**, unlike command-handler-map's pattern-inferred assumption that one exists | **N** | Pattern-inferred, contradicted by this phase's direct directory listing | If `SetDefaultFilters` truly has no event, it would be this module's first confirmed "guarded transition, zero event" case beyond the known silent-Archive pattern — flagged for a future phase's direct read of `Dashboard.SetDefaultFilters`'s method body to confirm whether it calls `AddDomainEvent` at all (and if so, using which existing event type) |
| Dashboard / Widget | Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| Widget | Create | Y | `WidgetCreatedEvent` | Y — adds `Type (WidgetType)` | **N** | Directly verified | |
| Widget | Activate | Y | `WidgetActivatedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | |
| Widget | Configure | Y | `WidgetConfiguredEvent` | Y (**F15 caveat**) — identity/timestamp/actor only, **no configuration payload itself** — a "Configured" event that doesn't say what was configured is one of the sharper F15-adjacent gaps found this phase (arguably worse than a simple "no old value" gap, since there's no new value either) | **N** | Directly verified | |
| Widget | Deprecate | Y | `WidgetDeprecatedEvent` | Y — `Reason (string), DeprecatedAtUtc` | **N** | Directly verified | |
| Dashboard (Analytics marker) | Create only | Unconfirmed | Presumed exists; SQL ownership ambiguous vs. this rich `Dashboard` (inherited, unresolved) | Unconfirmed | **N** | Directly verified (inherited) | |

**Duplicate-publish check (F17):** event-class inventory shows exactly 5 distinct event types for
Dashboard's 6 named transitions (`SetDefaultFilters` has no confirmed dedicated file, see above) and
4 for Widget's 4 — no evidence of a double-publish, but the Dashboard count mismatch (5 events for 6
transitions) is itself worth flagging as new information this phase, contradicting the
pattern-inferred assumption in `command-handler-map/dashboards.md`.

**Module-level summary:** `WidgetConfiguredEvent`'s empty payload (an event that carries neither the
old nor the new configuration) is this file's sharpest finding — worse than the typical F15 case
because there's no state captured at all beyond "a configure happened." The untyped `Guid` for
cross-aggregate Widget references recurs here as it did in Auditing/Observability. Zero live
delivery for any of the ~10-11 transitions in this file (F9), including the confirmed
event-raising `Archive`.
