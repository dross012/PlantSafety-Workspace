# Tenant Isolation Model (Phase 7 §1.1)

## Chosen strategy: Shared Database + Row-Level Security + Explicit TenantId Propagation

PlantSafety uses a **shared-database, shared-schema** multi-tenancy model across
`PlantSafety.CoreDB`, `PlantSafety.ArchiveDB`, and `PlantSafety.DiagnosticsDB`. Tenant isolation is
enforced by SQL Server **Row-Level Security (RLS)**, not by per-tenant databases/schemas and not
by an ORM-level global query filter (ORM global filters are explicitly prohibited by the Handbook's
Prohibited Technology rule).

This is a **formalization of work already in the codebase** (Phase 3/9), not a new architectural
choice made by this pass. Phase 7 confirms the strategy is genuinely RLS-backed, closes the gaps in
how consistently it is applied, and adds the first-class `Tenant` aggregate other Phase 7 work
(Billing, Admin Portal) depends on.

## Why shared-DB + RLS (and not per-tenant databases/schemas)

- **Cost and operational simplicity at scale.** Per-tenant databases/schemas do not scale
  economically to the platform's target tenant count without a database-provisioning pipeline of
  their own; RLS achieves the same isolation guarantee with a single physical schema to migrate,
  back up, and monitor.
- **The platform's existing Outbox/messaging/analytics infrastructure already assumes a shared
  schema.** Every domain event, outbox row, and analytics fact/dimension table already carries an
  explicit `TenantId` column rather than living in a tenant-specific namespace. Moving to
  per-tenant databases would require re-architecting all of that, for no isolation benefit RLS does
  not already provide.
- **Deterministic, auditable, defense-in-depth isolation.** RLS is enforced at the SQL engine
  itself — the last line of defense after the API and Application layers — so a bug in any single
  layer above it cannot, by itself, leak cross-tenant data.

## How the layers compose end-to-end

1. **API layer (Zero-Trust tenant resolution).**
   `TenantResolutionMiddleware` resolves `TenantId` (and, as of Phase 7, `Region`) from the
   authenticated JWT's `tenantId`/`region` claims — **never** from client-supplied query/body
   input — and stores them in `HttpContext.Items` under `TenantContextAccessor`'s key constants.
   `IOperationContext`/`OperationContextAccessor` (Application-layer abstraction, API-layer
   implementation) surface this as `IOperationContext.TenantId`/`.Region` to every handler.

2. **Application layer (defense-in-depth against client-supplied TenantId).**
   `TenantIsolationBehavior<TRequest,TResponse>` is a MediatR pipeline behavior that rejects any
   request whose bound public `TenantId` property does not match the server-resolved
   `IOperationContext.TenantId`. This closes the gap where a client-supplied `TenantId` on a
   Command/Query DTO could otherwise be trusted over the JWT-resolved value.
   *Phase 7 note:* genuinely cross-tenant, platform-admin operations (the new `Tenancy.Tenant`
   module's provisioning/lifecycle commands) intentionally do **not** expose a public `TenantId`
   property matching this reflection check (see `ProvisionTenantCommand`/`SuspendTenantCommand`/etc.
   remarks) — they carry a `TargetTenantId` and satisfy `ICommand<TResponse>.TenantId` via an
   explicit interface implementation invisible to that reflection lookup. Authorization for those
   commands is instead enforced by `TenantAuthorizationPolicies.Manage` (PlatformAdmin-and-above) on
   the controller action. This is the correct, intentional carve-out for platform administration,
   not a weakening of the control for ordinary tenant-scoped requests.

3. **Infrastructure/SQL layer (RLS is the enforcement backstop).**
   - `Security.fn_TenantAccessPredicate(@TenantId)` (a `SCHEMABINDING` inline table function) is
     applied as both a `FILTER` predicate (SELECT/UPDATE/DELETE visibility) and a `BLOCK` predicate
     (INSERT/UPDATE) via `Security.TenantAccessPolicy` on every table across CoreDB/ArchiveDB/
     DiagnosticsDB with a `NOT NULL uniqueidentifier TenantId` column — auto-discovered by a live
     query, so new tenant-scoped tables are picked up automatically on the next migration run.
   - `TenantSessionContextExtensions.SetTenantSessionContextAsync` sets
     `SESSION_CONTEXT('TenantId')` on the Dapper connection before any RLS-protected table is
     touched, reading the tenant id back out of the same `DynamicParameters` every repository
     already builds via `StoredProcedureBuilder` (no new ambient-context abstraction needed).
     `DapperQueryExecutor`/`DapperCommandExecutor` call this automatically on every stored-procedure
     invocation; `TenantContextPropagationOptions.EnableTenantContextPropagation` is a staged
     rollout gate that, once enabled, makes an absent TenantId parameter a hard failure
     (`TenantContextRequiredException`) instead of a silent no-op.
   - **Deliberate, documented exclusions** from the RLS policy (a table's `TenantId` column is
     itself the row's identity, not a foreign reference to an owning tenant, or the table is
     shared cross-tenant platform-operations data): `AuditDB.dbo.AuditLog`, `ErrorDB.dbo.ErrorLog`,
     `ArchiveDB.Archival.ArchivalJob` (all pre-existing), and — new in Phase 7 —
     **`Tenancy.Tenant`** (the tenant registry itself; see `Tenant.sql`'s header and
     `90-row-level-security.sql`'s new exclusion clause). `Tenancy.TenantApiKey` and
     `Tenancy.TenantAdminRoleAssignment` are genuinely tenant-scoped and are **not** excluded.

4. **Event bus / Outbox (tenant-scoped propagation).**
   Every `OutboxMessages` row and `EnterpriseEventEnvelope` carries `TenantId` (and, as of Phase 7,
   `Region`) so tenant context survives the hop from a synchronous request into asynchronous
   cross-module/cross-system event processing. `CrossModuleIdempotentConsumerBase.EnsureExpectedTenant`
   adds a consumer-side defense-in-depth check (Phase 7 §1.3), mirroring `TenantIsolationBehavior`'s
   API-layer guarantee at the message-consumer boundary.

5. **Analytics layer.**
   `AnalyticsUnifiedModel` fact/dimension tables (Phase 5) follow the platform's StrongId+TenantId
   convention; every fact table carries a `NOT NULL uniqueidentifier TenantId` column and is
   therefore automatically covered by the same RLS auto-discovery query as every other business
   table (no separate mechanism needed).

## Scheduling (Phase 7 §1.4)

The Predictive Scheduling optimizer's `SchedulableTask`/`ResourceAvailabilityWindow` value objects
previously carried **no** `TenantId` at all (a real gap — the only tenant scoping was on the
containing `OptimizationRun` aggregate). Phase 7 adds an optional `TenantId` property to both value
objects (default `Guid.Empty`, additive/non-breaking) and a guard in `OptimizationRun.Request` that
throws `OptimizationRunCrossTenantDataException` if any supplied task/window carries a non-empty
`TenantId` that does not match the run's own tenant — closing the "never mix records across
different TenantIds in the same optimization run" requirement without breaking any existing caller
that does not yet populate the new property.

## Known interaction to flag (not silently absorbed)

The `Tenancy.Tenant` write/read repositories' `ListAllAsync`/`NameExistsAsync`/`GetByNameAsync`
calls legitimately issue **no** `@TenantId` parameter (there is no single tenant to scope a
cross-tenant registry lookup/listing by). If
`TenantContextPropagationOptions.EnableTenantContextPropagation` is ever enabled platform-wide,
these three call sites will need an explicit allow-list entry rather than being treated as an
unmigrated gap — flagged in `TenantDapperRepository`'s file header for the same reason.
