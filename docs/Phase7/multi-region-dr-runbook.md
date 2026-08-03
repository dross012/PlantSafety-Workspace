# Phase 7 §4.4 — Multi-Region Disaster Recovery Runbook

**Status:** Concrete runbook for THIS platform's real components. No real multi-region
infrastructure exists in this sandbox to execute this runbook against — it is written for a
human ops team to follow once real region-scoped SQL Server / RabbitMQ / traffic-manager
infrastructure exists (see `docs\Phase7\multi-region-architecture.md` for what that
infrastructure decision entails). Single-instance backup/restore is already covered by the
existing, real Phase 6/12/18 work (`DR-manual-restore-runbook.md`,
`Invoke-PlantSafetyBackup.ps1`) — this runbook covers the region-cutover angle that prior work
does not.

## When to invoke this runbook

Invoke when a region hosting one or more tenants' primary data/traffic becomes
`RegionHealthStatus.Unavailable` (or sustained `Degraded`) — see
`PlantSafety.Domain.MultiRegion.Services.IRegionFailoverPolicy` for the code-level decision of
which region to fail over to. This runbook is the *human* execution of that decision once made.

## Pre-requisites this runbook assumes (not yet true in this sandbox)

1. A region-scoped SQL Server Always On Availability Group (or region-scoped instance with
   log-shipping/replication) with a real secondary in the failover-target region, per
   `GeoReplicationOptions` (today every region reports `IsReplicaProvisioned = false` — this
   runbook cannot be executed for real until at least one region flips that to `true` against a
   real replica).
2. A region-scoped RabbitMQ cluster in the failover-target region, federated/shovel-linked to
   the primary region for the specific cross-region event types that must be preserved.
3. A real global traffic manager (DNS/Front Door/Traffic Manager/Global Accelerator) capable of
   cutting over client traffic from the failed region to the target region.

## Step 1 — Declare the incident and confirm the failover target

1. Confirm the affected region's health via the platform's own signals: `/health` (includes the
   `geo-replication` diagnostic check — see `GeoReplicationHealthCheck`) plus whatever real
   region-level monitoring exists once provisioned (this sandbox has none).
2. Call (or have an on-call runbook script call) `IRegionFailoverPolicy.DetermineFailoverTarget`
   with the affected region and a snapshot of every region's current health to get the
   deterministic failover target. Do not override this decision ad hoc without documenting why
   in the DR drill/incident record (see Step 5).

## Step 2 — SQL Server failover (region cutover)

1. If using an Always On Availability Group spanning regions: run a **manual, planned failover**
   (`ALTER AVAILABILITY GROUP [PlantSafetyAG] FAILOVER;` from the target region's secondary,
   promoting it to primary) if the source region is still reachable and the failover is planned;
   use **forced failover with data loss** (`FAILOVER WITH FORCE_FAILOVER_ALLOW_DATA_LOSS`) only
   if the source region is genuinely unreachable — and if forced, the resulting data-loss window
   MUST be checked against the region's target in `RegionalRpoRtoOptions`/`RegionalRpoRtoTarget`
   for its tier, and any breach logged as an "issue found" in the drill/incident record (Step 5).
2. If using region-scoped standalone instances (the sharded-by-tenant-affinity model — see
   `docs\Phase7\multi-region-architecture.md`'s §1 discussion) instead of a single spanning AG:
   there is no "failover" of the database itself — the target region's own instance already
   holds that region's tenants' data on its own schedule per `BackupOptions`/`RpoRtoOptions`.
   The action here is restoring the affected region's most recent backup into the target
   region's infrastructure (or provisioning a fresh instance in the target region and restoring
   into it), following the same restore steps as the existing single-instance
   `DR-manual-restore-runbook.md`, just executed in the target region rather than in place.
3. Re-point `PlantSafety.Infrastructure`'s connection-string resolution (today a single static
   connection string per environment; a real multi-region deployment requires this to become
   region-aware, per `docs\Phase7\multi-region-architecture.md`) to the newly-primary/restored
   database.

## Step 3 — RabbitMQ cluster failover

1. Stop routing new outbox dispatches (`PlantSafety.SharedKernel/Outbox`) to the affected
   region's broker — repoint the region-scoped MassTransit bus configuration
   (`PlantSafety.Infrastructure/Messaging`, `Shared.EventBus.Infrastructure`) to the target
   region's broker endpoint.
2. Drain/replay any in-flight messages still queued in the affected region's broker once it
   becomes reachable again (or accept their loss and record it as an "issue found" if the
   broker is permanently lost) — there is no automatic cross-region message replay mechanism in
   this codebase today; this is a manual reconciliation step.
3. Confirm consumers in the target region are running and processing the redirected traffic
   (the same `PlantSafety.Api` container image, per `PlantSafety.Api/Dockerfile`'s
   `MultiRegion__CurrentRegion` build-arg, just running in the target region with the target
   region's broker configured).

## Step 4 — DNS / traffic cutover

1. Update the global traffic manager (whichever platform is eventually chosen — see the
   flagged, still-undetermined deploy-platform ambiguity in
   `docs\Phase6\deployment-release-architecture.md`) to stop routing the affected tenants'
   traffic to the failed region and start routing it to the failover target region.
2. Confirm via `/health/live` and `/health/ready` on the target region's `PlantSafety.Api`
   instance that it is healthy before completing the cutover (do not cut traffic over to an
   unhealthy target).
3. Monitor error rates/latency for the newly-routed traffic for an initial stabilization window
   before declaring the cutover complete.

## Step 5 — Log the drill/incident

Whether this runbook was executed for a real incident or as a planned drill, log the outcome
using `RecordDrDrillCommand` (`PlantSafety.Application/Operations/DisasterRecovery/Commands`),
which raises a `DrDrillResult` domain event
(`PlantSafety.SharedKernel/Audit/DrDrillResult.cs`) recording:

- **Scope** — which components were exercised (e.g. "SQL Server AG failover UsEast -> UsWest",
  "Full regional cutover drill: SQL + RabbitMQ + DNS").
- **DrillDateUtc** — when it happened.
- **Outcome** — "Passed" / "Failed" / "PartialSuccess".
- **IssuesFound** — anything that did not go as expected (e.g. an RPO/RTO breach, a manual step
  that had no automation, a runbook step that was unclear or wrong).

This is logged via the same `ILogger`-based audit convention already used for feature-flag
toggles and secret rotations (see `docs\architecture\audit-logging.md`'s documented gap:
`dbo.AuditLog` has no INSERT-capable path from C# for non-tabular events with no backing SQL
table row of their own) — it is real, queryable (via log aggregation), structured evidence that
a drill happened, not a database row.

## Honest summary of what is and is not real here

| Step | Real today | Needs real infra |
|---|---|---|
| Failover-target decision | `IRegionFailoverPolicy`/`RegionFailoverPolicy` — real, deterministic, unit-testable | A real per-region health-signal feed to pass in (none exists) |
| SQL Server region cutover | The underlying single-instance backup/restore automation (Phase 6/12/18) is real | A real cross-region AG or region-scoped instance to fail over to/restore into; region-aware connection-string resolution in `PlantSafety.Infrastructure` |
| RabbitMQ cluster failover | — | A real region-scoped RabbitMQ cluster + federation/shovel config |
| DNS/traffic cutover | — | A real global traffic manager |
| Drill logging | `RecordDrDrillCommand`/`DrDrillResult` — real, working, logs today | — (this piece works standalone right now) |
