# Phase 9 — Authorization Modernization

## Scope corrections made before writing any code

1. **Diagnostics/Routing/Scheduling dropped again.** The prompt (and the follow-up architectural
   answer) both re-listed these as modules to add authorization to. Confirmed in Phase 8, still
   true: none of these exist anywhere in this program's real code. No `DiagnosticsAuthorization.cs`,
   `RoutingAuthorization.cs`, or `SchedulingAuthorization.cs` was written — there is nothing to wrap.
2. **API-layer enforcement was explicitly not enabled.** `PlantSafety.UI.Api` has no authentication
   scheme, no tokens, no session state, no identity provider — confirmed standing fact since Phase
   3.9, still true. Enabling `[Authorize]` + `app.UseAuthorization()` would have broken every live,
   verified endpoint built across Phases 4, 7, and 8 with a startup/request-time exception (`No
   authentication handlers are registered`), not a clean 401. Nothing was added to `Program.cs`'s
   middleware pipeline for this reason.

## The enforcement model actually built: CallerContext

Since there's no real identity at the API boundary, authorization is enforced **inside the domain
layer**, against an explicit, deterministic, pure-data identity surrogate:

```csharp
public sealed record CallerContext(EnterpriseRole Role, MfaStatus MfaStatus, string CorrelationId)
{
    public static CallerContext SystemDefault(string correlationId) =>
        new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, correlationId);
}
```

`CallerContext` is not authenticated identity — it's an explicit stand-in that lets
`Shared.Authorization`'s real, existing role/permission model be exercised and made observable
today, without inventing an authentication scheme this phase was never asked to build.
`SystemDefault()` grants `EnterpriseRole.PlatformAdmin` (the highest tier), documented in the type's
own header comment: not because "the system" is a privileged actor, but because there is no real
identity to grant a lower tier to, and defaulting to `Viewer` would silently start rejecting every
existing, working request the moment authorization checks were wired in.

## What Shared.Authorization already provided (verified before building anything on top of it)

- `SafetyRoles` / `EnterpriseRole` — a real, evidence-based 4-tier model (Viewer/Editor/Approver/
  PlatformAdmin), reconciled from ~140 existing per-module policy classes in the (unrelated)
  `PlantSafety.Api` repository these types were originally extracted from.
- `RoleInheritanceResolver` — deterministic rank-based inheritance (`Satisfies`, `GetInheritedRoles`,
  `ResolveHighestRole`). Existed on disk, fully written, but had **zero test coverage** and had
  **never been committed** to this repository (see "discovery" below) before this phase.
- `EnterprisePermissionResolver` / `IPermissionResolver` — resolves `"{Module}.{Verb}"` permission
  strings from a role, using `ModulePermissionVerb` (Read=0/Write=1/Approve=2/Admin=3, ranked 1:1
  against `EnterpriseRole`). Also existed uncommitted.
- `MfaPolicy.RequireMfa` — a policy-*name* constant only. The actual claim-checking logic (checking
  the OIDC `"amr"` claim for `"mfa"`) lives in the unrelated `PlantSafety.Api` repository, not here.
  No real MFA evaluation logic was invented to fill that gap — `MfaStatus` has an explicit
  `NotApplicable` state specifically so this isn't silently misrepresented as "MFA checked and
  passed."

**None of the role/permission/tier model was changed.** Phase 9 added a caller-identity type
(`CallerContext`), a pure decision function (`AuthorizationCheck`), and the telemetry/governance
wiring around them — it did not touch `SafetyRoles`, `EnterpriseRole`, `RoleInheritanceResolver`,
`EnterprisePermissionResolver`, `ModulePermissionVerb`, or `MfaPolicy` themselves.

## What was built

### `Shared.Authorization.Abstractions` (extended)
- **`Caller/CallerContext.cs`**, **`Caller/MfaStatus.cs`** — the identity surrogate described above.
- **`Checks/AuthorizationDecision.cs`**, **`Checks/AuthorizationCheck.cs`** — `AuthorizationCheck.Evaluate`
  is the single, pure `(CallerContext, module, verb) → AuthorizationDecision` function every domain
  wrapper calls. No "first match wins," no priority overrides, no weighted scoring — a caller is
  authorized if and only if `EnterprisePermissionResolver.Resolve(role, module)` contains the exact
  `"{module}.{verb}"` string. `EvaluateWithMfa` composes the same check with an `MfaStatus.Verified`
  requirement.

### `Shared.Authorization.Infrastructure` (new project)
- **`Telemetry/AuthorizationTelemetry.cs`** — reports an `AuthorizationDecision` through the existing
  Phase 8 `Shared.Telemetry` primitives (`ILogEventWriter`/`LogEvent`, `PlantSafetyMetrics`). No new
  logging, metrics, or tracing primitive — three new counters (`AuthorizationChecksPerformed`,
  `AuthorizationDenials`, `MfaRequirementFailures`) added to the same `Meter` `PlantSafetyMetrics`
  already owns.
- **`Governance/AuthorizationGovernanceRules.cs`**, **`Governance/AuthorizationGovernanceEvaluator.cs`**
  — pure validation of `CallerContext` shape and permission-string shape, reported through the
  **same** `PlantSafetyMetrics.GovernanceViolationsDetected` counter and log-event pattern Phase 8's
  `TelemetryGovernanceEvaluator` already established, rather than a second, parallel governance
  mechanism.

### Domain wrappers (WorkflowEngine, Facility, Audit — the only three real execution paths)
- **`Shared.WorkflowEngine.Infrastructure/Authorization/WorkflowEngineAuthorization.cs`** — wraps
  Phase 8's `WorkflowEngineTelemetry.EvaluateWithTelemetryAsync`. `WorkflowEngine.Evaluate` is
  read-only (no state anywhere in this program's WorkflowEngine, verified across Phases 4/7), so it
  requires `ModulePermissionVerb.Read` — the correct existing verb for a query, not an invented one.
  **This is the one real, live execution path wired end-to-end**: `WorkflowEngineController.Evaluate`
  now constructs `CallerContext.SystemDefault(correlationId)` and calls this wrapper.
- **`Shared.FacilityDomain.Infrastructure/Authorization/FacilityAuthorization.cs`** — wraps all three
  of Phase 8's `FacilityTelemetry` methods, same `Read` verb, same "no live caller in
  `PlantSafety.UI.Api` today" caveat Phase 8 already documented for the underlying rules.
- **`Shared.AuditLogging.Infrastructure/Authorization/AuditAuthorization.cs`** (separate repository —
  see below) — `AuthorizedAuditLogRepository` decorates Phase 8's `TelemetryAuditLogRepository`, same
  `Read` verb, same "no live caller today" caveat.

On deny, every wrapper throws `UnauthorizedAccessException` — deliberately not a new exception type.
`Shared.ErrorHandling`'s existing `ExceptionClassifier` (Phase 2.3) already maps
`UnauthorizedAccessException → ErrorCategory.Unauthorized`, and `ErrorCategoryHttpMapping` already
maps that to HTTP 401. Reusing this means a denied request gets a correctly classified 401 through
the existing `GlobalErrorHandler` pipeline with **zero new error-handling code**.

## Verified live

`dotnet run` against `PlantSafety.UI.Api`, `POST /api/v1/ui/engines/workflow/evaluate` with a
Catastrophic/Fire context: still returns the same `WorkflowDecision` as before this phase (status
200, byte-identical payload) — confirming `CallerContext.SystemDefault`'s `PlatformAdmin` grant
preserves existing behavior exactly. The structured JSON console log now shows
`AUTHORIZATION_CHECK_PERFORMED` (`Outcome: Allow`, `CallerRole: Platform.Admin`) immediately before
`WORKFLOW_DECISION_EVALUATED`, both carrying the request's real correlation ID — the authorization
pipeline is genuinely running and observable, not merely present in source.

## A discovery made while doing this work, unrelated to authorization logic itself

`RoleInheritanceResolver.cs`, `EnterpriseRole.cs`, `IPermissionResolver.cs`,
`EnterprisePermissionResolver.cs`, `ModulePermissionVerb.cs`, and `MfaPolicy.cs` all existed,
complete and correct, on disk in the `Shared.Authorization` repository — but **had never been
committed**. Only `SafetyRoles.cs`/`SafetyTierPolicies.cs` made it into that repository's one
existing commit (`8ee1bbf`, 2026-07-27); the other six files (dated 2026-08-02, attributed to a
different prior agent) sat as uncommitted working-tree changes for over a week. This phase's commit
captures that pre-existing work into version control alongside its own additions, rather than
leaving it permanently unrecorded.

Also reconfirmed (first found while building Phase 8): `Shared.AuditLogging`, `Shared.EventBus`, and
`Shared.Authorization` are each independently version-controlled git repositories, never tracked by
the outer modernization program repository at all. `AuditAuthorization.cs` is therefore committed
separately, in `Shared.AuditLogging`'s own repository, not here.

## What was not built (and why)

- No ASP.NET Core authentication scheme, JWT/cookie/OIDC/OAuth handler, or `IdentityServer`
  integration — explicitly out of scope per the corrected enforcement model above.
- No `[Authorize]` attributes, `PermissionAttribute`/`RoleRequirementAttribute`/`MfaRequirementAttribute`
  on any controller, and no `app.UseAuthorization()` call — enabling any of these without a real
  authentication scheme would break every existing endpoint.
- No `DiagnosticsAuthorization.cs`/`RoutingAuthorization.cs`/`SchedulingAuthorization.cs` — none of
  those modules exist in this program.
- No new roles, permissions, MFA semantics, or tier model changes — `Shared.Authorization`'s existing
  role/permission model was used exactly as-is.
- No Employee/Machine authorization wrapper — kept to what was explicitly requested and has a real
  execution path pattern already established (WorkflowEngine, Facility, Audit), matching Phase 8's
  same scope discipline.
