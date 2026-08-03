# Actor / Correlation / Causation Propagation Audit — End to End

Phase 7 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Consolidates and extends Phases 2-4's findings (`classification-matrix.md`,
`gaps/tenant-violations.md`, `gaps/missing-events.md`) with direct-read/grep verification of
every write path from the API boundary through to the outbox row.

---

## Bottom line

**ActorId is captured correctly at the API layer in PlantSafety's ~61+ `RequestEnvelopeFactory`
controllers, then silently dropped at the very last step before persistence.** Every one of
PlantSafety's 58 write-repository outbox call sites — with no exception found, including
`AccidentDapperRepository.cs`, the exact repository Phase 2 cited as proof the envelope pattern
works end to end — calls `outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(),
"system", Guid.Empty.ToString())`. The literal strings `"system"` and `Guid.Empty` are
hardcoded at the repository layer, discarding whatever `ActorId`/`CorrelationId` the command
handler actually received. This is not a partial gap concentrated in the legacy
`BaseApiController` path (as Phase 2/4 framed it) — it is a **universal, 100%-of-write-paths**
gap that also afflicts the modules held up as the "well-instrumented" example.

The interface designed to carry the real values through —
`IUnitOfWork.EnqueueEvent(IDomainEvent, string tenantId, string actorId, string
correlationId)` — exists, is correctly implemented (`UnitOfWork.cs:40-41` passes its
parameters straight through to `IOutboxWriter.Enqueue` unmodified), and **has zero callers
anywhere in the solution.** It is dead code sitting next to the code path that's actually used.

CorrelationId fares better at the API→command boundary (confirmed propagated, no gap, matching
Phase 4) but is **also discarded at the same outbox write point** — `Guid.Empty.ToString()` is
hardcoded regardless of what CorrelationId the request actually carried. So the "CorrelationId
has no gap" finding from Phase 4 needs a correction: it holds only up to the command handler;
it does not hold at the outbox/audit-trail layer, which is exactly the layer where "who did
this and how do I trace it" questions get asked later.

---

## 1. The API layer captures Actor/Correlation correctly (where the envelope pattern is used)

`PlantSafety.Api\Context\RequestEnvelopeFactory.cs` builds two envelope records:

```csharp
public sealed record CommandEnvelope(
    Guid CommandId, Guid CorrelationId, Guid? CausationId, DateTime RequestedAtUtc,
    string ApiVersionToken, Guid TenantId, Guid ActorId);

public sealed record QueryEnvelope(
    string TraceId, string SpanId, string ApiVersionToken, Guid RequestingUserId,
    string RequestingUserRole, Guid TenantId, Guid CorrelationId,
    CachePreference CachePreference, bool SuppressUnauthorized, bool StrictMode);
```

- `ActorId` (command path) comes from `currentUserService.ActorId` — a real authenticated-user
  source, not a placeholder (`RequestEnvelopeFactory.cs:54`).
- `CorrelationId` comes from `operationContext.CorrelationId` →
  `HttpContext.Items[CorrelationIdMiddleware.ItemsKey]`, populated by
  `CorrelationIdMiddleware` early in the pipeline (`OperationContextAccessor.cs:47-52`) — a
  real per-request identifier, not `Guid.Empty`.
- `CausationId` is deliberately `null` for root API-initiated operations
  (`OperationContextAccessor.cs:59`, matching the documented "Canonical Lineage Rule" —
  root commands don't inherit a causation id from nothing, they generate one only when they
  themselves cause a child operation). This is a documented design choice, not a gap.

This confirms Phase 2's finding that the ~61+ `RequestEnvelopeFactory`-pattern controllers are
genuinely actor- and correlation-aware at the point the command object is constructed. The
57-controller `RequireAuthenticatedUser` population identified in `rls-audit.md` §1d is drawn
entirely from this pattern (`env.TenantId`/`env.ActorId` usage confirmed by direct read of
`AccidentCategoriesV1Controller.cs`, `DashboardsV1Controller.cs`, `WidgetsV1Controller.cs`,
`ChecklistTemplatesV1Controller.cs`).

## 2. Every write repository discards Actor/Correlation at the very last step

`PlantSafety.Infrastructure\Persistence\UnitOfWork\OutboxWriter.cs:32` defines the real
signature:

```csharp
public void Enqueue(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId)
```

`TenantId` is genuinely threaded through correctly (`aggregate.TenantId.ToString()`, confirmed
per-repository). `ActorId` and `CorrelationId` are not. Grepped every call site of
`outboxWriter.Enqueue(` across the solution: **59 total**, of which **58** are:

```csharp
outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
```

verbatim, including (spot-checked in full):

- `PlantSafety.Infrastructure\AccidentManagement\...\AccidentDapperRepository.cs:124` — the
  same repository behind `AccidentManagementV1Controller`, Phase 2's cited example of the
  envelope pattern (`CloseAccidentInvestigationCommand(..., env.TenantId, accidentId,
  env.ActorId)`) working end to end. **It does not** — `env.ActorId` is captured by the
  controller, threaded into the command, and then discarded here.
- `ContractorSafety\ActiveMonitoringAuditing\Persistence\ActiveMonitoringAuditingDapperRepository.cs:126`
- `ContractorSafety\IncidentTrackingPerformanceEvaluation\Persistence\IncidentTrackingPerformanceEvaluationDapperRepository.cs:126`
- `ContractorSafety\PermitToWork\Persistence\PermitToWorkDapperRepository.cs:128`
- `ContractorSafety\PreQualificationVetting\Persistence\PreQualificationVettingDapperRepository.cs:130`
- `ContractorSafety\SiteOrientation\Persistence\SiteOrientationDapperRepository.cs:124`
- ...and 53 more, identical shape (grep-confirmed, not individually re-read — the pattern is
  character-for-character identical across all 58, making a per-file read unnecessary beyond
  the spot checks above).

The 59th call site, `PlantSafety.Infrastructure\Persistence\UnitOfWork\UnitOfWork.cs:41`, is
the *correct*, pass-through implementation:

```csharp
public void EnqueueEvent(IDomainEvent domainEvent, string tenantId, string actorId, string correlationId)
    => _outboxWriter.Enqueue(domainEvent, tenantId, actorId, correlationId);
```

This method (`IUnitOfWork.EnqueueEvent`, declared `IUnitOfWork.cs:28`) is the one designed to
carry real values through. **Grepped every call site of `EnqueueEvent(` in the solution: two
hits — the interface declaration and this one implementation. Zero callers.** No command
handler, repository, or pipeline behavior anywhere invokes it. It is fully-built, correctly-
wired, dead code sitting beside the path that's actually used.

### Why this matters more than Phase 4's framing suggested

Phase 4's gap analysis (`tenant-violations.md` §2-3) framed the actor-capture gap as concentrated
in two places: the legacy `BaseApiController` path (2 controllers, no mutating action, low risk)
and the 22 stub broker consumers dropping `evt.Actor` on arrival. Both of those findings still
hold (reconfirmed, no changes found this phase) — but this phase's direct read of the *write*
side shows the gap is **much larger and unconditional**: it doesn't matter whether a controller
uses the envelope pattern correctly or not, because the repository layer downstream of *every*
command handler throws the real `ActorId` away before it reaches the `OutboxMessages` table.
The 61-controller envelope pattern Phase 2 held up as the actor-aware majority is actor-aware
only up to the command object; it is not actor-aware end-to-end.

## 3. Consequence for the domain event contracts and the eventual audit trail

Combined with Phase 4's finding that `CultureCreatedEvent` (and its ~25 siblings, per
`missing-events.md`'s aggregate grep) carries an `Actor` field at the **domain event** level —
this phase's finding means there are now **two independent places** actor identity gets lost on
the way from "user clicked a button" to "row in the outbox table":

1. The domain event object (e.g. `CultureCreatedEvent`) does carry `Actor` — populated
   correctly from the aggregate/command at the point the event is raised (Phase 2, confirmed).
2. But the **outbox row** — the actual `OutboxMessages` record that any future consumer,
   retry/dead-letter tool, or audit query would read — carries `ActorId = "system"` regardless,
   because `OutboxWriter.Enqueue` is called with the hardcoded literal, not
   `domainEvent.Actor` or anything derived from it.

So even if a future engineer "fixes" the 22 stub `*CreatedConsumer` classes to actually read
`evt.Actor` (Phase 4's landmine #3), the fix is incomplete unless the outbox-write call sites
are also fixed — the event object has the right value in memory, but the persisted envelope
that survives past the initial request does not.

## 4. CorrelationId — corrected finding: propagated to the command, dropped at the outbox

Phase 4 concluded "CorrelationId... is present and threaded through consistently... the one
cross-cutting identifier in the whole solution that does not show a gap." That holds strictly
at the **API → command handler** boundary (§1, confirmed again this phase). It does **not**
hold at the **outbox** boundary: the same 58 `Enqueue(...)` call sites that hardcode
`ActorId = "system"` also hardcode `correlationId = Guid.Empty.ToString()`, discarding
whatever real `CorrelationId` the request's `CommandEnvelope.CorrelationId` carried. This
phase amends Phase 4's finding: **CorrelationId propagation has the identical, universal gap
as ActorId propagation, at the identical location (the 58 write-repository `Enqueue` calls),
via the identical dead-code fix (`IUnitOfWork.EnqueueEvent`) that would resolve both at once**
since both parameters flow through the same unused method.

## 5. Module-by-module summary (extends Phase 2/4, no material change found for these modules)

| Module | ActorId end-to-end? | CorrelationId end-to-end? | Notes |
|---|---|---|---|
| PlantSafety API → command | Yes (61+ envelope controllers) | Yes | Confirmed again this phase (§1) |
| PlantSafety command → outbox row | **No — universal, 58/58 repos hardcode `"system"`** | **No — universal, same 58/58 hardcode `Guid.Empty`** | **New, this phase** — corrects the "no gap" framing for CorrelationId and sharpens the ActorId gap from "legacy path only" to "universal" |
| PlantSafety outbox → 22 stub consumers | N/A — consumers never read `evt.Actor` even when present (Phase 4, reconfirmed no change) | N/A — consumers are no-ops | Moot until the 22 stubs are implemented (Phase 4) |
| DataLake API → outbox | ActorId never captured as a first-class outbox column (Phase 2) | TenantId is (Phase 2) | No change found this phase |
| Intelligence API → outbox | Neither TenantId nor ActorId are first-class outbox columns (Phase 2) | Same | No change found this phase; `RequestingUserId` is required at the API layer but not preserved past it |
| Azure API → event handler | No ActorId anywhere in the module (Phase 2) | CorrelationId only | No change found this phase |
| Reporting | No ActorId, no CorrelationId captured meaningfully (no auth at all — see `rls-audit.md` §3) | — | No change found this phase |
| ExportImport `StartImportCommand` | No ActorId field on the command itself | Has CorrelationId/CausationId fields (unused, module unreachable) | No change found this phase (module still orphaned per `orphaned-ingress.md`) |

---

## Summary — severity-ranked, this phase's findings only

| # | Finding | Severity | Confidence |
|---|---|---|---|
| 1 | Every PlantSafety write-repository outbox call (58/58, universal, no exceptions found) hardcodes `ActorId = "system"` and `CorrelationId = Guid.Empty`, discarding values the command handler actually received | **High** — audit-trail integrity is broken for literally every mutating action in PlantSafety's primary module, not a corner case | High — direct grep of all 59 call sites + full read of 2 representative repositories + full read of `OutboxWriter`/`UnitOfWork` |
| 2 | The correctly-designed fix (`IUnitOfWork.EnqueueEvent`, which threads real `actorId`/`correlationId` through) exists, compiles, is registered, and has **zero callers** | **High** (as a missed, ready-made fix — closing this gap requires only rewiring call sites to an already-existing method, not new design work) | High — direct grep, 2 hits total (declaration + impl) |
| 3 | Phase 4's "CorrelationId shows no gap" finding is corrected: the gap exists at the outbox layer, identical in shape and location to the ActorId gap | Medium (corrects a prior phase's conclusion; same root cause and same fix as #1) | High |
| 4 | `AccidentDapperRepository` — Phase 2's own cited example of the envelope pattern "working" end to end — is itself affected; the envelope pattern is not a mitigating factor for this specific gap anywhere in the module | High (undermines confidence in the module's most-cited good example) | High — direct read |

## Recommended fix

Route all 58 write-repository `outboxWriter.Enqueue(...)` call sites through the existing
`IUnitOfWork.EnqueueEvent(domainEvent, tenantId, actorId, correlationId)` method instead of
calling `IOutboxWriter.Enqueue` directly with hardcoded literals, threading the command
handler's actual `ActorId`/`CorrelationId` (already available via the `CommandEnvelope` at the
API boundary — see §1) through to the repository call. This is a mechanical, low-risk fix: the
correct plumbing already exists and compiles; it is simply unused.
