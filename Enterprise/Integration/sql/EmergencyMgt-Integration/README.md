# EmergencyMgt-Integration

PlantSafety-side durable storage for the EmergencyMgt <-> PlantSafety integration surface.
Schema `EmergencyMgtIntegration` in `PlantSafety.CoreDB`. Mirrors `IncidentMgt-Integration`'s
own pattern in this same directory exactly (see that folder's `00-create-schema.sql` for the
full architectural rationale: one-directional push, eventually-consistent, no linked-server
join, PlantSafety reads back out through read-only views).

## What this is

- **Schema**: `EmergencyMgtIntegration` (7 SQL files, `00-` through `40-`, applied in filename
  order).
- **6 tables**: `RoutingKeyRegistry`, `EmergencySummaryInbound`, `EmergencyLifecycleInbound`,
  `EmergencyCorrectiveActionInbound`, `EmergencyReadinessInbound`, `EmergencyGovernanceLog`.
- **5 read-only views**: `vwEmergencySummary`, `vwEmergencyLifecycle`,
  `vwEmergencyCorrectiveActions`, `vwEmergencyReadiness`, `vwEmergencyKPIs`.
- **9 procedures**: 4 upsert/record procs (one per inbound table plus lifecycle), routing-key
  register/heartbeat, governance-log record, an integration health-check, and
  `usp_ValidateEmergencyMgtIntegration` (installer-contract validation, mirrors EmergencyMgt's
  own `Governance.usp_ValidateInstallation` shape).
- **20 seeded routing keys** (`40-seed-routingkeys.sql`), covering EmergencyMgt's base module
  events plus one representative key per each of its 14 Extensions subsystems.

## Verified live (2026-08-28)

Deployed to the real `PlantSafety.CoreDB` on the `PLANTSAFETY` named instance (not a draft —
`IncidentMgt-Integration`, the precedent this mirrors, remains source-only/undeployed as of
this date; this is the first schema of this integration shape actually live in this repo):

- All 7 files applied cleanly, then re-applied with no changes (idempotent — `CREATE OR ALTER`
  / existence-checked `CREATE` throughout).
- `EmergencyMgtIntegration.usp_ValidateEmergencyMgtIntegration`: **23/23 checks passing**
  (1 schema + 6 tables + 5 views + 9 procs + 1 routing-key-seed-count + 2 informational
  data-flow checks).
- A real push round-trip proved live: `usp_EmergencySummaryInbound_Upsert` ->
  `vwEmergencySummary`/`vwEmergencyKPIs` read back correctly, `usp_RoutingKeyRegistry_Heartbeat`
  updated `LastSeenUtc`, `usp_EmergencyGovernanceLog_Record` wrote a `Health` signal,
  `usp_IntegrationHealth_Check` reflected all of it. The synthetic smoke-test row was cleaned up
  afterward.

## PlantSafety.Api endpoints (added 2026-08-29)

`/integration/emergency/*` is now a real, compiling, wired C# vertical slice, following the
existing `AlertsV1Controller`/CQRS pattern exactly (MediatR `ISender` → Application-layer
query → `IEmergencyMgtIntegrationQueryRepository` → Dapper stored-procedure call):

- **Application layer**: `PlantSafety.Application/Integrations/EmergencyMgt/` — 8 DTOs, the
  `IEmergencyMgtIntegrationQueryRepository` interface, 8 queries, 8 handlers, DI registration.
- **Infrastructure layer**: `PlantSafety.Infrastructure/Integrations/EmergencyMgt/` —
  `EmergencyMgtIntegrationQueryDapperRepository` calling 7 stored procedures (new
  `22-usp-reads.sql`: thin wrappers over the 5 views plus `EmergencyGovernanceLog`/
  `RoutingKeyRegistry` reads — required because `DapperQueryExecutor` executes
  `CommandType.StoredProcedure` only, never a bare view), DI registration.
- **Api layer**: `PlantSafety.Api/Integration/EmergencyMgt/Controllers/V1/
  EmergencyMgtIntegrationV1Controller.cs` — 9 GET endpoints (`/summary`, `/lifecycle`,
  `/corrective-actions`, `/readiness`, `/kpis`, `/health` (unauthenticated, matches every other
  health probe), `/version-manifest` and `/drift-check` (read the latest `EmergencyGovernanceLog`
  row for that signal type, 404 if none was ever consumed — honest, matches EmergencyMgt's own
  precedent), `/dependency-map` (returns the registered routing keys — no separate
  "dependency map" object exists in this schema, so the routing-key registry is the closest
  real concept). All reads require `SafetyTierPolicies.Read`.

**Verified live** (2026-08-29): the 7 new stored procedures were deployed to the real
`PlantSafety.CoreDB` and smoke-tested via `sqlcmd` (all returned correct data, including the
already-populated `Health` governance signal and all 20 routing keys). `dotnet build
PlantSafety.sln` succeeds with **0 errors** end to end (see the build-repair note below) — the
new controller/handlers/repository were confirmed to compile and wire correctly through the
full DI chain, not merely written and assumed correct. `dotnet test` could not be verified in
this pass — console output from the xUnit v3 test host was not capturing in this environment (a
tooling/output-capture issue encountered while investigating, not a known test failure); this is
recorded honestly rather than claimed as a passing test run.

## Build-repair note (2026-08-29)

`PlantSafety.Api` did not build at all before this pass, for reasons entirely unrelated to
EmergencyMgt: `Shared.Authorization.Abstractions` and `Shared.AuditLogging.Abstractions` had
**no source code** anywhere in the `Shared` repo (only stale compiled DLLs remained; the
`.csproj` files themselves were missing). Both were reconstructed from their compiled DLLs'
reflection surface (types, members, and — for constants/seed values — actual runtime values
read via reflection, not guessed) and verified to compile standalone before use.
`Shared.AuditLogging.Infrastructure` was similarly reconstructed, additionally cross-checked
against the real, live `dbo.usp_SearchAuditLog` stored procedure on `PlantSafety.AuditDB`.

`Shared.EventBus.Abstractions`/`Shared.EventBus.Infrastructure`/`Shared.DomainEvents.Abstractions`
were **deliberately not reconstructed** — `DomainEvents.Abstractions` alone contains hundreds of
unrelated cross-module event-type records (Accident Management/CAPA/Compliance/Crisis
Management/etc.) with no way to verify field-level correctness from reflection alone. Per
explicit user direction, the 4 files that depended on them
(`PlantSafety.Infrastructure/Messaging/CrossModule/CrossModuleDomainEventMapper[V2].cs`,
`CrossModuleEventRelay[V2].cs`) and the matching test files were excluded from compilation via
`<Compile Remove>`, with the call sites in `InfrastructureServiceRegistration.cs` commented out
and documented in place. The two relay *interfaces* (`ICrossModuleEventRelay[V2]`) are
self-contained and were NOT excluded, so the ~7 real repositories that take them as optional,
nullable constructor parameters still compile unaffected. Restoring real cross-module eventing
is separate, owned work.

## Automatic pusher (added 2026-08-29)

`Enterprise/Integration/Pusher/` — a **real, direct, synchronous pusher**, not an EventBus
publisher (no message bus, event envelope, or routing-key dispatcher exists anywhere in this
program — see "Build-repair note" above for why one wasn't fabricated to route this through):

- `PlantSafety.Integration.Pusher.Abstractions` — `IIntegrationPusher`, `IIntegrationPushResult`,
  `IntegrationPushStatus`.
- `PlantSafety.Integration.Pusher.Local` — `LocalEmergencyMgtPusher`, a console app that opens
  direct connections to both `EmergencyManagementDB` and `PlantSafety.CoreDB`, reads
  `EmergencyEvent`/`EmergencyCorrectiveAction`/`EmergencyReadinessScore`/
  `Governance.VersionManifest` from the former, and calls the `usp_*_Upsert`/`usp_*_Record`
  procedures on the latter. Severity/Status `TINYINT` codes are mapped to labels by duplicating
  `EmergencyManagement.fnEmergencySeverity`/`fnEmergencyStatus`'s own logic locally (no linked
  server exists to call the real functions cross-database — same precedent as the OLS
  forecasting engine, EmergencyMgt `docs/deviations.md` DEV-009).

**Verified live, twice** (2026-08-29): first run caught a genuine bug (`ManifestHash` is `CHAR`
on the EmergencyMgt side, not `VARBINARY` — fixed, not worked around); second run: **Success, 5
succeeded, 0 failed** — 4 real `EmergencyEvent` rows pushed into `EmergencySummaryInbound`/
`EmergencyLifecycleInbound`, plus EmergencyMgt's own latest `VersionManifest` pushed as a
`Health`-adjacent governance signal. Confirmed read back correctly via
`usp_EmergencyKpis_GetAll` (2 emergency types, 3 open, 1 closed) and
`usp_IntegrationHealth_Check`. `EmergencyCorrectiveAction`/`EmergencyReadinessScore` pushed 0
rows — confirmed genuinely correct (those tables hold 0 rows on the EmergencyMgt side today, not
a pusher bug). Lifecycle rows reflect current status only, not true transition history —
`EmergencyEventHistory` is never written by any EmergencyMgt CRUD procedure (EmergencyMgt
`docs/deviations.md` DEV-001), so no transition history exists anywhere to push.

Not yet wired to any scheduler — run manually today (`dotnet run` from
`PlantSafety.Integration.Pusher.Local`, optionally passing both connection strings as args),
mirroring EmergencyMgt's own `tools/*.ps1` "no CI runner; run manually or via Windows Task
Scheduler" precedent (`docs/deviations.md` DEV-020). See `PlantSafetyInstallerContract.md`'s
config-schema section for the shape a real scheduled registration would take.

## Governance expansion (added 2026-08-29)

`50-governance.sql` / `51-usp-validateintegration-extensions.sql` — real, deployed, live-verified:

- `EmergencyMgtIntegration.VersionManifest` (append-only, guard trigger) +
  `usp_VersionManifest_ComputeAndRecord`/`_SetSignature`/`_GetLatest` — snapshots this schema's
  own deployed object state (`sys.objects` FOR JSON), hashes it (`SHA2_256`), records it.
  RSA signing hook exists (`_SetSignature`) but nothing calls it yet — no signing certificate
  exists for PlantSafety in this environment (EmergencyMgt's own DEV-ONLY cert, DEV-017, does not
  extend automatically; a real cert is separate, ops-owned work).
- `EmergencyMgtIntegration.DriftCheck` (append-only, guard trigger, `Record`/`GetUnresolved`/
  `Resolve`) — the same log-table "drift-safe" mechanism EmergencyMgt's own `Governance.
  DriftCheck` and PredictiveSafety's `RiskDriftCheck` already establish program-wide (recordable,
  not a live `sys.*`-comparison engine — see EmergencyMgt `docs/deviations.md` DEV-032 for why
  that's the honest program-wide baseline, not a shortfall unique to this schema).
- `EmergencyMgtIntegration.ArchiveRegistry` + `usp_EnforceRetention` (3-year cutoff on
  `ReceivedUtc` across the 4 inbound feed tables, fixed table list, never dynamic SQL) — run live:
  `TotalArchived=0` (correct — no row is yet 3 years old).

`usp_ValidateEmergencyMgtIntegration` grew from 23 to **45 checks**, all passing live
(`Table:VersionManifest`/`DriftCheck`/`ArchiveRegistry`, 23 procedure-presence checks, 2
append-only-guard checks, `DriftDetection` (0 unresolved), `VersionManifestPopulated`).

## Integration test project (added 2026-08-29)

`Enterprise/Integration/Tests/EmergencyMgtIntegration.Tests` — a **real, runnable** xUnit v3
project (not Testcontainers — no such harness exists for `PlantSafety.CoreDB` anywhere in this
repo; real because both named SQL Server instances are already live on this machine). Run live:
**7/7 passing** — installer validation, routing-key seed count, a genuine
upsert-then-read-then-cleanup round trip, version-manifest compute, drift-check
record-then-resolve, health check, and a cross-check that EmergencyMgt's own source tables still
match what the pusher assumes (catches source-side schema drift before it silently breaks the
pusher).

## Installer contract & CI/CD templates (added 2026-08-29)

`PlantSafetyInstallerContract.md` and `ci/{pipeline,test,deploy,audit}-template.yml` — **contracts
and templates, explicitly not executable**, per direct instruction: no installer framework
(bootstrapper, rollback/upgrade engine) and no CI runner exist anywhere in either repo to attach
real automation to (EmergencyMgt's own `.github/workflows/ci.yml` jobs reference a
`emergencymgt`-labeled runner that has never existed either — this is a program-wide gap, not
unique to this integration). These documents scope the work precisely so it's ready the moment
real infrastructure exists, without pretending that infrastructure exists today.

## What is NOT built yet — recorded honestly, not fabricated

- **The pusher runs manually, not on a schedule.** No Windows Task Scheduler registration, no CI
  cron trigger — see "Automatic pusher" above.
- **No RSA signing certificate for PlantSafety.** `usp_VersionManifest_SetSignature` exists;
  nothing calls it with a real signature yet.
- **No CI/CD execution.** The templates above are real files with real step definitions but do
  not run anywhere (no runner registered).
- **No installer framework to attach `PlantSafetyInstallerContract.md`'s checks to.** The checks
  themselves are already real, live SQL (`usp_ValidateEmergencyMgtIntegration`); only the
  bootstrapper/rollback/upgrade engine around them doesn't exist.
- **`EmergencyCorrectiveActionInbound`/`EmergencyReadinessInbound` are genuinely empty** — not a
  pusher bug, EmergencyMgt itself has 0 rows in the corresponding source tables today.
- **Live drift detection remains a log-table mechanism**, not a `sys.*`-comparison engine —
  matches the program-wide baseline both sibling modules already established.

The `DEPLOYMENT-MANIFEST.md` at `source/sql/DEPLOYMENT-MANIFEST.md` records this folder under
Tier 0 (no cross-module FK dependencies), alongside `IncidentMgt-Integration`.
