# Incident Response Runbook

Phase 18 (Deployment Modernization). Deterministic, ordered steps an operator follows when a
deployment or tenant-affecting incident occurs.

## 1. Triage

1. Identify the affected `TenantId` and `DeploymentId` from the alert (or from
   `AuditLog.GetEntries(tenantId)`/`DeploymentTelemetry`-emitted `LogEvent`s carrying `EventId`
   `DEPLOYMENT_STEP_EXECUTED`, `DEPLOYMENT_BLOCKED`, or `DEPLOYMENT_DRIFT_RECONCILED`).
2. Check `AuditLog.VerifyChainIntegrity(tenantId)`. If this returns `false`, treat the incident as a
   **potential tamper event**, not just an operational failure — escalate per your organization's
   security incident process in addition to the steps below.
3. Determine severity:
   - **Blocked deployment** (`DEPLOYMENT_BLOCKED` event present, no partial rollout occurred) — low
     severity, no user-facing impact; the governance gate did its job.
   - **Failed deployment mid-rollout** (`DeploymentExecutionResult.Succeeded: false`,
     `CompensationRan: true`) — medium severity; `DeploymentEngine` already ran compensation for
     every step that had succeeded, but confirm the compensated state is actually healthy (see
     step 3 below).
   - **Drift detected** (`DriftReport.DriftDetected: true`) — severity depends on the specific
     `ConfigChange`s reported; a change to `TenantConfig.Status` or `RoleConfig.GrantedPermissions`
     is high severity (potential unauthorized access change), a change to `ModuleConfig.FeatureFlags`
     is typically low.

## 2. Contain

1. If a deployment is actively rolling out and behaving unexpectedly, do **not** manually kill
   processes on the host — instead, if using Canary, the next stage gate will not run since
   `CanaryManager.ExecuteAsync` stops advancing at the first failed gate; if using Blue/Green,
   `BlueGreenManager.PromoteAsync` never switched active traffic if the promotion gates failed, so
   the previously-active slot is still serving traffic untouched.
2. If the tenant itself needs to be prevented from receiving further deployments while you
   investigate, call `ITenantLifecycleApi.SuspendTenantAsync` with a `Reason` describing the
   incident (see [`RollbackProcedure.md`](RollbackProcedure.md) if the tenant's config itself needs
   reverting).

## 3. Diagnose

1. Pull `AuditLog.GetEntries(tenantId)` for the full ordered history of actions leading up to the
   incident.
2. Pull `ConfigRegistry.ListVersions(tenantId)` and compare the last known-good version against the
   version active at incident time via `ConfigDiff.Compare` — this gives you the exact field-level
   change list, not a guess.
3. Cross-reference `DeploymentExecutionResult.StepResults` for the specific step and
   `FailureReason` string that stopped the run.

## 4. Resolve

- If the root cause is a bad config change: follow [`RollbackProcedure.md`](RollbackProcedure.md).
- If the root cause is a genuine infrastructure fault (not a config problem): fix the infrastructure,
  then re-run `DeploymentEngine.ExecuteAsync` with the same `DeploymentId` — every step is required
  to be idempotent, so re-running is safe.
- If the tenant was suspended in step 2, call `ITenantLifecycleApi.UpdateTenantAsync` is not the
  reactivation path (there is no `ReactivateTenant` method by design — suspension requires the same
  deliberate Admin+MFA action to reverse as any other lifecycle change; call
  `CreateTenantAsync`-adjacent tooling only if this was a genuine mis-suspension, otherwise leave the
  tenant Suspended pending a full incident review).

## 5. Post-incident

Write a postmortem referencing the exact `AuditEntry.EntryId`/`EntryHash` range and
`ConfigDiffResult` that caused the incident — the hash-chained audit trail and immutable snapshot
history mean the postmortem can cite exact, unambiguous evidence rather than a reconstructed
timeline.
