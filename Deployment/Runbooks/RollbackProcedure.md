# Rollback Procedure

Phase 18 (Deployment Modernization). Deterministic, ordered steps to roll a tenant's configuration
back to a prior, known-good `ConfigSnapshot`. This procedure never rewrites history — `ConfigRegistry`
is append-only, so "rolling back" means **publishing a new snapshot whose content matches an older
one**, not deleting or mutating anything.

## Prerequisites

1. A `CallerContext` with `ManageConfig`-capable permissions (an Admin-tier role, per
   [`../Boundaries/RolePermissionSchema.cs`](../Boundaries/RolePermissionSchema.cs)) and verified MFA.
2. The target `SnapshotVersion` to restore, identified via
   [`IncidentResponse.md`](IncidentResponse.md)'s diagnosis step or via
   `ConfigRegistry.ListVersions(tenantId)`.

## Steps

1. **Confirm the target version exists and is actually the intended state.** Call
   `ConfigRegistry.GetVersion(tenantId, targetVersion)` and review its `DeploymentConfig`/
   `TenantConfigs`/`RoleConfigs`/`ModuleConfigs` and `ContentHash` before proceeding — there is no
   "undo" for a bad rollback target choice beyond rolling back again.
2. **Diff the target against the current latest**, for the record: `ConfigDiff.Compare(target,
   registry.GetLatest(tenantId))`. Attach this `ConfigDiffResult` to the incident/change ticket —
   it is the exact, unambiguous list of what rolling back will change.
3. **Call `ConfigRegistry.RollbackTo(tenantId, targetVersion, nowUtc)`.** This publishes a brand-new
   snapshot (at `latestVersion + 1`) carrying the target version's exact content.
   `DeploymentTelemetry.RecordSnapshotPublished` fires automatically, and the new snapshot's
   `ContentHash` will exactly match the target's `ContentHash` (same content, different
   `SnapshotVersion`/`CreatedAtUtc`) — verify this as a sanity check.
4. **Run `ConfigValidator.ValidateSnapshot`** against the newly-published rollback snapshot before
   it is used for any real deployment. It should pass, since it was validated when originally
   published — if it now fails, something about the *validation rules themselves* changed since the
   target version was published; do not proceed until this is understood.
5. **Run `DeploymentGovernanceEvaluator.Evaluate`** against the rollback snapshot before executing
   any deployment against it — the same blocking gate every forward deployment goes through applies
   equally to a rollback; a rollback is not exempt from governance.
6. **Record the rollback in the audit trail** via `AuditLog.Append(tenantId, "ConfigRolledBack",
   actorDescription, correlationId, nowUtc, logEventWriter)` if your integration does not already do
   this automatically inside step 3's caller.
7. **Re-run `DeploymentEngine.ExecuteAsync`** for any deployment that needs to reflect the
   rolled-back configuration. Every step must be idempotent, so this is safe even if a prior,
   now-reverted deployment partially ran.

## Verifying success

- `ConfigRegistry.ListVersions(tenantId)` shows the new version appended (history length increased
  by exactly 1).
- `ConfigRegistry.GetLatest(tenantId).ContentHash` equals the target version's `ContentHash`.
- `AuditLog.VerifyChainIntegrity(tenantId)` still returns `true`.
