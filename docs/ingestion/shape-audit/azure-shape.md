# Shape Audit — Azure

Phase 5 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `module-flow/azure.md`, `classification-matrix.md` (Phases 1-3).

## Top findings for Azure (all directly verified)

1. **There is no DTO→domain→SQL chain to audit in this module — Azure's "repository" layer is an
   outbound Azure Resource Manager (ARM) API call, not a database write.** This was established in
   Phase 3 and re-confirmed this phase: `ContainerAppRepository`
   (`Azure\Compute.Infrastructure\Persistence\ContainerAppRepository.cs`) wraps `ContainerAppsClient`
   (the Azure ARM SDK), not a `DbContext`/Dapper connection. Grepping the entire `Azure` tree for
   `.sql` files returns **zero results**, and grepping for `DbContext` returns zero results. This
   phase's core mandate (DTO → domain entity → EF/SQL column) **does not apply** to Azure's
   lifecycle-mutating commands (Restart, Scale, Purge, Tier/Throughput update, etc.) — there is no
   local table whose shape could drift from the DTO.
2. **The one place Azure *could* have a shape-drift problem — its 18 domain events — has no SQL
   persistence to drift against either.** `ContainerAppRestartedEvent` and its 17 siblings are
   published in-process via MediatR (`IPublisher.Publish`) and consumed by a single same-subdomain
   `INotificationHandler<T>` that writes one `ILogger` line (Phase 3, directly verified). No event
   is ever serialized to a table, so there is no "domain event contract vs. stored column" shape
   question here either — the closest thing to a persisted shape is the free-text log line itself,
   which is not schema-checked at all.
3. **Read-side DTOs (`ContainerAppSummaryDto`/`ContainerAppDetailDto`) map from ARM SDK response
   objects, not from a database row** — `ContainerAppMapper.cs` was spot-checked to confirm the
   mapping source is the ARM client's response model, not a repository row type. Any shape mismatch
   here would be a DTO-vs-ARM-API contract issue (out of scope for this SQL-focused phase, and
   arguably Azure's own SDK's problem to keep in sync, not this codebase's).

## Why this module is out of scope for the rest of this audit's table format

The deliverable's required columns (EF/SQL Column Type, Nullability Match, Truncation Risk, Enum
Drift, Populated By Any Ingestion Path) all presuppose a SQL column exists to compare against. For
Azure, across all 9 subdomains (Compute, Data, Eventing, Identity, ML, Networking, Observability,
Security, Storage) and all 63 endpoints, that column does not exist anywhere in this solution — the
real Azure resource IS the store, and it lives outside this codebase entirely (in the actual Azure
subscription/tenant the deployed app targets). Producing a DTO→SQL table for this module would
either be empty or would have to fabricate SQL columns that don't exist in source, which the task's
own "directly-verified vs pattern-inferred" discipline forbids.

## What was directly checked to reach this conclusion

- `find Azure -iname "*.sql"` → 0 results (module-wide, all 9 subdomains).
- `grep -rl "DbContext" Azure` → 0 results (module-wide).
- `Azure\Compute.Infrastructure\Persistence\ContainerAppRepository.cs` — read in Phase 3, wraps
  `ContainerAppsClient` (ARM SDK) behind a Polly resilience pipeline, no connection string.
- `Azure\Compute.Application\Commands\RestartContainerAppCommand.cs`,
  `Azure\Compute.Application\Validators\RestartContainerAppCommandValidator.cs`,
  `Azure\Compute.Application\Events\ContainerAppRestartedEvent.cs`,
  `Azure\Compute.Application\EventHandlers\ContainerAppRestartedEventHandler.cs` — all read this
  phase to confirm no serialization step exists between the command/event objects and any durable
  store other than the ARM control plane and the application log stream.

## Recommendation for a future phase

If Azure is ever extended with a **local** cache/mirror of ARM resource state (e.g. to avoid a live
ARM call on every read, or to support offline reporting), that would be the first point at which
this module needs a genuine shape audit. As currently built, this module contributes **zero** rows
to the solution-wide shape-drift findings — not because it was skipped, but because the DTO→SQL
seam this phase is auditing does not exist in Azure's architecture.
