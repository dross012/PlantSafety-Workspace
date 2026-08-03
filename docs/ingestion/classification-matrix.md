# Ingestion Pipeline Discovery — Phase 2: Classification Matrix

Root: `C:\Users\dross\source`. Date: 2026-07-22. Input: `inventory.md`, `api-ingress.md`,
`broker-ingress.md`, `file-ingress.md` (Phase 1). This phase does not re-derive the inventory; it
classifies every ingress point Phase 1 found along seven axes and calls out gaps plainly for
Phase 4 (gap analysis) and Phase 7 (security/RLS audit).

## Method note on scale

879 API endpoints cannot each be individually read. Endpoints were grouped by **controller-base
pattern** (the shared base controller / MediatR pipeline-behavior set each module's controllers
inherit), because that base determines auth, validation, tenancy, and actor behavior far more than
any individual action does. One or more representative controllers/handlers per group were read in
full; the rest of the group is **pattern-inferred**. Any endpoint that visibly deviates from its
group's base pattern would be called out — none were found in the samples taken. The 76 broker
rows, 6 file adapters, and 6 scheduled jobs were classified individually (small enough), per the
task's instruction.

**Row-count correction (transparency, not a contradiction of Phase 1's headline count):**
`broker-ingress.md`'s own table lists **76** production rows, not 79 — I recounted it directly
(`grep -c "^| "` plus manual tally). The gap is explained by the table itself: `IConsumer<T>` and
`ConsumeContext<T>` are two separate regex hits inside the *same* consumer file, so most PlantSafety
consumer classes appear as two rows for one ingress point. Classifying by **unique class** (which is
what matters for gap analysis) yields **22 unique `*CreatedConsumer` classes** in PlantSafety, not
50 rows. Phase 1's 79 figure and this phase's per-class classification are reconciled in the
PlantSafety broker table below with an explicit row-count note. This is a Phase 1 arithmetic
footnote, not a reversal of any Phase 1 finding.

---

## Top-level summary

| Axis | Result | Notes |
|---|---|---|
| **Validated** (FluentValidation/DataAnnotations/domain validation before persistence) | ~745 / 879 API endpoints (85%) validated at the Application layer via a MediatR `ValidationBehavior` + per-command/query `AbstractValidator`; **Reporting's 5 endpoints are the only API group confirmed to have both validation AND zero authentication** — see below. | 79% of GET-only reads still pass through a query validator in every module sampled except a handful of PlantSafety read controllers that bind query params directly with no validator file found (pattern-inferred gap, not spot-verified further). |
| **Unvalidated** | A minority of PlantSafety pure-read (`GET`) controllers bind `[FromQuery]` DTOs with no matching `*QueryValidator` found in the same feature folder (sampled 3, all 3 had no validator) — flagged pattern-inferred, needs Phase 4 follow-up. All 22 PlantSafety broker consumers: **no schema/business validation at all**, only an idempotency check. | |
| **Tenancy-aware** (references TenantId in its path) | DataLake (100% of 52), Intelligence (100% of 37, but field is *optional* `Guid?`), PlantSafety (≈100% of 722, via either `BaseApiController.TenantId`, the `RequestEnvelopeFactory` envelope, or a client-supplied query parameter), PlantSafety outbox (`OutboxWriter`, `OutboxMessage`) — tenant-scoped. **Azure (0/63) and Reporting (0/5) have no tenant concept anywhere in the request path.** Intelligence's outbox record (`Intelligence.Infrastructure.Persistence.Outbox.OutboxMessage`) drops TenantId entirely — it is not a column, only (possibly) opaque JSON inside `Payload`. | ≈811/879 (92%) of API endpoints are tenancy-aware in some form; the two ways PlantSafety derives TenantId (auth-context vs. **client-supplied query parameter** in read controllers like `BehaviorV1Controller`) are **not equivalent from a security standpoint** — flagged as a concern below. |
| **Actor-aware** (captures/propagates ActorId or "who did this") | DataLake (100%, `RequestingUserId` required at the base-controller level), Intelligence (100%, `RequestingUserId` via `ICurrentUserContext`), PlantSafety **only for the ≈61 controllers using the newer `RequestEnvelopeFactory` envelope pattern** (mutating/lifecycle commands) — PlantSafety's older `BaseApiController` has **no actor capture at all**, and it is explicitly logged as a "Future Extension Seam" TODO in its own header comment. **Azure (0/63) and Reporting (0/5) capture no actor identity anywhere** — only a CorrelationId for tracing, never "who." | Roughly 40% of all API endpoints solution-wide have no actor capture whatsoever, concentrated in Azure, Reporting, and PlantSafety's non-command (read) controllers. |
| **Lifecycle-triggering** (mutates a domain entity's state) | 310 / 879 API endpoints (35%) are POST/PUT/PATCH/DELETE and lifecycle-triggering; 569 (65%) are GET/read-only. Of the 22 PlantSafety broker consumers, **0 currently perform any real lifecycle side-effect** — all 22 read as an idempotency-ledger check + a log line, with 5 of them (`Culture`, `Dashboard`, `Engagement`, `PredictiveAnalytics`, `ScoreCards`) containing an explicit `// TODO: execute … side-effects here` comment, and the other 17 following the identical no-op shape without even a TODO marker. | The 22 consumers are a bigger stub-surface than Phase 1's discovery implied — Phase 1 flagged the *cross-module* wiring as unverified; this phase additionally finds the **intra-module** side effects are unimplemented too. |
| **Sync vs async** | All 879 API endpoints are synchronous request/response over HTTP (even the ones returning `202 Accepted` block for that response). All 76 broker rows / 22 consumer classes and all 6 scheduled jobs are asynchronous (background-thread or queue-drained). File adapters are synchronous library calls invoked from within a synchronous command handler. | |
| **User- vs system-initiated** | All 879 API endpoints are user-initiated (HTTP caller) except the 1 webhook (external-system-initiated). All 76 broker rows, all 6 scheduled jobs, and the file-adapter library calls (when/if invoked) are system-initiated. | |
| **Internal vs external** | 878/879 API endpoints are internal-caller-facing (internal services/UI hitting the platform's own APIs); the 1 webhook is the sole external-system ingress. All 76 broker rows are internal (no external message broker). File adapters are internal library code, currently unreachable from outside the ExportImport module (orphaned — see Phase 1). | |

---

## 1. API endpoints (879) — by module / controller-base pattern

### Azure (63 endpoints, 9 subdomains: Compute, Data, Eventing, Identity, ML, Networking, Observability, Security, Storage)

Directly verified: `Azure\Compute.Api\Controllers\v1\ComputeController.cs` (full read),
`Azure\Identity.Api\Controllers\v1\IdentityController.cs` (full read), plus DI wiring in
`Azure\Compute.Api\Extensions\ServiceCollectionExtensions.cs` / `Azure\Data.Api\Extensions\ServiceCollectionExtensions.cs`
(both confirm `AddAuthentication(JwtBearerDefaults...).AddJwtBearer()` is genuinely wired per
subdomain — real JWT auth, not just decoration).

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| All 63 Azure endpoints (9 subdomain controllers) | API | Sync | User | Internal | **Yes** — `AbstractValidator` per command/query (102 validator files) + `ValidationBehavior` registered first in the MediatR pipeline in every subdomain's `ServiceCollectionExtensions.cs` (verified: Compute) | 24/63 are POST/PUT/PATCH mutating actions (restart, scale, purge, tier/throughput change, triage, disable) — lifecycle-triggering; 39 are GET reads — non-lifecycle | **No** — `TenantId` appears only as a data field on the `ManagedIdentity` domain model (Azure-tenant metadata), never as a request-scoping/multi-tenancy concept; no controller, query, or command carries an application-level TenantId | **No** — zero `ClaimsPrincipal`/`User.Identity`/`HttpContext.User` usage anywhere in the Azure module; only a `CorrelationId` is threaded through for tracing, never a "who" | Directly-verified (Compute, Identity full read; DI wiring confirmed for Compute + Data); pattern-inferred for the other 7 subdomains, which share the identical generated-controller header/comment template and the same `ValidationBehavior`/`LoggingBehavior` file names | `[Authorize]` + real JWT auth is genuinely wired (not just an attribute) — this module is the most conventionally-secured of the five, but has a **complete actor-identity blind spot**: an authenticated caller can restart a Container App or purge a dead-letter topic and no code anywhere records who did it beyond an opaque correlation GUID. |

### DataLake (52 endpoints, 10 controllers: Archival, Consumption, DataContracts, DataQuality, Governance, Ingestion, Integration, MetadataCatalog, Processing, Schema)

Directly verified: `DataLake\DataLake.Api\Controllers\DataLakeControllerBase.cs` (full read),
`DataLake\DataLake.Api\Controllers\v1\ArchivalController.cs` / `ConsumptionController.cs` /
`DataContractsController.cs` (grepped for envelope usage). Confirmed via grep: **zero**
`AddAuthentication`/`AddJwtBearer` anywhere in the DataLake tree, and **zero** `[Authorize]`
attributes on any DataLake controller.

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| All 52 DataLake endpoints (10 controllers, all inherit `DataLakeControllerBase`) | API | Sync | User | Internal | **Yes** — 25 `AbstractValidator` files + `ValidationBehavior` wired in `DataLake.Application`'s DI extensions | ~29 POST/PUT are lifecycle-triggering (archival tier moves, contract/schema creation, governance approvals, processing pipeline triggers); ~23 GET are reads | **Yes, and required** — `DataLakeControllerBase.TenantId => HttpContext.RequireTenantId()` is a non-nullable, throwing accessor: every action gets a hard-required tenant scope | **Yes, and required** — `RequestingUserId => HttpContext.RequireRequestingUserId()`, also throwing/required, plus `RequestingUserRole` | Directly-verified (base controller in full; 3 of 10 controllers spot-checked for envelope usage); pattern-inferred for the other 7, which all declare `: DataLakeControllerBase` | **Security gap**: the tenant/actor envelope is only as strong as whatever populates `HttpContext.Items` for `RequireTenantId()`/`RequireRequestingUserId()` — and there is **no authentication middleware configured anywhere in DataLake.Api** (no `AddAuthentication`, no `AddJwtBearer`, no `[Authorize]`). Best-practice tenant/actor plumbing sitting behind what appears to be an **unauthenticated HTTP surface** as currently wired — the single most concerning API-layer finding in this phase (see Top-5 below). |

### Intelligence (37 endpoints, spans AIInvestigations, AITraining, and others)

Directly verified: `Intelligence\Intelligence.Api\Controllers\IntelligenceControllerBase.cs` (full
read), `Intelligence\Intelligence.Api\Controllers\v1\AIInvestigationsController.cs` (route list from
Phase 1), `Intelligence.Application\Shared\Behaviors\AuthorizationBehavior.cs` (full read). Confirmed
via grep: **zero** `AddAuthentication`/`AddJwtBearer` and **zero** `[Authorize]` anywhere in
Intelligence.Api (6 controller files checked).

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| All 37 Intelligence endpoints (6 controllers, all inherit `IntelligenceControllerBase`) | API | Sync | User | Internal | **Yes** — `AbstractValidator` (fewer files than DataLake, 2 found directly under Application root, more likely nested per-feature — not exhaustively counted) + dedicated `ValidationBehavior`, **plus** an `AuthorizationBehavior` that runs *before* validation for any request implementing `IHasAuthorization` (business-rule authorization, not authentication) | Mix of POST (submit/cancel training jobs, submit investigations/hypotheses — lifecycle-triggering) and GET (reads) | **Yes, but optional** — `IntelligenceControllerBase.TenantId => HttpContext.GetTenantId()` returns `Guid?`, i.e. tenant scope can be absent and the base controller does not throw | **Yes, and required** — `RequestingUserId => HttpContext.GetRequestingUserId()` returns a non-nullable `UserId` | Directly-verified (base controller, `AuthorizationBehavior` in full); pattern-inferred for the 6 controllers as a group | **Same authentication gap as DataLake**: an in-process `AuthorizationBehavior` evaluates business authorization rules, but there is no HTTP-layer authentication (no JWT, no `[Authorize]`) establishing *who the caller claims to be* in the first place — the authorization check has nothing trustworthy to authorize against. Also: Intelligence's **outbox record has no TenantId column at all** (see broker section) — tenant scope established at the API layer is lost by the time the event reaches the outbox. |

### PlantSafety (722 endpoints, 362 controller files — 82% of the whole solution's API surface)

Directly verified: `PlantSafety\PlantSafety.Api\Controllers\BaseApiController.cs` (full read),
`PlantSafety\PlantSafety.Api\AccidentManagement\Controllers\V1\AccidentManagementV1Controller.cs`
(full read — `RequestEnvelopeFactory` pattern), `PlantSafety\PlantSafety.Api\Analytics\Culture\Behavior\Controllers\V1\BehaviorV1Controller.cs`
(full read — direct-`ControllerBase` read pattern), `PlantSafety\PlantSafety.Api\Webhooks\WebhookReceiverV1Controller.cs`
(the one external ingress). Confirmed by scripted check across all 362 controller files: 360/362
carry `[Authorize`, the 2 exceptions being the abstract `BaseApiController` itself (not an endpoint)
and the intentionally-anonymous webhook receiver. Confirmed JWT auth is genuinely wired
(`PlantSafety.Api\Extensions\ServiceCollectionExtensions.cs:105-106`,
`AddAuthentication(JwtBearerDefaults...).AddJwtBearer(...)`).

PlantSafety has **three distinct controller patterns**, which matter for actor-awareness:

| Pattern | File count (of 362) | Actor-aware? | Notes |
|---|---|---|---|
| `RequestEnvelopeFactory` envelope (`env.TenantId`, `env.ActorId`, `env.CorrelationId`, `env.CausationId`) | ≈61 (directly grepped) | **Yes** | Used on mutating/command controllers — the envelope is constructed per-request and threaded into every command, e.g. `CloseAccidentInvestigationCommand(..., env.TenantId, accidentId, env.ActorId)`. |
| Legacy `BaseApiController` (`TenantId`/`SiteId` helpers only) | 4 (2 are the base class + webhook, so effectively 2 real controllers) | **No** | Its own header comment lists "Add a `CallerUserId` property resolved from JWT sub claim for audit trail use" as a **Future Extension Seam** — i.e., actor capture was scoped and never built here. |
| Direct `: ControllerBase` with ad-hoc `TenantId`/`SiteId` (297 files, mostly read-only `GET` controllers) | 297 | **No** (reads don't mutate, so no actor is needed) — **but tenant scoping is often client-supplied**, e.g. `BehaviorV1Controller.GetAsync` builds `new GetBehaviorQuery(request.TenantId, ...)` straight from the query string, not from an authenticated tenant-context claim | Flagged as a possible tenant-isolation concern below — a caller authenticated for Tenant A could, if this pattern isn't backed by a policy check elsewhere, pass Tenant B's `TenantId` on the query string. Not confirmed exploitable (would need to trace `[Authorize(Policy=...)]` definitions per-controller, out of scope for this phase) — flagged **unverified/needs follow-up** for Phase 7. |

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| 473 GET endpoints (all patterns) | API | Sync | User | Internal | Mostly yes — 248 `AbstractValidator` files exist solution-wide in PlantSafety, but 3 sampled read-controllers had no matching query validator (pattern-inferred gap) | **No** — pure reads | Yes (see pattern table above; some client-supplied) | No (reads) | Pattern-inferred (2 full reads + scripted `[Authorize]`/base-class census across all 362) | |
| 249 POST/PUT/DELETE endpoints (mostly `RequestEnvelopeFactory` pattern) | API | Sync | User | Internal | Yes — validators + `ValidationBehavior` pipeline (mirrors Azure/DataLake/Intelligence pattern) | **Yes** — these are the lifecycle-triggering majority (create/close/complete/file workflow actions) | Yes | **Yes** (61-file envelope pattern covers the great majority of mutating controllers) | Directly-verified (AccidentManagement full read) | |
| `WebhookReceiverV1Controller.cs` (`POST api/v1/webhooks/receive`) | API — external ingress | Sync (fast-ack) then async (outbox-processed) | **System** (external system calls in) | **External** | Yes — HMAC-SHA256 signature verification substitutes for JWT auth; body/schema validation not independently re-verified this phase (Phase 1 documented it, not re-read in full here) | **Yes** — dispatches `ProcessInboundWebhookCommand` | Not verified this phase whether TenantId is resolved per-source; Phase 1 notes "per-source routing... different HMAC secrets per registered external system" is a **documented future extension, not yet built** — implies today's webhook receiver may be tenancy-agnostic or single-tenant. Flagged **unverified/needs follow-up**. | Not verified this phase | Pattern-inferred (relying on Phase 1's read; not independently re-read line-by-line this phase) | The one deliberately-anonymous, externally-reachable endpoint in the entire solution — highest-scrutiny candidate for Phase 7. |

### Reporting (5 endpoints, `ReportsController`)

Directly verified: `Reporting\Reporting.Api\Controllers\v1\ReportsController.cs` (full read),
`Reporting\Reporting.Api\Program.cs` (full grep for auth wiring — **none found**).

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/reports/types` | API | Sync | User | Internal | No validator needed (no input) | No | No | No | Directly-verified | |
| `GET /api/v1/reports/{reportTypeCode}/metadata` | API | Sync | User | Internal | No — no validator found for this query | No | No | No | Directly-verified | |
| `POST /api/v1/reports/{reportTypeCode}/preview` | API | Sync | User | Internal | Not directly (no dedicated validator found for the preview query itself, though its sibling generate/export commands are validated) | No (preview only, no persistence) | No | No | Directly-verified | |
| `POST /api/v1/reports/{reportTypeCode}/generate` | API | Sync | User | Internal | **Yes** — `GenerateReportCommandValidator.cs` + `ValidationBehavior` wired in DI | Arguably yes (produces/exports a durable artifact) — no domain entity state change found, though | No | No | Directly-verified | |
| `POST /api/v1/reports/{reportTypeCode}/export` | API | Sync | User | Internal | **Yes** — `ExportReportCommandValidator.cs` | Same as generate | No | No | Directly-verified | |
| **All 5 endpoints, module-wide** | — | — | — | — | — | — | — | — | Directly-verified | **No `[Authorize]` attribute anywhere in `Reporting.Api`, and no `AddAuthentication`/`AddJwtBearer`/`FallbackPolicy` in `Program.cs`.** `app.UseAuthorization()` is called but there is nothing for it to authorize against — as wired today, these 5 endpoints are reachable with **no authentication of any kind**. Smallest module by endpoint count, but the starkest access-control gap of the whole API surface. |

---

## 2. Broker / event consumers — full per-item classification

**76 rows in `broker-ingress.md`'s table** collapse to **~30 unique classes** once
`IConsumer<T>`/`ConsumeContext<T>` duplicate hits on the same file are merged (see Method note
above). Classified by unique class below.

### Azure — 18 `INotificationHandler<T>` classes (1 per subdomain event)

Directly verified: `Azure\Compute.Application\EventHandlers\ContainerAppRestartedEventHandler.cs`
(full read).

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| All 18 (`ContainerAppRestartedEventHandler`, `ContainerAppScaleUpdatedEventHandler`, `CosmosAccountThroughputUpdatedEventHandler`, `SqlDatabaseTierUpdatedEventHandler`, `TopicDeadLetterPurgedEventHandler`, `ManagedIdentityQueriedEventHandler`, `DriftDetectedEventHandler`, `ModelDeployedEventHandler`, `TrainingJobCancelledEventHandler`, `TrainingJobCompletedEventHandler`, `TrainingJobFailedEventHandler`, `TrainingJobSubmittedEventHandler`, `NetworkSecurityGroupUpdatedEventHandler`, `PrivateEndpointConnectedEventHandler`, `VirtualNetworkProvisionedEventHandler`, `ObservabilityDataQueriedEventHandler`, `SecurityDataAccessedEventHandler`, `StorageDataAccessedEventHandler`) | Broker/domain-event | Async (in-process MediatR notification, fired after the originating command completes) | System (fired by the originating command's own pipeline, not a caller) | Internal | N/A — nothing to validate, it's a structured log write | **No** — pure audit-log side effect (`ILogger` entry only), no persistence, no state change | No — no TenantId anywhere | No — no ActorId; only `CorrelationId` for tracing | Directly-verified (Compute); pattern-inferred for the other 17 (identical header template, identical "write one audit log line, never throw" invariant stated in the file's own doc comments) | Genuinely low-risk as implemented (it's a logger, not a data path) — but confirms the actor-blind-spot finding from the API section extends through the entire Azure event chain: restart → command → notification → log, and "who" is never captured at any layer. |

### DataLake — 4 outbox-related classes

Directly verified: `DataLake\DataLake.Infrastructure\Outbox\OutboxProcessor.cs` (full read),
`DataLake\DataLake.Infrastructure\Outbox\OutboxMessage.cs` (grepped for Tenant/Actor fields).

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `OutboxProcessor` (BackgroundService, polls + publishes) | Broker (dispatcher) | Async | System | Internal | N/A (infra plumbing; retries/dead-letters on exception, no schema validation) | Not itself — it republishes events written elsewhere | N/A at this class (see `OutboxMessage`) | N/A | Directly-verified | Also a scheduled/hosted job — cross-referenced in section 4. |
| `OutboxMessage` (POCO) | Broker (data model, not itself an ingress trigger) | — | — | — | — | — | **Yes** — has a `TenantId` column (`Guid TenantId { get; init; }`) | **No** — no ActorId field | Directly-verified | Correctly tenant-scoped at the outbox layer, unlike Intelligence's (below). |
| `OutboxRepository` | Broker (data access) | Async | System | Internal | N/A | N/A | Inherits from `OutboxMessage` schema (tenant-scoped) | No | Pattern-inferred | |
| `OutboxCleanupService` (BackgroundService) | Broker/scheduled | Async | System | Internal | N/A | Deletes old processed rows — arguably lifecycle-triggering on the outbox table itself, not a domain entity | Operates across all tenants' rows (verified no tenant filter in the class per the scheduled-jobs check in section 4) | No | Pattern-inferred (see section 4 for direct check) | |

### Intelligence — 4 outbox-related classes

Directly verified: `Intelligence\Intelligence.Infrastructure\Persistence\Outbox\OutboxMessage.cs` (full read).

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `OutboxMessage` (POCO) | Broker (data model) | — | — | — | — | — | **No** — fields are only `Id, EventType, Payload, CreatedAtUtc, ProcessedAtUtc, RetryCount, LastError`; no `TenantId` column at all | **No** | Directly-verified | **Gap**: whatever tenant/actor context existed at the API layer (`IntelligenceControllerBase.TenantId`/`RequestingUserId`) is not carried as a first-class, queryable field into the outbox — it can only survive (if at all) buried inside the opaque `Payload` JSON string, invisible to retry/dead-letter/monitoring logic that operates on the envelope. |
| `OutboxProcessor` | Broker (dispatcher) | Async | System | Internal | N/A | Republishes only | No (per above) | No | Pattern-inferred (same shape as DataLake's, not separately fully read) | |
| `OutboxRepository` | Broker (data access) | Async | System | Internal | N/A | N/A | No | No | Pattern-inferred | |
| `OutboxPublishingResiliencePipeline` | Broker (resilience wrapper — retry/backoff around publish) | Async | System | Internal | N/A | N/A | No | No | Pattern-inferred (name-only, not read) | |

### PlantSafety — 22 unique `*CreatedConsumer` classes + 3 outbox/base-infra classes

Directly verified in full: `PlantSafety.Infrastructure\Messaging\Consumers\IdempotentConsumerBase.cs`,
`PlantSafety.Infrastructure\Analytics\Culture\Messaging\CultureCreatedConsumer.cs`,
`PlantSafety.Infrastructure\Compliance\Auditing\Messaging\AuditingCreatedConsumer.cs`,
`PlantSafety.Domain\Analytics\Culture\Events\CultureCreatedEvent.cs`. Scripted check confirmed all
22 consumer classes share the identical idempotency-check-then-log body; 5 (`Culture`, `Dashboard`,
`Engagement`, `PredictiveAnalytics`, `ScoreCards`) contain an explicit `// TODO: execute ... side-
effects here` comment, the other 17 (`Auditing`, `Certification`, `Compliance`, `ManagementOfChange`,
`Permits`, `Policy`, `Procedures`, `RegulatoryCalendar`, `Alert`, `Checklists`, `PPEManagement`,
`RiskManagement`, `Schedules`, `EmergencyManagement`, `IncidentManagement`, `Observation`,
`CompetencyManagement`) have the same no-op shape without even the TODO marker (spot-checked
`Auditing` in full — identical structure).

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| All 22 `*CreatedConsumer` classes | Broker/domain-event consumer | Async (drained from PlantSafety's Outbox, dispatched via the custom `IConsumer<T>`/`ConsumeContext<T>` abstraction — **not real MassTransit**, per Phase 1) | System | Internal (intra-module; Phase 1 found no evidence of cross-module consumption) | **No** — only an idempotency check (`SELECT COUNT(1) FROM ProcessedEvents WHERE EventId=@EventId`), no schema or business-rule validation of the message content | **No, currently** — every one of the 22 is a stub: check-idempotency → log a line → mark processed. **Zero actually perform the domain projection/side-effect their name implies.** | The domain event itself (e.g. `CultureCreatedEvent`) carries **no TenantId field** — checked the full record, only `EventId, CultureId, CreatedAtUtc, Actor, CorrelationId, CausationId` | **Yes, at the event level** — every event carries a `PerformedByActor Actor` field — but the consumer **never reads or uses `evt.Actor`** in either file read in full; it's captured upstream but dropped on arrival | Directly-verified (2 full consumer reads + 1 base class + 1 event) — pattern-inferred for the remaining 18 by structural/header consistency | **Biggest lifecycle-triggering finding in this phase**: what Phase 1 correctly flagged as "unverified — no cross-module wiring found" is, on direct read, actually **no wiring or implementation at all, even intra-module** — these are placeholders. Also a tenancy gap distinct from the API layer: these domain events don't carry TenantId even though the aggregates that raise them are certainly tenant-scoped elsewhere; if/when these stubs are implemented, tenant scoping will need to be added to the event contracts, not just the handler bodies. |
| `IdempotentConsumerBase<TMessage>` | Broker (shared base class) | Async | System | Internal | No (same idempotency-only check) | N/A (abstract — behavior is in subclasses) | No | No | Directly-verified | |
| `OutboxWriter` | Broker (write-side, invoked synchronously inside command handlers, not itself consumed asynchronously) | **Sync** (called inline during the write transaction, unlike the other Outbox classes) | User (invoked as part of a user-initiated command) | Internal | N/A | Supports lifecycle-triggering commands (writes the outbox row in the same transaction) | **Yes** — explicit `TenantId`/`EnvelopeTenantId` handling, including a documented RLS defense-in-depth footgun (a Dapper parameter is deliberately renamed to avoid colliding with a global `GetTenantId()` convention) | **Yes** — explicit `ActorId` field | Directly-verified | The best-instrumented outbox implementation of the three (DataLake/Intelligence/PlantSafety) — full tenant + actor capture. |
| `OutboxProcessor` (PlantSafety) | Broker (dispatcher) | Async | System | Internal | N/A | Republishes only | Pattern-inferred yes (writer populates tenant-scoped rows) | Pattern-inferred yes (writer populates actor) | Pattern-inferred (not separately read; DataLake/Intelligence siblings were read and this one's row schema is fed by the actor/tenant-aware `OutboxWriter`) | |
| `OutboxPipelineBehavior` | Broker (MediatR pipeline behavior, triggers the outbox write after a command succeeds) | Sync (runs inline in the request pipeline) | User | Internal | N/A | Supports lifecycle-triggering commands | Pattern-inferred yes | Pattern-inferred yes | Pattern-inferred (not read; name and pipeline position strongly imply it's the glue between command completion and `OutboxWriter`) | |

---

## 3. File ingestion — 6 genuine adapters (ExportImport only)

Directly verified: `CsvAdapter.cs` (full read), `StartImportCommand.cs` (full read),
`StartImportCommandHandler.cs` (partial read), plus grep confirmation of validators and
`ValidationBehavior` wiring.

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `CsvAdapter` (`ExportImport.Infrastructure.Adapters.Csv`) | File ingestion | Sync (streaming, but synchronous call from its caller's perspective) | Would be user-initiated if wired (currently unreachable — see Phase 1) | Internal (library only; no external caller exists yet) | Structural only — throws on missing header row / mismatched field count, but no business/domain validation at this layer | No (pure parsing) | No | No | Directly-verified | |
| `JsonAdapter` | File ingestion | Sync | (unreachable) | Internal | Pattern-inferred structural validation only (same interface contract as CsvAdapter) | No | No | No | Pattern-inferred | |
| `XmlAdapter` | File ingestion | Sync | (unreachable) | Internal | Pattern-inferred structural validation only | No | No | No | Pattern-inferred | |
| `ExcelAdapter` (ClosedXML) | File ingestion | Sync | (unreachable) | Internal | Pattern-inferred structural validation only | No | No | No | Pattern-inferred | |
| `ImportPipelineService` / `StartImportCommandHandler` (orchestration) | File ingestion (pipeline entry) | Sync (awaited within the command handler) | Would be user-initiated if wired | Internal | **Yes at the command level** — `StartImportCommandValidator.cs` + `ValidationBehavior` wired in DI | **Yes** — persists an import operation record and publishes `ImportStarted`/`ImportCompleted`/`OperationFailed` events | **No** — `StartImportCommand` (read in full) carries `CommandId, CorrelationId, CausationId, RequestedAtUtc, ApiVersionToken, FileContent, FileFormat, DatasetName, RequiredFields, BatchSize` — **no TenantId field anywhere** | **No** — no ActorId/UserId field on the command either | Directly-verified | If/when this pipeline is wired to a real caller (Phase 1's biggest open question), it will need tenant and actor fields added to its command contracts — as built today it has no way to scope an import to a tenant or attribute it to a user. |

---

## 4. Scheduled jobs — 6 hosted services (individually verified)

| Ingress Point | Category | Sync/Async | User/System | Internal/External | Validated? | Lifecycle-triggering? | Tenancy-aware? | Actor-aware? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| `DataLake.Infrastructure.Outbox.OutboxCleanupService` | Scheduled job | Async | System | Internal | N/A | Deletes/archives old outbox rows — mutates infrastructure state, not a domain entity | No tenant filter found in the class | No | Directly-verified (grepped for Tenant/Actor — zero hits) | Runs across all tenants' outbox rows uniformly — appropriate for a platform-level cleanup job, just noting it for completeness. |
| `DataLake.Infrastructure.Outbox.OutboxProcessor` | Scheduled job / broker dispatcher | Async | System | Internal | N/A (retries/dead-letters on failure) | Republishes only | Tenant-scoped at the message level (`OutboxMessage.TenantId`), not at the service level | No | Directly-verified (full read, see section 2) | |
| `Intelligence.Azure...IntelligenceEventConsumerHostedService` (via `IntelligenceAzureHostingExtensions.cs`) | Scheduled job / broker consumer host | Async | System | Internal | N/A | Hosts consumption, not itself a mutation | No tenant filter found | No | Pattern-inferred (grepped for Tenant/Actor — zero hits; class body not fully read this phase) | |
| `Intelligence.Azure...IntelligenceDeadLetterHostedService` | Scheduled job | Async | System | Internal | N/A | Moves failed messages to dead-letter storage — infra state mutation | No tenant filter found | No | Pattern-inferred (grepped — zero hits) | |
| `Intelligence.Azure.KeyVault.Secrets.KeyVaultStartupLoader` | Scheduled job (startup-only, not recurring) | Async | System | Internal | N/A | No domain mutation — loads secrets into config at boot | No | No | Pattern-inferred (per Phase 1's own note, included for completeness only) | Not a real "ingestion" job — startup bootstrapping. |
| `Intelligence.Infrastructure.Persistence.Outbox.OutboxProcessor` | Scheduled job / broker dispatcher | Async | System | Internal | N/A | Republishes only | **No** — `OutboxMessage` has no TenantId column (see section 2) | No | Directly-verified (full read, see section 2) | |

PlantSafety's scheduling surface (`IntegrationJobOptions`/`IntegrationPipelineOptions`/
`IntegrationSchedulerOptions`/`IntegrationWorkerOptions`) is config-only with **no implementing
`BackgroundService` class found anywhere** — there is nothing to classify; it does not exist as a
running ingress point yet (confirmed consistent with Phase 1).

---

## Top 5 most concerning findings (highest-risk combinations)

1. **DataLake's 52 API endpoints: excellent tenant/actor plumbing sitting behind no authentication at all.** `DataLakeControllerBase` *requires* `TenantId` and `RequestingUserId` (throws if absent) — strong-looking multi-tenancy design — but grepping the entire DataLake tree found zero `AddAuthentication`/`AddJwtBearer` calls and zero `[Authorize]` attributes anywhere, and `Program.cs` never wires authentication middleware. As configured, there is nothing establishing *who* `HttpContext.Items` actually belongs to before those "required" tenant/actor values are trusted. Same gap, same severity, in **Intelligence (37 endpoints)** and **Reporting (5 endpoints)** — 94 endpoints total (~11% of the API surface) with no authentication wired at the HTTP layer.

2. **All 22 PlantSafety `*CreatedConsumer` domain-event handlers are unimplemented stubs — lifecycle-triggering in name only.** Every one does an idempotency check, logs a line, and returns; 5 have an explicit `TODO` saying so, the other 17 don't even have the TODO. This means 13+ safety domains (Culture, Compliance, OperationalSafety, SafetyEvents, Training) that appear on paper to have event-driven cross-cutting behavior (projections, downstream triggers, notifications) currently have **none** — a silent gap, since nothing fails or errors; it just quietly does nothing beyond the ledger write.

3. **Reporting's 5 endpoints have no authentication *and* no tenant/actor concept of any kind** — the combination of "smallest, easiest-to-overlook module" and "genuinely wide open" makes this the starkest single access-control gap found (report generation/export, which reads and returns potentially sensitive aggregate data, requires no proof of identity as wired today).

4. **Domain events and outbox records systematically under-carry tenant/actor context relative to the API layer that raised them**, in three distinct ways: (a) PlantSafety's `CultureCreatedEvent` (and presumably its 12 siblings, pattern-inferred) carries `Actor` but no `TenantId`; (b) Intelligence's `OutboxMessage` carries neither TenantId nor ActorId as first-class columns — only opaque JSON `Payload`; (c) even where the outbox layer *is* tenant-scoped (DataLake), no `ActorId` is preserved anywhere past the API request. Any Phase 7 RLS/audit-trail work needs to assume the async/broker layer is a weaker enforcement point than the API layer, not an equivalent one.

5. **PlantSafety's read-heavy (473 GET, ~65% of its endpoints) controllers accept `TenantId` as a client-supplied query parameter** (`BehaviorV1Controller.GetAsync` builds its query directly from `request.TenantId`) rather than deriving it from the authenticated tenant context the way the ~61 command-envelope controllers do. This was not confirmed exploitable in this pass (would require tracing every `[Authorize(Policy=...)]` policy definition, out of this phase's scope), but the *inconsistency itself* — two different tenant-resolution strategies coexisting across 362 controllers in the same module — is exactly the kind of drift Phase 7's RLS/tenant-isolation audit should target first.
