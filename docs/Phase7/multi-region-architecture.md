# Phase 7 §4.1 — Multi-Region Architecture

**Status:** Real code-level constructs delivered (Region model, routing/failover domain logic,
geo-replication health-check scaffold, formalized regional RPO/RTO targets). No actual
multi-region cloud infrastructure exists or was provisioned — this document is explicit about
that boundary throughout.

## Scope note (read first)

This solution runs, today, as a **single-instance** deployment: one SQL Server database set,
one RabbitMQ/MassTransit broker (`Shared.EventBus.Infrastructure`), one ASP.NET Core API
process (`PlantSafety.Api`), reachable at a single origin. There are no US East/US West/EU/APAC
regions actually running anywhere, no geo-replicated database, no real global load balancer, and
no live cross-region network path. Everything below is either (a) real, working C# domain/
application logic that is correct and unit-testable *regardless* of how many regions eventually
exist, or (b) a description of what real infrastructure a human/ops team would need to
provision to make (a) meaningful in production. Nothing here should be read as "deployed to 4
regions."

## What a real multi-region deployment of THIS platform would require

The platform's actual stack is SQL Server (Dapper-based repositories throughout
`PlantSafety.Infrastructure`), RabbitMQ/MassTransit for messaging (`Shared.EventBus.Infrastructure`,
`PlantSafety.Infrastructure/Messaging`), and ASP.NET Core (`PlantSafety.Api`). Grounded in that
real stack, not generic cloud advice:

### 1. Data tier — SQL Server

- **Today:** a single SQL Server instance (or single Always On availability group scoped to one
  region) hosts every `PlantSafety.*` database. `BackupDapperRepository`,
  `Persistence/Migrations/**/*.sql`, and the entire repository layer assume one connection
  string per environment (see `PlantSafety.Infrastructure/Persistence`).
- **What real multi-region requires:** a decision between two fundamentally different models,
  each with real trade-offs for this specific codebase:
  1. **Region-scoped SQL Server instances (data sharded by tenant-region affinity).** Each
     region (US East, US West, EU, APAC) runs its own independent SQL Server instance/AG,
     containing only the tenants whose `TenantRegionAffinity` resolves to that region (see
     §4.3 below — `ITenantRegionRouter`). This is the natural fit for **data residency**
     (the EU region's database physically never leaves the EU) but requires every
     connection-string resolution in `PlantSafety.Infrastructure` to become
     region-aware (today it is a single static connection string per environment) and requires
     cross-region reporting/analytics (e.g. `PlantSafety.Domain/Analytics`) to either federate
     queries across regions or replicate read models centrally.
  2. **Geo-replicated SQL Server Always On Availability Groups** (one logical database,
     asynchronous secondary replicas in other regions for DR read-scale/failover only, not
     tenant sharding). Simpler operationally (one schema, one set of stored procedures to
     maintain) but does NOT satisfy EU/APAC data-residency requirements on its own (the primary
     write replica is still in one region), and introduces real replication-lag semantics that
     `GeoReplicationHealthCheck` (§4.2 below) is scaffolded to report on once a real secondary
     exists.
  - **This platform's tenant/entity shape (SQL Server, ~400 stored procedures, Dapper
    repositories keyed by `TenantId`) makes model (1) — region-scoped instances keyed by tenant
    affinity — the more natural long-term fit**, because `TenantId` is already the universal
    partition key threaded through every command/query envelope (`ICommand<T>.TenantId`,
    `IQuery` conventions) and every stored procedure. `ITenantRegionRouter` (§4.3) is built
    against this assumption.

### 2. Messaging tier — RabbitMQ/MassTransit

- **Today:** a single RabbitMQ cluster is assumed (`Shared.EventBus.Infrastructure`,
  `PlantSafety.Infrastructure/Messaging`); all outbox-pattern dispatch
  (`PlantSafety.SharedKernel/Outbox`) publishes to it.
- **What real multi-region requires:** region-scoped RabbitMQ clusters (one broker per region,
  matching the region-scoped-SQL-instance model above), with **federation** or **shovel**
  plugins configured for the specific cross-region event types that must fan out globally
  (e.g. a canonical enterprise event mesh message — see
  `PlantSafety.SharedKernel/Contracts/Enterprise/EventMesh` — that every region's consumers
  need, vs. a region-local domain event that should never leave its region). The outbox
  dispatcher would need a region-aware routing key so a message produced in the EU region's
  broker is federated only to the specific other regions/exchanges that have subscribers,
  not broadcast to all four unconditionally.

### 3. API/compute tier — ASP.NET Core

- **Today:** `PlantSafety.Api` is a single ASP.NET Core process; `PlantSafety.Api/Dockerfile`
  (Phase 6) builds one region-agnostic container image.
- **What real multi-region requires:** one deployed instance of that same container image per
  region, each configured (via `IConfiguration`/environment variables) with its own region-local
  SQL connection string and RabbitMQ broker endpoint, sitting behind a **real global traffic
  manager** (Azure Front Door / Traffic Manager, AWS Global Accelerator, or a DNS-based
  GeoDNS solution — the specific choice is infrastructure the Phase 6 doc already flagged as an
  **undetermined target deploy platform**, and this document does not re-guess that decision).
  `ITenantRegionRouter` (§4.3) is the piece of *that* puzzle that lives in application code —
  it decides which region a tenant's request *should* go to; the actual network-level steering
  of the request to that region is the traffic manager's job, and no traffic manager is
  provisioned in this sandbox.

## Summary: real code vs. real infra needed

| Concern | Real code delivered this pass | Real infra still required |
|---|---|---|
| Region identity/metadata | `Region` Smart Enum (`PlantSafety.SharedKernel/Contracts/Enterprise/MultiRegion/Region.cs`) | — |
| Tenant → region routing decision | `ITenantRegionRouter` / `TenantRegionAffinityResolver` (`PlantSafety.Application/Enterprise/MultiRegion`) | A real global traffic manager to act on the routing decision at the network layer |
| Failover target decision | `IRegionFailoverPolicy` / `RegionFailoverPolicy` (`PlantSafety.Domain/MultiRegion/Services`) | A real health-signal feed per region (today none exists — see §4.2) and DNS/traffic-manager cutover automation to execute the decision |
| Geo-replication reporting | `GeoReplicationHealthCheck` (`PlantSafety.Api/HealthChecks`), `GeoReplicationOptions` | A real secondary SQL Server replica with queryable replication-lag telemetry (`sys.dm_hadr_*` DMVs) — none exists in this sandbox |
| Regional RPO/RTO targets | `RegionalRpoRtoOptions`/`RegionalRpoRtoTarget` (`PlantSafety.Infrastructure/Operations/Backup/Options`) | Actual per-region backup/restore automation and drilled, measured RTO figures |
| Region-scoped SQL instances / AG topology | — (documented above only) | Provisioning decision (region-scoped instances vs. geo-replicated AG), plus region-aware connection-string resolution in `PlantSafety.Infrastructure/Persistence` |
| Region-scoped RabbitMQ clusters + federation | — (documented above only) | Provisioning decision + region-aware outbox routing keys |
| Global traffic manager | — (documented above only) | Azure Front Door / Traffic Manager / AWS Global Accelerator / GeoDNS — platform choice still undetermined per Phase 6's flagged ambiguity |

See `docs\Phase7\multi-region-dr-runbook.md` for the concrete failover runbook and
`docs\Phase6\deployment-release-architecture.md` for the still-open deploy-target-platform
ambiguity this document does not re-decide.
