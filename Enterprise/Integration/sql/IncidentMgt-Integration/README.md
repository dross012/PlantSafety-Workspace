# IncidentMgt-Integration

PlantSafety-side durable storage for the IncidentMgt <-> PlantSafety integration surface.
Schema `IncidentMgtIntegration` in `PlantSafety.CoreDB`. See `00-create-schema.sql` for the
full architectural rationale: one-directional push, eventually-consistent, no linked-server
join, PlantSafety reads back out through read-only views. Mirrored by
`EmergencyMgt-Integration` in this same directory.

## What this is

- **Schema**: `IncidentMgtIntegration` (11 SQL files, `00-` through `51-`, applied in filename
  order).
- **7 tables**: `RoutingKeyRegistry`, `IncidentSummaryFeed`, `GovernanceConsumptionLog`,
  `OutboundEventRegistry` (`00`/`01`/`02`), plus `VersionManifest`, `DriftCheck`,
  `ArchiveRegistry` (`50`, governance).
- **5 read-only views**: `vwIncidentDashboardReadOnly`, `vwIncidentTrendWidget`,
  `vwSeverityDistribution`, `vwCorrectiveActionStatusReadOnly`, `vwInvestigationStatusReadOnly`.
- **24 procedures**: upsert/register/heartbeat procs for the 4 base tables, an integration
  health-check, 8 thin reads-over-views procs (`22-*.sql` — required because
  `PlantSafety.Infrastructure`'s `DapperQueryExecutor` executes
  `CommandType.StoredProcedure` only, never a bare view), a VersionManifest/DriftCheck/
  retention governance trio (`50-*.sql`), and `usp_ValidateIncidentMgtIntegration`
  (installer-contract validation, mirroring `EmergencyMgt-Integration`'s own
  `usp_ValidateEmergencyMgtIntegration` shape).
- **8 seeded routing keys** (`40-seed-routingkeys.sql`) IncidentMgt publishes, and **5 seeded
  outbound feeds** (`02-outbound-tables.sql`) PlantSafety publishes back to IncidentMgt.

## Verified live (2026-09-06)

Deployed to the real `PlantSafety.CoreDB` on the `PLANTSAFETY` named instance — this schema
had been source-only since 2026-08-28 (see `sql/DEPLOYMENT-MANIFEST.md`); this is the date it
was actually deployed:

- All 11 files applied cleanly (the original 7 — `00`/`01`/`02`/`10`/`20`/`30`/`40` —
  unchanged; `21`/`22`/`50`/`51` added at deploy time to bring this schema to the same
  validate/reads/governance parity `EmergencyMgt-Integration` already has).
- `IncidentMgtIntegration.usp_ValidateIncidentMgtIntegration`: **44/44 checks passing**
  (1 schema + 7 tables + 5 views + 18 procs + 2 append-only guard triggers + 1 routing-key-seed
  + 1 outbound-feed-seed + 1 drift-detection + 1 version-manifest-populated + 2 informational
  data-flow checks).
- An initial `VersionManifest` snapshot was computed and recorded
  (`usp_VersionManifest_ComputeAndRecord @SchemaVersionLabel = '1.0.0-initial-deploy'`).

## Known limitations (honestly reported, not defects)

- **No automatic IncidentMgt-side puller/pusher is wired yet.** `IncidentSummaryFeed` and
  `GovernanceConsumptionLog` are correctly empty (0 rows) until IncidentMgt's own Servicer (or
  a manual/scheduled pusher tool, matching `EmergencyMgt-Integration`'s
  `PlantSafety.Integration.Pusher.Local` precedent) actually calls
  `usp_IncidentSummaryFeed_Upsert`/`usp_GovernanceConsumption_Record`. The validation proc
  reports this as informational, not a failure — schema correctness and live data flow are two
  different, honestly distinguishable facts.
- **No PlantSafety.Api vertical slice exists yet for this integration** (unlike
  `EmergencyMgt-Integration`, which has a full `/integration/emergency/*` controller +
  Application/Infrastructure layer — see that folder's README). Building the equivalent
  `/plantsafety/incidents/*` C# slice (DTOs, queries/handlers, Dapper repository, controller)
  is a separate, larger piece of work, out of scope for this SQL-deployment pass.
