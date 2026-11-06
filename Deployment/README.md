# Deployment — Phase 18 (Deployment Modernization)

The final planned modernization phase: a deterministic, auditable, multi-tenant on-prem deployment
platform. Hardened servers, configurable tenant/role/module boundaries, a versioned configuration
registry with immutable snapshots and rollback, a deterministic deployment engine with Blue/Green and
Canary rollout strategies, continuous drift reconciliation, and integrated telemetry/governance —
built on the same deterministic-rules, decorator-chain, and Shared.Authorization conventions every
phase since Phase 8 has used.

## Project layout note

Unlike Phases 14–17 (`Shared.<Module>.Abstractions/Infrastructure/Tests`), this phase's own prompt
specifies an explicit, flat `Deployment/` folder tree rather than that three-project split, so this
module follows that tree literally: a single `Deployment.csproj` (targeting **.NET 10**, per this
phase's own requirement — every other `Shared.*` project in this repo targets net8.0; a net10.0
project referencing a net8.0 library compiles cleanly, confirmed while building this module) covers
every production `.cs` file under the folders below, and `Deployment/Tests/Deployment.Tests.csproj`
covers `Tests/Unit/` and `Tests/Integration/`. Two `.csproj` files were necessary for the code to
compile at all (C# has no project-less compilation unit) but are not literally listed in the phase
prompt's required file tree — the only intentional deviation from the requested structure.

```
Deployment/
    Deployment.csproj
    Hardening/                          -- server hardening baseline, scripts, network policy, secrets
    Tenancy/                            -- tenant lifecycle API + service, topology/onboarding docs
    Boundaries/                         -- RBAC role schema/validator, module/facility/dept/machine-group scoping
    ConfigSchema/
        Contracts/                      -- DeploymentConfig, TenantConfig, RoleConfig, ModuleConfig
        Registry/                       -- ConfigRegistry, ConfigSnapshot, ConfigDiff, ConfigValidator
        JsonSchemas/                    -- JSON Schema for every Contracts/ type
        SampleManifests/                -- Shared-topology, Isolated-topology, and Canary-strategy sample JSON
    Orchestration/
        DeploymentEngine.cs             -- ordered, idempotent, timeout/retry/compensation step executor
        RolloutStrategies/              -- BlueGreenManager, CanaryManager
        DriftReconciler.cs
    Observability/                      -- DeploymentTelemetry, DeploymentMetrics, DeploymentTracing, AuditLog
    Governance/                         -- DeploymentGovernanceRules, DeploymentGovernanceEvaluator, GovernanceEvents
    Tests/
        Deployment.Tests.csproj
        Unit/
        Integration/
    Runbooks/                           -- IncidentResponse.md, RollbackProcedure.md
    README.md
```

## Determinism, explicitly

This phase's own constraints are stricter than any prior phase: **no reflection, no dynamic mapping,
no AutoMapper, no partial classes.** Every place a lesser design would reach for reflection instead
uses explicit, hand-written code:

- `ConfigSnapshotFactory`'s `ContentHash` is a SHA-256 over an explicitly, field-by-field-built
  canonical string (ids sorted ordinal) — not a reflection-based object hasher.
- `ConfigDiff.Compare` is explicit, named field-by-field comparison (`AddIfChanged` calls per field)
  across `DeploymentConfig`/`TenantConfig`/`RoleConfig`/`ModuleConfig` — not a generic diff library.
- `RoleValidator`/`ConfigValidator.ValidateModuleDependencies` detect inheritance/dependency cycles
  via the same explicit white/gray/black DFS `Shared.SafetyGraph.Abstractions.Traversal.
  SafetyGraphTraversalEngine.DetectCycle` (Phase 16) already established — not a generic graph
  library.
- `RolePermissionSchema.ResolveEffectivePermissions` walks `RoleConfig.InheritsFromRoleIds` via an
  explicit breadth-first traversal over a caller-supplied dictionary — never via reflection over
  `RoleConfig` instances.

## The two engines that actually deploy something

`DeploymentEngine.ExecuteAsync` runs a fixed, ordered list of `DeploymentStep`s (each with its own
`Timeout`/`MaxRetries`), stopping at the first step that exhausts its retry budget and then running
every previously-succeeded step's `Compensation` in reverse order. It does not decide *how much*
traffic a new version receives — that is `BlueGreenManager`/`CanaryManager`'s job:

- **`BlueGreenManager.PromoteAsync`** runs every supplied `BlueGreenGate`, in order, against the idle
  slot; traffic switches only if every gate passes. Any failed gate is an explicit rollback trigger
  — the active slot never changes.
- **`CanaryManager.ExecuteAsync`** walks `DeploymentConfig.CanaryStagePercentages` in ascending order
  (the same ordering `ConfigValidator.ValidateDeploymentConfig` already enforces at the config level),
  running one gate per stage. The first failed stage stops all further promotion.

`DriftReconciler.Reconcile` compares an actually-observed `ConfigSnapshot` against the registry's
latest published snapshot via the same `ConfigDiff.Compare` used for rollback diffing — one diff
implementation, two call sites, never a second parallel one.

## Governance actually blocks deployments

`DeploymentGovernanceEvaluator.Evaluate` runs `ConfigValidator.ValidateSnapshot` (structural
validity) plus every `DeploymentGovernanceRules` policy check (tenant must be Active, quota must not
be exceeded, a "production" `EnvironmentName` deployment must have an assigned role that effectively
grants `ApproveDeployments`), and returns a `DeploymentGovernanceDecision` whose `Allowed` field is
the actual enforcement point — any violation, structural or policy, blocks the deployment and emits
both `DEPLOYMENT_GOVERNANCE_VIOLATION` and `DEPLOYMENT_BLOCKED` events. There is no partial-allow.

## Tenant lifecycle and isolation

`TenantService` implements `ITenantLifecycleApi` (`CreateTenant`/`UpdateTenant`/`SuspendTenant`/
`DeleteTenant`), and every mutating call runs through the real, unchanged
`Shared.Authorization.Abstractions.Checks.AuthorizationCheck` (Phase 9) for module `"Deployment"`,
`ModulePermissionVerb.Admin` — the same tier `AuthorizationCheck` itself already requires
`MfaStatus.Verified` for, so tenant lifecycle operations get MFA enforcement without this module
inventing a second MFA gate. Isolation (namespace scoping, resource quotas, module/facility/
department/machine-group scoping) is documented in [`Tenancy/TenantTopologies.md`](Tenancy/TenantTopologies.md)
and enforced by `ConfigRegistry`'s per-tenant partitioning plus `ModuleBoundaryChecker`'s explicit
containment checks.

## Immutable audit trail

`AuditLog` is append-only and hash-chained (each entry's `EntryHash` folds in the previous entry's
hash, computed the same explicit-field-concatenation-then-SHA-256 way `ConfigSnapshotFactory` hashes
snapshots) — `VerifyChainIntegrity` deterministically detects any entry altered or removed after the
fact, per-tenant.

## Metrics and log categories added this phase

- `LogCategory.Deployment` (`Shared.Telemetry.Abstractions.Events.LogCategory`)
- `LogCategoryClassifier.Classify("deployment")` -> `LogCategory.Deployment`
- `PlantSafetyMetrics.DeploymentStepsExecuted`
- `PlantSafetyMetrics.ConfigSnapshotsPublished`
- `PlantSafetyMetrics.DriftReconciliationsPerformed`
- `PlantSafetyMetrics.DeploymentGovernanceViolationsDetected`

No new Meter instance was created — every Deployment counter is published through the same
`PlantSafetyMetrics.Meter` every other module's telemetry already uses.

## Running the tests

```bash
cd Deployment/Tests
dotnet run
```

37 unit + integration tests cover `ConfigValidator`, `DeploymentEngine` (success, failure +
compensation, retry exhaustion, retry-then-succeed), `TenantService`/`ITenantLifecycleApi`
(including the MFA-gated Admin authorization path), tenant isolation across `ConfigRegistry`/
`ModuleBoundaryChecker`/`AuditLog`, and end-to-end rollout scenarios (`BlueGreenManager` +
`DeploymentEngine` + `DriftReconciler`, `CanaryManager` mid-rollout gate failure, `ConfigRegistry`
rollback, and `DeploymentGovernanceEvaluator` blocking a non-compliant production manifest).

## What "done" means for this phase, honestly

Every contract, engine, and gate described above is real, compiled, and tested. What this phase does
**not** include, by design, is a live host integration: `HardeningOrchestrator.ApplyHardeningAsync`
(runs a real shell script via `Process`) and `ISecretsAdapter` (a real vault client) are both
documented, real seams with no concrete host-facing implementation shipped in this module — the same
"dormant capability, no live caller, documented honestly" pattern every phase since Phase 8 has used
for the parts of a system that require a real operational environment to exercise. Pilot rollout,
hardening validation against real hosts, and operator runbook adoption are the operational activities
that follow this phase, not additional code.
