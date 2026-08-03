# Gap Analysis — Tenant / Actor / Correlation Scoping Violations

Phase 4 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `classification-matrix.md` (Phase 2, primary source for this axis), `module-flow/*.md`
(Phase 3), plus new spot-verification this phase (marked **[NEW]**).

Every ingestion flow identified across all phases that fails to properly scope or propagate
TenantId, ActorId, or CorrelationId is listed below, with concrete file:line and an explicit
exploitability assessment — not just "this is untidy," but "here is what a malicious or careless
caller could actually do with it," where that can be said with the evidence in hand.

---

## 1. Client-supplied TenantId on read endpoints (PlantSafety) — the core finding, now further verified

**Flow**: `GET api/v{version}/analytics/culture/behavior` → `BehaviorV1Controller.GetAsync`
(`PlantSafety.Api\Analytics\Culture\Behavior\Controllers\V1\BehaviorV1Controller.cs:55-63`) builds
`new GetBehaviorQuery(request.TenantId, request.SiteId, ...)` **directly from the query string**
(`[FromQuery] GetBehaviorRequest request`), not from an authenticated tenant-context claim. This
pattern is shared by the ~297 direct-`: ControllerBase` read controllers (Culture, Dashboard,
Engagement, PredictiveAnalytics, Predictive sub-areas, ×V1/V2) per Phase 2's classification.

Phase 2 flagged this as "not confirmed exploitable... would require tracing every
`[Authorize(Policy=...)]` definition, out of scope." **This phase did that tracing.** Result:

### 1a. **[NEW]** The authorization policy meant to gate this endpoint is never registered — confirmed by direct read

- `BehaviorV1Controller` is decorated `[Authorize(Policy = BehaviorAuthorizationPolicies.Read)]`,
  where `BehaviorAuthorizationPolicies.Read = "RequireAnalyticsCultureBehaviorRead"`
  (`PlantSafety.Api\Analytics\Culture\Behavior\Authorization\BehaviorAuthorizationPolicies.cs:27`).
- The only place ASP.NET Core authorization policies are registered for PlantSafety.Api is
  `AddPlantSafetyAuthorization()` (`PlantSafety.Api\Extensions\ServiceCollectionExtensions.cs:117-135`),
  which registers exactly two named policies: `RequireAuthenticatedUser` and `RequireDashboardRead`.
  Neither is `RequireAnalyticsCultureBehaviorRead`.
- A second registry, `AddPlantSafetyPolicies()` (`PlantSafety.Api\Extensions\AuthorizationPolicyExtensions.cs:49-101`),
  registers ~30 *module-level* policy names (`Analytics.Read`, `Compliance.Create`, etc.) — a
  **different naming convention** from the ~147 individual `*AuthorizationPolicies` classes found
  throughout the controller tree (one per feature area: `BehaviorAuthorizationPolicies`,
  `RiskForecastingAuthorizationPolicies`, `CAPAAuthorizationPolicies`, and 144 others — confirmed by
  grep, `class \w+AuthorizationPolicies` → 147 files). None of the 147 feature-level policy name
  strings match any of the ~30 registered in `AddPlantSafetyPolicies()`.
- **`AddPlantSafetyPolicies()` itself is never called anywhere** — grepped the entire solution for
  `AddPlantSafetyPolicies(`: only its own declaration, zero invocations.

**Net effect, if this holds at runtime (see caveat below):** any controller using
`[Authorize(Policy = "<Feature>AuthorizationPolicies.<X>")]` — which appears to be the majority
pattern across the 147 feature areas found — references a policy name that
`IAuthorizationPolicyProvider` cannot resolve. ASP.NET Core's default policy combination throws
`InvalidOperationException("The AuthorizationPolicy named: '...' was not found.")` when this
happens, which surfaces as an unhandled 500 at request time (fail-**closed**, not fail-open — this
is a genuine correctness/availability defect, not by itself a way to bypass authorization to read
another tenant's data).

**Caveat, stated plainly**: this conclusion is derived from static analysis (grep + full reads of
the DI registration files), not from running the application. It is possible a mechanism this pass
didn't find (a custom `IAuthorizationPolicyProvider`, a build-time source generator, an
`AuthorizationPolicy` fallback configured elsewhere) resolves these names at runtime. No such
mechanism was found — `AddAuthorization(` appears exactly once in `PlantSafety.Api`
(`ServiceCollectionExtensions.cs:119`) and no custom `IAuthorizationPolicyProvider` implementation
exists anywhere in the module (grepped, zero hits). Flagged **unverified/needs runtime
confirmation**, but the static evidence is unambiguous enough to report as a real, high-confidence
finding rather than a hunch.

### 1b. What this means for the original tenant-leak question

If 1a is confirmed at runtime (endpoints 500 before reaching the handler), then the client-supplied
`TenantId` on these 297 read controllers is **not currently exploitable as a live cross-tenant
data leak**, because the request never reaches `GetBehaviorQuery`/the repository at all — it fails
before authorization completes. This is a materially different (and, from a pure
data-confidentiality standpoint, *less severe*) risk than Phase 2 flagged, **but it trades one
severe problem for another**: a broken, 500-ing analytics surface across ~130+ endpoints is itself
a serious availability defect that would need to be caught immediately in any real environment
(unless a fallback/default policy silently permits the request — not confirmed either way this
phase). **If, on the other hand, some controllers in this set use bare `[Authorize]` with no
`Policy=` argument** (only requiring authentication, not a specific policy — this does not hit the
missing-policy code path), **then those specific endpoints* are* still exploitable exactly as Phase
2 described**: an authenticated caller for Tenant A can pass Tenant B's `TenantId` in the query
string with no server-side cross-check, and the query will execute against Tenant B's data. This
phase did not have time to individually classify all 297 read controllers by
`[Authorize]`-vs-`[Authorize(Policy=...)]`; **that classification is the single highest-value
follow-up for Phase 7's RLS/security audit** — it directly determines whether this is an
availability bug or a live tenant-isolation breach, and the answer likely differs controller by
controller.

**Exploitability verdict**: **Real risk, mechanism uncertain pending the classification above.**
Either (a) a live cross-tenant read on any controller using bare `[Authorize]` with client-supplied
TenantId — genuinely exploitable, no additional conditions needed — or (b) a broad 500-error
availability defect on any controller using the unregistered `[Authorize(Policy=...)]` pattern.
Both are High severity; they are not mutually exclusive within the same 297-controller population.

---

## 2. PlantSafety legacy `BaseApiController` path captures no actor identity

- `PlantSafety.Api\Controllers\BaseApiController.cs` exposes `TenantId`/`SiteId` helpers only — no
  `CallerUserId`/actor accessor. Its own header comment lists "Add a `CallerUserId` property
  resolved from JWT sub claim for audit trail use" as a **Future Extension Seam** (i.e., scoped and
  never built).
- Only 2 real (non-abstract, non-webhook) controllers use this legacy base:
  `PlatformV1Controller.cs`/`PlatformV2Controller.cs` — **[NEW]**, read in full this phase:
  `PlatformV1Controller.GetVersionAsync` (`PlantSafety.Api\Controllers\V1\PlatformV1Controller.cs:54-63`)
  is `[AllowAnonymous]`, read-only (returns assembly version), and writes nothing. **No
  exploitability here** — there is no lifecycle-triggering write on this legacy path to attribute
  to a missing actor. This narrows Phase 2/3's general "legacy path captures no actor identity"
  finding: in practice, the only two live controllers on that path don't perform any action an
  actor identity would need to be attached to. The concern is theoretical unless a future
  controller is added to this base class with a mutating action.

**Exploitability verdict**: **Low today** (no mutating endpoint currently uses this path), but
**flag as a landmine** — any new controller added under `BaseApiController` inherits the
no-actor-capture gap silently, with no compiler or review signal forcing the author to add one.

---

## 3. PlantSafety domain events carry Actor but drop TenantId; consumers drop Actor even when present

- `CultureCreatedEvent` (full record read, Phase 2) carries `EventId, CultureId, CreatedAtUtc,
  Actor, CorrelationId, CausationId` — **no `TenantId` field** — even though the `Culture` aggregate
  that raises it is itself tenant-scoped in its repository/SQL layer.
- All 22 `*CreatedConsumer` bodies (`CultureCreatedConsumer.cs`, `AuditingCreatedConsumer.cs`, both
  read in full) **never read `evt.Actor`** despite the event carrying it — captured upstream,
  dropped on arrival, in every one of the 22 consumers (structurally identical, scripted-check
  confirmed by Phase 2).
- **Exploitability verdict**: **Medium — audit-trail integrity risk, not a live access-control
  bypass.** If/when these consumers are implemented to actually perform their projection/side-effect
  (currently they're no-ops per `orphaned-ingress.md`), any audit trail or "who did this" reporting
  built on top of them will be unable to attribute the action to a user, and any tenant-scoping
  added to a downstream read of the resulting projection will have nothing to key off unless
  TenantId is added to the event contract first. This is a **forward-looking landmine for whoever
  implements the 22 stub consumers** — they cannot correctly complete "acknowledge the actor" or
  "scope the result to a tenant" without first fixing the event contracts.

---

## 4. Intelligence's Outbox drops both TenantId and ActorId as first-class fields

- `Intelligence.Infrastructure.Persistence.Outbox.OutboxMessage` (full read, Phase 2) has fields:
  `Id, EventType, Payload, CreatedAtUtc, ProcessedAtUtc, RetryCount, LastError` — **no `TenantId`,
  no `ActorId`** column. Whatever tenant/actor context existed at
  `IntelligenceControllerBase.TenantId`/`RequestingUserId` is only preserved (if at all) inside the
  opaque `Payload` JSON blob, invisible to any monitoring/retry/dead-letter logic operating on the
  outbox row itself.
- Compare to DataLake's `OutboxMessage`, which **does** have a first-class, correctly-populated
  `TenantId` column (Phase 2, directly verified) — the same "outbox" concept was built to two
  different standards in two different modules.
- **Exploitability verdict**: **Medium — cross-tenant data mixing risk in dead-letter/retry
  tooling, not a live read/write bypass.** An operator or dead-letter-queue tool that processes
  failed Intelligence outbox rows without parsing `Payload` cannot tell which tenant a failed
  message belongs to, which risks accidental cross-tenant reprocessing or misrouted retries if that
  tooling is ever built. Currently moot in practice only because the entire pipeline dead-ends at
  `NullOutboxPublisher` (see `orphaned-ingress.md`) — nothing is actually retried today.

---

## 5. Azure and Reporting: zero tenant concept, zero actor concept, zero authentication (Reporting only)

- **Azure (63 endpoints)**: `TenantId` appears only as an Azure-resource metadata field
  (`ManagedIdentity.TenantId`), never as an application-level multi-tenancy concept. No
  `ClaimsPrincipal`/`HttpContext.User` usage anywhere in the module — only a `CorrelationId` for
  tracing. **However**, Azure genuinely has JWT authentication wired (`AddAuthentication(...)
  .AddJwtBearer()`, confirmed in `Compute.Api`/`Data.Api` DI extensions) and `[Authorize]` on every
  controller. **Exploitability verdict**: **Low-Medium** — an authenticated caller can restart a
  Container App or purge a dead-letter topic and no code anywhere records *which tenant/organization
  boundary* that caller belonged to or *who* they were, beyond an opaque `CorrelationId`. This is a
  genuine audit-trail gap (who restarted this and on whose behalf), not a data-confidentiality
  bypass, because there's no tenant partitioning concept to bypass in the first place — Azure reads
  as a platform-operator tool, not a multi-tenant data surface.
- **Reporting (5 endpoints)**: no TenantId, no ActorId, **and no authentication wired at all**
  (`Program.cs` grepped in full, Phase 2/3 — zero `AddAuthentication`/`AddJwtBearer`/`[Authorize]`;
  `app.UseAuthorization()` is called with nothing to authorize against). **Exploitability verdict**:
  **High** — this is the starkest access-control gap in the whole solution as documented: an
  unauthenticated network caller can invoke `POST .../generate` or `.../export` and receive a
  rendered PDF of aggregate safety-report data with zero proof of identity, zero tenant scoping,
  and zero actor logging. Unlike Azure, there is no compensating JWT/`[Authorize]` layer at all
  here.
- **DataLake (52) and Intelligence (37)**: **excellent** tenant/actor plumbing at the code level
  (`DataLakeControllerBase.TenantId`/`RequestingUserId` are non-nullable, throwing accessors;
  Intelligence's `RequestingUserId` is required, `TenantId` is optional) — **but zero authentication
  middleware wired in either module** (confirmed: zero `AddAuthentication`/`AddJwtBearer`/
  `[Authorize]` anywhere in `DataLake.Api`/`Intelligence.Api`). **Exploitability verdict**: **High**
  — the tenant/actor values these controllers "require" are read from `HttpContext.Items`/claims
  that nothing populates via a trustworthy authentication step; as wired, an unauthenticated caller
  can supply any `TenantId`/`RequestingUserId` value the framework will accept (mechanism for how
  these values get into `HttpContext.Items` in the first place was not separately traced this phase
  — flagged **unverified/needs follow-up**, but the absence of any authentication layer to
  establish a trustworthy caller identity is independently confirmed and sufficient on its own to
  flag this High).

**Combined**: 94 of 879 endpoints (~11%) — DataLake + Intelligence + Reporting — have strong-looking
or entirely-absent tenant/actor scoping sitting behind literally no authentication whatsoever.

---

## 6. ExportImport's `StartImportCommand` carries no TenantId or actor/user field at all

- `StartImportCommand` (full read, Phase 2): `CommandId, CorrelationId, CausationId,
  RequestedAtUtc, ApiVersionToken, FileContent, FileFormat, DatasetName, RequiredFields, BatchSize`
  — no `TenantId`, no `ActorId`/`UserId`.
- **Exploitability verdict**: **N/A today** (module has no live caller — see
  `orphaned-ingress.md`), but this is a concrete, actionable design gap for whoever eventually wires
  this module up: the command contract itself would need tenant and actor fields added before it
  could be safely exposed, not just an HTTP endpoint bolted on top.

---

## 7. CorrelationId — the one dimension that is consistently propagated

Across every module examined (Azure, DataLake, Intelligence, PlantSafety, ExportImport's command
contracts), `CorrelationId`/`CausationId` is present and threaded through consistently — this is
the one cross-cutting identifier in the whole solution that does **not** show a gap. No violation
found in this dimension; included here only for completeness since the task asked about all three
(TenantId, ActorId, CorrelationId).

---

## Summary — exploitability-ranked

| # | Finding | Exploitability | Confidence |
|---|---|---|---|
| 1a/1b | PlantSafety client-supplied TenantId on 297 read controllers, gated by an authorization policy that appears to be unregistered | **High** (either a live cross-tenant read on bare-`[Authorize]` controllers, or a broad 500 availability defect on policy-based ones — needs Phase 7 per-controller classification to resolve which) | High (static evidence), runtime-unconfirmed |
| 5 | DataLake + Intelligence + Reporting: 94 endpoints with tenant/actor plumbing (or none at all, for Reporting) behind zero authentication middleware | **High** | High (directly verified: grep for auth wiring returned zero hits in all three) |
| 3 | PlantSafety domain events drop TenantId; consumers drop Actor even when present | **Medium** (forward-looking audit/tenant-scoping landmine for the 22 stub consumers) | High |
| 4 | Intelligence Outbox has no TenantId/ActorId columns | **Medium** (cross-tenant mixing risk in future retry/dead-letter tooling; moot today since the pipeline is fully dead-ended) | High |
| 2 | PlantSafety legacy `BaseApiController` path has no actor capture | **Low today** (no mutating endpoint uses it), landmine for future additions | High |
| 6 | ExportImport `StartImportCommand` has no TenantId/ActorId | **N/A today** (unreachable module), real gap if ever wired | High |
| 7 | CorrelationId propagation | No gap found | High |

## Single most severe issue in this document

**#1a/1b — the PlantSafety client-supplied-TenantId read surface, compounded by an apparently
unregistered authorization-policy layer covering the majority of that same surface.** This is the
one item in this phase's findings that could plausibly be a live, unauthenticated-boundary
cross-tenant data leak (not merely an architectural untidiness) if any of the 297 affected
read-controllers use bare `[Authorize]` rather than the (apparently broken)
`[Authorize(Policy=...)]` pattern — and it sits on PlantSafety's Analytics/Compliance surface,
which is the single largest concentration of endpoints in the entire solution (473 GET endpoints,
65% of PlantSafety's 722). Recommend this be the very first item Phase 7 resolves, specifically by
sampling several `[Authorize(Policy=...)]`-decorated read controllers at runtime (not just via
static grep) to determine whether requests actually 500 or actually succeed with an unchecked
client-supplied tenant.
