# Tenant Onboarding Playbook

Phase 18 (Deployment Modernization). Deterministic, ordered steps to onboard a new tenant. Every
step is idempotent — re-running a completed step is a no-op, not an error, so an operator can safely
resume this playbook after an interruption.

## Prerequisites

1. A `CallerContext` with the `deployment-admin` role (or an inherited-equivalent role granting
   `DeploymentPermission.ManageTenants`) and `MfaStatus.Verified` — `TenantService` rejects
   `CreateTenantAsync` calls that don't meet `ModulePermissionVerb.Admin`, which itself requires
   verified MFA (`Shared.Authorization.Abstractions.Checks.AuthorizationCheck`, Phase 9).
2. A decision on `TenantTopology` (`Shared` or `Isolated`) — see [`TenantTopologies.md`](TenantTopologies.md).
3. A `TenantResourceQuota` sized for the tenant's expected load.

## Steps

1. **Reserve the TenantId.** Choose a stable, lowercase, hyphenated identifier (e.g.
   `acme-plant-1`). `TenantService.CreateTenantAsync` fails deterministically
   (`TenantLifecycleResult.Succeeded: false`) if the id is already in use — safe to retry with the
   same id if step 2 has not yet succeeded.
2. **Call `ITenantLifecycleApi.CreateTenantAsync`** with the chosen `TenantId`, `TenantName`,
   `Topology`, `ResourceQuota`, and the initial `AssignedModuleIds`/`AssignedRoleIds`. This creates
   the tenant in `TenantStatus.Active`.
3. **Publish the tenant's first `ConfigSnapshot`** via `ConfigRegistry.Publish` (`SnapshotVersion: 1`)
   — must include the new `TenantConfig`, the module set from `AssignedModuleIds` (as full
   `ModuleConfig` entries), and the role set from `AssignedRoleIds` (as full `RoleConfig` entries,
   at minimum `deployment-viewer` from `RolePermissionSchema.BuildViewerRole`).
4. **Run `ConfigValidator.ValidateSnapshot`** against the published snapshot before it is used for
   any real deployment. A non-empty violation list must block progression to step 5 — this is the
   same pre-deploy gate `DeploymentGovernanceEvaluator` runs automatically inside
   `DeploymentOrchestrator` (not written separately here to avoid duplicating that check).
5. **Run hardening validation** (`HardeningOrchestrator.ValidateBaseline`) against the tenant's
   target compute pool (shared pool for `Shared` topology, dedicated pool for `Isolated`) before the
   first deployment is scheduled.
6. **Execute the tenant's first deployment** via `DeploymentEngine`/`DeploymentOrchestrator`, using
   the snapshot published in step 3.

## Rollback

If any step fails, no destructive action is taken automatically. Steps 1–2 are reversed by calling
`DeleteTenantAsync` with a `Reason` describing the failed onboarding attempt (see
[`../Runbooks/RollbackProcedure.md`](../Runbooks/RollbackProcedure.md)); steps 3+ are reversed by
never referencing the published snapshot from a real deployment (the registry is append-only —
publishing an onboarding snapshot is not itself destructive and does not need to be "undone").
