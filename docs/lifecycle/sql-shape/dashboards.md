# SQL Shape Audit — Dashboards

Phase 6 of 10. Covers `Dashboards.Dashboard`/`Widget` (rich, Reads/Writes SQL naming). `Analytics.
Dashboard` (create-only marker, SQL ownership ambiguous per `module-inventory/dashboards.md`) is not
traced here — see `analytics.md`.

## Worst issues in this module

1. **Second confirmed whole-module zero-validator exposure, after Alerts** — `find` across
   `PlantSafety.Application\Dashboards\` this phase returned **zero `*Validator.cs` files**, for either
   `Dashboard` or `Widget` commands. Every `AddString` call in both repositories is therefore exposed to
   the classic silent-truncation bug with no FluentValidation length check anywhere upstream — this is
   the same category of gap as `TriggerAlertCommand`, but affecting an entire module's command set (6
   Dashboard + 4 Widget transitions), not one command. Directly verified this phase (`find` sweep of
   `PlantSafety.Application/Dashboards`, zero results).
2. **`Dashboard.Name` (200) / `Widget.Name` (200) / `Widget.DataSourceQueryOrEndpoint` (200) /
   `Dashboard.Description` (2000)** — all fed directly from command input with no upstream length check
   of any kind, confirmed by direct read of `DashboardDapperRepository.cs`/`WidgetDapperRepository.cs`.
   A caller-supplied name or endpoint URL longer than 200 chars is silently cut, not rejected.
3. **`FilterKey`/`FilterValue` (Dashboard, 100/500) and `ParamKey`/`ParamValue` (Widget, 100/500)** are
   arbitrary caller-supplied key/value pairs (dashboard filter overrides, widget data-source parameters)
   with no length validation and no schema-level CHECK — the same unvalidated-truncation exposure,
   applying per-entry rather than per-aggregate (a dashboard/widget with many filter/param entries
   multiplies the number of silently-truncatable values per single write).
4. **No enum drift found, and this is the module with the 2nd/3rd positive CHECK-constraint controls in
   the solution** — `CK_Dashboard_Status CHECK (Status IN ('Draft','Published','Archived'))` matches
   `DashboardStatus` enum exactly (`Draft=0, Published=1, Archived=2`); `CK_Widget_Status CHECK (Status
   IN ('Draft','Active','Deprecated','Retired'))` matches `WidgetStatus` exactly (`Draft=0, Active=1,
   Deprecated=2, Retired=3`). Directly verified by reading both enum files and both table DDLs this
   phase — genuinely no drift, a rare positive finding.
5. **`Dashboard.Archive` — silent-archive status not yet confirmed** (inherited flag from
   `module-inventory/dashboards.md`): guarded, but whether it calls `AddDomainEvent` was not individually
   re-checked this phase; the 4 already-confirmed silent-archive instances (Accident/Risk/Scorecard/
   SafetyPolicy-suspected) make this the most likely outcome, not yet directly verified.

## Method

Direct read this phase: `DashboardDapperRepository.cs`, `WidgetDapperRepository.cs` (both in full,
including every `AddString` call site and size), `Dashboard.sql`, `Widget.sql` (table DDL, including both
CHECK constraints), `DashboardStatus.cs`, `WidgetStatus.cs` (both enums in full). Validator absence
confirmed by `find` across the whole `PlantSafety.Application\Dashboards\` tree.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Dashboard.Create | Yes — `Name`, `Description`, `CategoryCode`, `Role` all present in `usp_Dashboard_Writes` param list | Not individually re-verified column-by-column this phase; no mismatch found in the fields read | No — `CategoryCode`/`Role` have no CHECK (structurally possible, not confirmed drifted) | **Yes — confirmed** (`Name` 200, `Description` 2000, zero validator) | Directly verified | Worst-issue #1/#2 |
| Dashboard.AddWidget | Yes (`FilterKey`/`FilterValue` per-entry) | Not verified | N/A | **Yes — confirmed** (100/500, zero validator, per-entry) | Directly verified | Worst-issue #3 |
| Dashboard.RemoveWidget | Field set not re-opened (expected: WidgetId only, no free-text) | N/A | N/A | No — no string field expected | Pattern-inferred | |
| Dashboard.SetDefaultFilters | Yes (`FilterKey`/`FilterValue`, same child-table shape as AddWidget) | Not verified | N/A | **Yes — confirmed**, same as AddWidget | Directly verified | |
| Dashboard.Publish | Yes (`Status` only) | Yes — `CK_Dashboard_Status` enforces valid values | **No drift** — positive control | N/A (no string field) | Directly verified | Worst-issue #4 |
| Dashboard.Archive | Yes (`Status` only) | Same as Publish | No drift | N/A | Directly verified (guard); **unconfirmed** (event-publish) | Worst-issue #5 |
| Widget.Create | Yes — `Name`, `TypeCode`, `DataSourceModule`, `DataSourceQueryOrEndpoint` all present | Not individually re-verified | No — `TypeCode` has no CHECK | **Yes — confirmed** (`Name` 200, `DataSourceQueryOrEndpoint` 200, `DataSourceModule` 100, zero validator) | Directly verified | |
| Widget.Configure | Yes (`ParamKey`/`ParamValue` per-entry, `RoleName` per-entry 50) | Not verified | N/A | **Yes — confirmed** (100/500/50, zero validator) | Directly verified | |
| Widget.Activate | Yes (`Status` only) | Yes — `CK_Widget_Status` enforces valid values | **No drift** — positive control | N/A | Directly verified | |
| Widget.Deprecate | Yes (`Status`, `DeprecationReason` — literal `"Deprecated"` hardcoded at one call site, `AddString("DeprecationReason", "Deprecated", 500)`) | Yes | No drift | No — value is a hardcoded literal, not user input, at this call site | Directly verified | Notable: `DeprecationReason` is not actually caller-supplied here — a literal string, so no real truncation exposure despite the missing validator |
| Cancel / Delete | N/A — transition does not exist for either aggregate | N/A | N/A | N/A | Directly verified | |
