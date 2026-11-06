# Phase 10 — Authentication Layer Modernization

## Scope corrections made before writing any code

1. **Diagnostics/Routing/Scheduling reintroduced (4th consecutive occurrence).** Dropped again —
   confirmed still nonexistent anywhere in this program's real code. No `DiagnosticsIdentity.cs`,
   `RoutingIdentity.cs`, or `SchedulingIdentity.cs` was written. `WorkflowEngineIdentity.cs`,
   `FacilityIdentity.cs`, and `AuditIdentity.cs` were also not written as separate files — see
   "What was built" below for why that's not a gap.
2. **`CallerContext.Permissions` / a `PermissionSet` type does not exist and was not added.** The
   real `CallerContext` (Phase 9, already merged into `main`/`master`) is
   `CallerContext(EnterpriseRole Role, MfaStatus MfaStatus, string CorrelationId)` — no `Permissions`
   field. Permissions are resolved on-demand per module via the existing
   `IPermissionResolver.Resolve(role, module)`, not stored statically on the caller. Adding a
   `Permissions` field would invent a redundant type nothing in `Shared.Authorization` has, and would
   break every Phase 9 call site already merged (`WorkflowEngineAuthorization`, `FacilityAuthorization`,
   `AuthorizedAuditLogRepository`). The existing shape is unchanged.
3. **No `SafetyRoles.System` role / no "Permissions default to All" concept.** There are exactly four
   canonical roles (Viewer/Editor/Approver/PlatformAdmin — confirmed in Phase 9). "CallerContext
   defaults to System role, permissions default to All" is satisfied by reusing the existing
   `CallerContext.SystemDefault()` factory (`EnterpriseRole.PlatformAdmin`, which
   `EnterprisePermissionResolver` already grants every verb), not by inventing a new role.

## What "authentication" means here

`PlantSafety.UI.Api` has no external identity provider, no OAuth/OIDC/JWT, no cookies or sessions —
same standing fact as Phase 9. This phase does not change that. What it adds is a **deterministic,
stateless mapping from an optional request header to a `CallerContext`**, so that:

- An operator *can* register API keys mapped to specific `EnterpriseRole`s, and requests presenting
  a registered key are authorized as that role.
- A request with **no** key, or an **unrecognized** key, behaves exactly as every request did before
  this phase: `CallerContext.SystemDefault()` (`PlatformAdmin`). No endpoint breaks. No 401/403 is
  ever returned by this layer — authorization enforcement continues to happen only inside the domain
  (Phase 9's `WorkflowEngineAuthorization` and friends), unchanged.

This is intentionally *not* a login system. There's no credential issuance, no expiry, no revocation
flow — a registered key is a static, deterministic role assignment, config-bound, nothing more.

## What was built

**`Shared.Authorization.Abstractions`**
- `Identity/ApiKeyValidator.cs` — pure `Resolve(apiKey, registeredKeys) -> EnterpriseRole?` lookup.
- `Identity/ApiKeyIdentity.cs` — the deterministic outcome record (`NoKeyProvided`/`UnrecognizedKey`/
  `Recognized` factories).
- `Identity/ApiKeyAuthenticationOptions.cs` — config-bound `Dictionary<string,string>` (raw key ->
  role name), with `ToRoleMap()` resolving role names via `EnterpriseRole.FromClaimValue` and
  silently dropping unrecognized role names (an operator config typo must not crash the host).
- `Caller/ICallerContextProvider.cs` / `CallerContextProvider.cs` — request-scoped, Set-once-then-read
  carrier for the resolved `CallerContext`, structurally identical to `ICorrelationContext`/
  `CorrelationContext` (Phase 2.4). This is the module-level "identity wiring" the prompt asked for:
  every existing consumer of a `CallerContext` (`WorkflowEngineAuthorization`, `FacilityAuthorization`,
  `AuthorizedAuditLogRepository`) already takes a `CallerContext` as a parameter — Phase 9 built that
  correctly — so there is nothing module-specific to add. What was missing was *one shared way to
  produce that `CallerContext` from a request*, which is what this provider is. Separate
  `WorkflowEngineIdentity.cs`/`FacilityIdentity.cs`/`AuditIdentity.cs` files would just be the same
  three lines duplicated three times with no behavioral difference.
- `Caller/CallerContextResolution.cs` / `CallerContextFactory.cs` — pure composition of the above into
  a `CallerContext`, falling back to `SystemDefault` on missing/unrecognized keys.

**`Shared.Authorization.Infrastructure`**
- `Telemetry/AuthenticationTelemetry.cs` — reports a `CallerContextResolution` via the existing
  `ILogEventWriter`/`PlantSafetyMetrics` (one new counter, `ApiKeyResolutionsPerformed`, tagged by
  outcome; same `Meter` instance as everything else).
- `Telemetry/AuthenticationTracing.cs` / `AuthenticationCorrelation.cs` — thin named wrappers (a span-
  name constant, a correlation-id accessor) over the same log-event/correlation plumbing above. Not
  new tracing or correlation primitives — this program has no distributed-tracing collector
  configured, same standing fact as Phase 8.
- `Governance/AuthenticationGovernanceRules.cs` / `AuthenticationGovernanceEvaluator.cs` — pure
  validation of a `CallerContextResolution`'s shape, reported through the same
  `GovernanceViolationsDetected` counter and `ILogEventWriter` pattern `AuthorizationGovernanceEvaluator`
  (Phase 9) and `TelemetryGovernanceEvaluator` (Phase 8) already established — not a third, parallel
  governance mechanism.

**`Shared.Telemetry.Abstractions`**
- `LogCategory.Authentication` added (mirrors how `Authorization` was added in Phase 9).
- `LogCategoryClassifier` extended for the `"authentication"` module name.
- `PlantSafetyMetrics.ApiKeyResolutionsPerformed` counter added.

**`PlantSafety.UI.Api`**
- `Middleware/ApiKeyAuthenticationMiddleware.cs` — reads the `X-Api-Key` header, resolves it via
  `CallerContextFactory`, sets it on the request-scoped `CallerContextProvider`, and emits telemetry +
  governance signals. Registered in `Program.cs` immediately after `CorrelationIdMiddleware` (so a
  real correlation id already exists) and before routing.
- `Program.cs` — binds `ApiKeyAuthenticationOptions` from configuration
  (`ApiKeyAuthentication:RegisteredKeys`, defaulting to empty), registers the resolved role map,
  `CallerContextProvider`/`ICallerContextProvider` (scoped), and the new middleware.
- `Controllers/WorkflowEngineController.cs` — updated to read `CallerContext` from
  `ICallerContextProvider.Current` instead of constructing `CallerContext.SystemDefault(...)` inline.
  No other behavioral change.

## What was deliberately not built

- No OAuth/OIDC/JWT/cookies/sessions/IdentityServer/external identity provider — explicitly
  prohibited by the prompt and unnecessary for a deterministic, config-bound role mapping.
- No MFA flow. `MfaStatus` is always `NotApplicable` for API-key-resolved callers — a matching key is
  not evidence of a second factor. The prompt explicitly excludes MFA flows at the API layer.
- No new roles, permissions, or verbs. `ApiKeyAuthenticationOptions.ToRoleMap()` only ever resolves
  to one of the four existing `EnterpriseRole` values via `EnterpriseRole.FromClaimValue`.
- No `[Authorize]` attribute or `app.UseAuthorization()` reliance — same reasoning as Phase 9: there
  is still no ASP.NET Core authentication *scheme* registered, and this middleware does not add one.
  It populates a `CallerContext` for the domain layer to check; it does not gate the pipeline itself.
- No key issuance, rotation, expiry, or revocation flow. `ApiKeyAuthenticationOptions` is a static,
  operator-managed configuration map — key lifecycle management was not in scope.

## Verification performed

- `Shared.Authorization.Abstractions`, `Shared.Authorization.Infrastructure`, and
  `PlantSafety.UI.Api` all build cleanly (0 errors).
- `Shared.Authorization.Tests` — 47 tests pass, including the new `Identity`/`Caller`/`Governance`/
  `Telemetry` coverage for this phase, alongside all pre-existing Phase 9 tests (unaffected).
- Live-verified via `dotnet run` + `curl` against the one real endpoint,
  `POST /api/v1/ui/engines/workflow/evaluate`:
  - No `X-Api-Key` header: `200 OK`, identical response to before this phase (`CallerContext.
    SystemDefault`, `PlatformAdmin`).
  - Unrecognized `X-Api-Key` value: `200 OK`, identical response (falls back to `SystemDefault`,
    logs a `Warning`-severity `API_KEY_RESOLVED` event, no governance violation — the resolution
    itself is well-formed even though the key wasn't recognized).
  - A key registered via `ApiKeyAuthentication:RegisteredKeys` config (mapped to `Safety.Editor`):
    `200 OK`, `API_KEY_RESOLVED` logs `Outcome=Recognized`, `ResolvedRole=Safety.Editor` — the
    resolved role is correctly threaded through to the domain authorization check.
