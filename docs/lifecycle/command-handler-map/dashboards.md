# Command-Handler Map — Dashboards

See `_shared-findings.md` for F7 (Outbox universal), F8 (neither `Dashboards.Dashboard`/`Widget` nor
the `Analytics.Dashboard` marker is on the 23-aggregate SQL-audit list), F9 (event delivery, [EVT-A]
for `DashboardCreatedEvent`), F10, and `domain-mapping/_shared-findings.md` F3 (`Dashboard.Archive`
confirmed to raise `DashboardArchivedEvent` — one of only 3 non-silent Archives in the solution).
Command/handler names pattern-inferred; not individually opened this phase.

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Dashboard | Create | `CreateDashboardCommand` | matching handler | `AddAsync` | `DashboardCreatedEvent` | Y | **N** | Pattern-inferred | Reads/Writes SQL naming (`usp_Dashboard_Reads`/`_Writes`), confirmed not a naive-grep false negative (inherited) |
| Dashboard | AddWidget | `AddDashboardWidgetCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` (via `_Writes` proc) | pattern-inferred name | Y | **N** | Pattern-inferred | |
| Dashboard | Archive | `ArchiveDashboardCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | `DashboardArchivedEvent` — **confirmed raised** (F3, direct read) | **Y** | **N** | Directly verified (event side, inherited F3) | One of only 3 confirmed non-silent Archives (alongside `PermitTemplate`, `Operational.Alerts.Alert`) — but still gets zero SQL audit row (not F8-covered) and zero event consumer (below), so "non-silent" here means only "outbox row exists," not "anything downstream happens" |
| Dashboard | Publish | `PublishDashboardCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | |
| Dashboard | RemoveWidget | `RemoveDashboardWidgetCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | |
| Dashboard | SetDefaultFilters | `SetDashboardDefaultFiltersCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | |
| Dashboard / Widget | Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| Widget | Create / Activate / Configure / Deprecate | matching commands | matching handlers | `AddAsync` (Create) / `GetByIdAsync`→`UpdateAsync` (rest) | pattern-inferred names | Y | **N** | Pattern-inferred | |
| Dashboard (Analytics marker) | Create only | Presumed exists | Unconfirmed | SQL ownership ambiguous vs. the rich `Dashboard` above (both named `Dashboard`, inherited finding — not resolved this phase either) | Unconfirmed | Unconfirmed | **N** | Directly verified (method presence, inherited); SQL/command ambiguity unresolved | If this marker and the rich `Dashboard` share the same `_Reads`/`_Writes` procs, one is silently writing into the other's table — a real data-integrity risk, still open |

**Event delivery reality (F9 [EVT-A]):** `DashboardCreatedConsumer` exists
(`publishing-map.md` line 63/`consumption-map.md` line 26) but is never registered
(`AddConsumer<DashboardCreatedConsumer>()` missing) and would receive the wrong CLR type from
`OutboxProcessor` even if registered. All non-Create events (`AddWidget`, `Archive`, `Publish`,
`RemoveWidget`, `SetDefaultFilters`, and all 4 Widget transitions) have zero consumer class of any
kind. Net: zero live delivery for every transition in this module, including the confirmed
event-raising `Archive`.
