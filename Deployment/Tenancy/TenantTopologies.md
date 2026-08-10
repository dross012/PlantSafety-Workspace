# Tenant Topologies

Phase 18 (Deployment Modernization). Two deterministic topologies, selected via
`TenantConfig.Topology` (`Deployment.ConfigSchema.Contracts.TenantTopology`):

## Shared

The tenant runs on shared compute and shared network, isolated only by namespace scoping and
`ModuleBoundaryConfig` (`Deployment.Boundaries`). Suitable for small tenants where dedicated
infrastructure is not cost-justified.

- **Namespace scoping**: every resource (config snapshot, audit entry, deployment) is keyed by
  `TenantId` and never queried without it. `ConfigRegistry` partitions history per tenant
  (`Dictionary<string, List<ConfigSnapshot>>` keyed by `TenantId`) — there is no cross-tenant query
  surface in `ConfigRegistry`'s public API.
- **Resource quotas**: `TenantConfig.ResourceQuota` (`MaxMachineGroups`, `MaxConcurrentDeployments`,
  `MaxStorageBytes`) bounds how much of the shared pool a tenant may consume. Quota enforcement is a
  `DeploymentGovernanceRules` check (`Governance/`), not a separate infrastructure mechanism.
- **Network/storage scoping**: `ModuleBoundaryConfig.AllowedFacilityIds`/`AllowedDepartmentIds`/
  `AllowedMachineGroupIds` bound which real-world scopes a tenant's deployments may touch, checked
  deterministically by `ModuleBoundaryChecker.IsWithinBoundary`.

## Isolated

The tenant has dedicated compute, network, and storage — no resource is shared with any other
tenant at the infrastructure layer. Selected when a tenant's compliance, performance, or blast-radius
requirements exceed what namespace scoping alone can guarantee.

- Same `ModuleBoundaryConfig`/`ConfigRegistry` partitioning as Shared, plus the operational
  expectation (enforced by the hardening/provisioning playbooks, not by C# code, since it is a host
  allocation decision) that the tenant's compute pool and network segment are not shared with any
  other tenant's pool.
- `TenantResourceQuota` still applies (it bounds the tenant's *own* dedicated pool, not a shared
  one), so the same `ConfigValidator`/`DeploymentGovernanceRules` checks run unchanged regardless of
  topology.

## Choosing a topology

`TenantService.CreateTenantAsync` takes `Topology` as an explicit, caller-supplied field on
`CreateTenantRequest` — there is no default and no inference; the caller (an Admin-permissioned,
MFA-verified `CallerContext`, per `TenantService`'s own `AuthorizationCheck.Evaluate` call) must
choose deliberately.
