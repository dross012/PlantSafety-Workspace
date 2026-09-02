# PlantSafety Installer Contract — EmergencyMgt-Integration

**Status: CONTRACT, not an implementation.** No installer framework (bootstrapper, config
schema binder, rollback engine, upgrade engine, health-report generator) exists anywhere in the
PlantSafety repo for `PlantSafety.CoreDB` — confirmed by search before writing this document.
`EmergencyMgt`'s own `Deploy-EmergencyMgt.ps1`/`tools/New-EmergencyMgtHealthReport.ps1` are the
only such tooling found in either repo, and they only ever target the `EMERGENCYMGT` instance.
This document defines what a future PlantSafety installer would need to check/do for the
`EmergencyMgtIntegration` surface, so that work is scoped and ready the moment such a framework
exists — it does not simulate one.

## Required checks

All of these are **already implemented today** as real, live, deployed SQL —
`EmergencyMgtIntegration.usp_ValidateEmergencyMgtIntegration` (45/45 passing as of 2026-08-29).
A future installer's job is only to *call* this procedure and act on its result, not to
reimplement the checks:

| Category | Check | Source |
|---|---|---|
| Schema presence | `EmergencyMgtIntegration` schema exists | `usp_ValidateEmergencyMgtIntegration` |
| Table presence | 9 tables (6 inbound/registry + VersionManifest/DriftCheck/ArchiveRegistry) | same |
| View presence | 5 read-only views | same |
| Procedure presence | 23 procedures (upsert/record, read, governance) | same |
| Append-only guards | `TR_..._VersionManifest_NoDelete`, `TR_..._DriftCheck_NoDelete` | same |
| Routing key seed | Exactly 20 active routing keys | same |
| Drift detection | Zero unresolved `DriftCheck` rows | same |
| Version manifest | At least one recorded manifest row | same |
| Data flow (informational) | Row counts per inbound feed — never a hard fail | same |

## Required config schema (not yet formalized)

A real installer would need a config section along these lines (illustrative, not binding):

```json
{
  "EmergencyMgtIntegration": {
    "PusherConnectionStrings": {
      "EmergencyMgt": "Server=.\\EMERGENCYMGT;Database=EmergencyManagementDB;Trusted_Connection=True;",
      "PlantSafety": "Server=.\\PLANTSAFETY;Database=PlantSafety.CoreDB;Trusted_Connection=True;"
    },
    "PusherScheduleCron": "*/15 * * * *",
    "RetentionYears": 3
  }
}
```

`PlantSafety.Integration.Pusher.Local` (see `Enterprise/Integration/Pusher/`) already accepts
both connection strings as command-line arguments today, matching this shape; a real installer
would bind this section and pass it through rather than the current hardcoded defaults.

## Required automatic-pusher service registration

Not yet wired into any Windows Service or scheduled task — `PlantSafety.Integration.Pusher.Local`
is a manually-run console app today (mirrors EmergencyMgt's own `tools/*.ps1`
run-manually-or-via-Task-Scheduler precedent, `EmergencyMgt` repo `docs/deviations.md` DEV-020).
A future installer would register a Windows Task Scheduler entry (or, if a real CI/CD runner
exists by then, a scheduled pipeline job) invoking it on `PusherScheduleCron`.

## Required health checks

`GET /integration/emergency/health` (already built, already live) is the health-check surface a
future installer would call. No separate installer-level health check needs building — it would
reuse this endpoint exactly, matching how `usp_IntegrationHealth_Check` is already the single
source of truth for pusher-freshness/routing-key-staleness.

## Rollback / upgrade logic

**Not defined.** No installer exists to roll anything back or upgrade in place; the SQL layer's
own idempotent `CREATE OR ALTER`/existence-checked `CREATE` pattern (every file in this folder)
is additive-only and safe to re-run, which is the closest real substitute today. A real
rollback/upgrade engine is out of scope until an installer framework exists to hang it on.

## Diagnostics

`usp_ValidateEmergencyMgtIntegration`'s own `Detail` column on each failing row is the existing
diagnostic surface. No separate diagnostics bundle exists.
