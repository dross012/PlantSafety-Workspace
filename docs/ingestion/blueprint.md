# Ingestion Pipeline Discovery — Master Blueprint

**Phase 10 of 10 (FINAL).** Root: `C:\Users\dross\source`. Date: 2026-07-22.

This document stands alone. It synthesizes all nine prior discovery phases (30+ documents under
this folder) into one blueprint for decision-making. You should not need to open any other file to
understand the headline findings and what to do about them — but every claim below links to the
phase document that backs it, in case you want the file:line evidence.

---

## 1. Executive Summary

**What this was:** A static-analysis inventory and audit of every way data enters or moves through
the 9-module .NET solution at `C:\Users\dross\source` — API endpoints, message/event consumers,
file-import adapters, scheduled jobs, and cross-module integration points. The method was
systematic, not sampled-and-guessed: automated regex/grep extraction across all **14,090 `.cs`
files**, followed by full, direct reads of one-or-more representative files per pattern group, with
every remaining member of that group explicitly marked "pattern-inferred" rather than silently
assumed. **No code was executed.** There is no running application, no test suite invocation, and
no live database behind any of these findings — everything here is what the source code says it
does, cross-checked internally (e.g. is a class actually registered in DI, does a grep for a call
site return zero results) but never confirmed by watching a real request execute.

**The single most important takeaway:** this solution defines roughly **90 distinct event types**
across its event-driven surfaces (domain events, integration events, MassTransit messages) — and
**zero of them** reach a consumer that performs a real, observable, business-meaningful action
anywhere in the solution. Every one dead-ends: never published, published into a void with no
subscriber, delivered to a consumer that's an unregistered stub, or — in the single worst case —
delivered to a `NullOutboxPublisher` that logs a warning and marks the message "delivered" while
sending it nowhere. The only events that reach *any* running consumer code are Azure's 18
same-subdomain audit-log handlers, and even those write one log line and stop. **This solution's
persistence layer works — data gets written — but its entire event/notification/cross-module
reaction layer is inert.** (Full detail: §5, [`events/publishing-map.md`](./events/publishing-map.md).)

**The second most important takeaway:** roughly **300 of PlantSafety's ~722 API endpoints
(applies broadly, including the module's largest read population) 500 on every single call today**,
because the `[Authorize(Policy = "...")]` attribute on those controllers names a policy that is
never registered in the DI container. This was directly confirmed (not just inferred) by tracing
the exact ASP.NET Core failure mode and the middleware pipeline: it fails **closed** with a clean
500 response, not a crash and not a silent bypass — but it is a confirmed, live, production-breaking
availability defect, not a theoretical risk. (Full detail: §6, [`security/rls-audit.md`](./security/rls-audit.md).)

Beyond those two headline items, this discovery found: a genuinely well-built SQL Server Row-Level
Security implementation in PlantSafety (a real strength, one of very few in the solution); a
universal bug where all 58 PlantSafety write-repositories hardcode `ActorId="system"` and
`CorrelationId=Guid.Empty` when writing to the outbox, silently destroying the audit trail for
every mutating action the module has ever recorded; three modules (DataLake, Intelligence,
Reporting — 94 endpoints) with no HTTP authentication wired at all; and two fully-built modules
(ExportImport, SafetyIntegrations) that nothing in the solution ever calls. None of this is
uniformly dire — Azure and PlantSafety's core persistence/validation/tenancy story are solid — but
the event-driven and authorization layers need focused remediation before this solution's
event-driven design or its authorization model can be trusted at runtime.

---

## 2. Complete Ingress Inventory

Pulled directly from Phase 1 ([`inventory.md`](./inventory.md)) — not re-counted here.

| Module | API Endpoints | Broker Consumers | File-Ingestion Adapters | Scheduled Jobs | Notes |
|---|---|---|---|---|---|
| **PlantSafety** | **722** (362 controller files) | 22 unique `*CreatedConsumer` classes (+ Outbox writer/processor) | 0 | 0 running classes (4 unimplemented `*Options` scaffolds only) | 82% of the solution's entire API surface |
| **Azure** | 63 (9 subdomains) | 18 `INotificationHandler<T>` | 0 | 0 | "Repository" = live Azure Resource Manager API calls, not a DB |
| **DataLake** | 52 (10 controllers) | 4 (Outbox classes) | 0 | 2 (`OutboxProcessor`, `OutboxCleanupService`) | No source-controlled SQL anywhere in the module |
| **Intelligence** | 37 (6 controllers) | 4 (Outbox classes) | 0 | 4 (2 hosted services + KeyVault startup loader + custom cron scheduling) | Has real, well-guarded domain aggregates the app layer never calls |
| **Reporting** | 5 (1 controller) | 0 | 0 | 0 | Smallest module; no auth, no tenancy, no persistence at all |
| **Diagnostics** | 0 | 0 | 0 | 0 | No `*.Api` project; genuinely empty of ingress |
| **ExportImport** | 0 | 0 | 6 genuine adapters (CSV/JSON/XML/Excel) | 0 | Fully built, zero live callers anywhere in the solution |
| **SafetyIntegrations** | 0 | Custom `IEventBus` abstraction (~29 files, no live caller) | 0 | 0 | 1 designed-but-unwired inbound adapter (`IInboundAdapter`) |
| **SafetyTooling** | 0 | 0 | 0 (PDF *generation* library, not ingestion — false positive excluded) | 0 | Not an ingestion surface at all |
| **Solution total** | **879** | **79 rows / ~30 unique classes** | **6 genuine (+ 54 excluded false positives)** | **6 hosted services** | 14,090 `.cs` files scanned; 0 legacy pipelines, 0 SSIS packages, 0 Quartz/Hangfire found |

One genuine external-facing ingress point exists in the entire solution:
`PlantSafety`'s `POST api/v1/webhooks/receive` (HMAC-SHA256, `[AllowAnonymous]`). Everything else is
internal-caller-facing. See [`api-ingress.md`](./api-ingress.md), [`broker-ingress.md`](./broker-ingress.md),
[`file-ingress.md`](./file-ingress.md), [`classification-matrix.md`](./classification-matrix.md) for
full row-level detail.

---

## 3. Complete Module Flow Map

Condensed from Phase 3 ([`module-flow/*.md`](./module-flow/)) — one paragraph per module, detail left in the linked files.

- **[Azure](./module-flow/azure.md)** — Every mutating endpoint follows an identical, rigid shape:
  controller → command handler → a live ARM API call (not a database write) → an in-process
  domain-event publish → a same-subdomain, log-only audit handler. This is a **complete, working,
  intentionally thin** lifecycle — the event's own doc comments concede cross-module fan-out was
  designed for but never built. Best-secured module in the solution (real JWT auth wired
  everywhere), with a total blind spot on actor identity (only `CorrelationId` is ever captured).

- **[DataLake](./module-flow/datalake.md)** — Every lifecycle-triggering flow traced (Ingestion,
  Processing, Integration) is a dead-end, in three distinct ways: events defined but never
  instantiated; one event genuinely published via in-process MediatR with zero subscribers; and a
  fully-wired Outbox (processor, cleanup service, tenant-scoped schema) that no command handler ever
  writes to. Strong tenant/actor plumbing at the controller-base level, but zero authentication
  wired anywhere in the module.

- **[Intelligence](./module-flow/intelligence.md)** — The most severe single dead-end in the whole
  discovery: `TransformDataCommand` correctly publishes through the Outbox, which is then handed to
  `NullOutboxPublisher` — a stub whose own header comment says it's "the default registration when
  no real event bus has been registered" — which logs a warning and marks the message
  `Processed = true` without delivering it. Every other command handler either never publishes at
  all, or (per §4 below) bypasses its own well-designed domain aggregate entirely.

- **[PlantSafety](./module-flow/plantsafety.md)** — 82% of the solution's endpoints. A real
  MassTransit + RabbitMQ bus is genuinely stood up (correcting an earlier assumption that this was a
  fake abstraction) — but zero consumers are ever registered to it (`AddConsumer<T>()` is never
  called), and separately, the Outbox processor publishes the wrong CLR type entirely, so even
  registering the consumers would not be sufficient on its own. Every lifecycle-triggering command
  reaches the database correctly; every path beyond persistence terminates before producing any
  observable effect. The webhook receiver's own documentation promises async outbox processing that
  does not exist in the code.

- **[Reporting](./module-flow/reporting.md)** — Architecturally the simplest module: no domain
  aggregates, no repository/DbContext writes, no events published, ever. Every endpoint either
  returns catalog/metadata or renders a PDF and streams it back in the same HTTP response. Its one
  defining problem is access control, not flow (see §6).

- **[Diagnostics](./module-flow/diagnostics.md), [ExportImport](./module-flow/exportimport.md),
  [SafetyIntegrations](./module-flow/safetyintegrations.md), [SafetyTooling](./module-flow/safetytooling.md)**
  — No live ingress path exists in any of these four modules today. Diagnostics and SafetyTooling
  are genuinely empty of ingestion surface (the latter is a PDF *generation*, not ingestion,
  library). ExportImport and SafetyIntegrations are the two "built but unwired" cases: real,
  substantial engineering investment (file adapters; a custom event-bus/inbound-adapter framework)
  that nothing in the solution currently calls.

---

## 4. Complete Lifecycle Coverage

From Phase 6 ([`lifecycle-coverage/*.md`](./lifecycle-coverage/)) — which aggregates/modules are
best- and worst-covered, by three different failure modes.

**Best-covered:** PlantSafety's **AccidentManagement** (`Accident` aggregate) is the single
best-guarded lifecycle in the solution — full status-machine enforcement including a genuine
cross-field business rule (`CloseInvestigation` requires regulatory filing to be complete first,
when required) and a correctly-populated audit log. PlantSafety also has ~12 other "rich"
state-machine aggregates (Alert, Risk, SafetyPolicy, PermitWorkflow, Scorecard, ChecklistTemplate/
Completion, CultureAssessment, AuditEngagement/AuditFinding, PMScheduling, PPEAssignment/Inventory,
Observability aggregates) that are consistently well-guarded. PlantSafety also has genuine SQL
Server Row-Level Security backing all of this (§6).

**Worst-covered, by three distinct failure modes:**

1. **Never grew past Create** (PlantSafety, ~50 of ~100 aggregates; DataLake, all of it): the
   domain model's entire public surface is `Create(...)` — no Update, Close, Cancel, or Delete
   exists anywhere. This is the majority shape in both modules; DataLake has no C# aggregate layer
   at all, so this is structural rather than an oversight there.
2. **Guarded in a class the application never calls** (Intelligence — the headline finding of this
   axis): `TrainingJob` and `Investigation` implement genuinely correct, well-guarded state
   machines, but every command handler bypasses the aggregate entirely and calls the Dapper
   repository directly. A code reviewer reading only the domain layer would conclude this module has
   the best lifecycle enforcement in the solution; a reviewer reading only the handlers would see
   plain CRUD passthrough. Both are true, because the two layers never connect — this is flagged as
   **more insidious** than a simple missing guard, because it creates false confidence.
3. **A correctly-built transition unreachable from any endpoint** (PlantSafety): `PMScheduling.Cancel`
   is the one `Cancel(...)` method in ~100 aggregates, and it is fully guarded and audit-logged — but
   no controller route exposes it. The inverse problem also recurs: **Archive transitions
   (Accident, Risk, Scorecard, confirmed; SafetyPolicy, PermitWorkflow, suspected) are guarded but
   never publish an event**, an apparently deliberate but undocumented design choice.

Azure's coverage is **uniform and shallow by explicit design** — no local state machine was ever
intended (state lives in Azure's own control plane), and this is internally consistent, not a gap.

---

## 5. Complete Event Publishing Map

From Phase 8 ([`events/publishing-map.md`](./events/publishing-map.md),
[`events/consumption-map.md`](./events/consumption-map.md)) — the headline finding, restated with
its mechanism-by-mechanism breakdown.

**~90 event types defined. ~59 actually published (instantiated and handed to some
dispatch mechanism). Zero delivered end-to-end to a consumer that performs a real, business-meaningful action.**

| Mechanism | What happens | Count affected |
|---|---|---|
| **Event class defined, never instantiated anywhere** | DataLake's `RawDataIngestedEvent` and siblings; Intelligence's `DataLakeIngestionCompletedIntegrationEvent`, `TrainingJobStartedIntegrationEvent` and siblings; Intelligence's 12 domain-aggregate events (dead code — see §4 item 2); ExportImport's 3 events (module has no caller at all) | ~20+ |
| **Published via in-process MediatR, zero subscribers** | DataLake's `IntegrationEventPublishedEvent` and `DataAccessApprovedEvent` — the module doing the "right thing" and still dead-ending | 2 |
| **Published to a real bus, consumer unregistered + type mismatch** | PlantSafety's 22 `*CreatedEvent`→`*CreatedConsumer` pairs: real MassTransit+RabbitMQ is stood up, but `AddConsumer<T>()` is never called for any of them, **and** `OutboxProcessor` publishes the raw envelope wrapper type, not the strongly-typed event each consumer expects — registering the consumers alone would not fix delivery | 22 |
| **Published, written to outbox, zero consumer of any kind** | PlantSafety's `AccidentCreatedEvent` and 6 investigation-lifecycle siblings, plus ~30 non-Created lifecycle events across ~26 other rich aggregates — not even a stub exists | ~37 |
| **Published, delivered, consumer is an active lie** | Intelligence's `DataLakeTransformationCompletedIntegrationEvent` — correctly written to the Outbox, correctly polled, handed to the sole registered `IOutboxPublisher` implementation: `NullOutboxPublisher`, which logs a warning and marks the message `Processed = true` without sending it anywhere. **The single most severe dead-end in the entire discovery** — an active stand-in silently hiding a production gap from monitoring, not merely an absence. | 1 |
| **Published, delivered, consumer runs but is log-only** | Azure's 18 `INotificationHandler<T>` handlers — the *only* category in the whole solution where a published event reaches real, running consumer code. Each writes one audit-log line, by explicit design (each handler's own header comment states "never throws," "log-only") | 18 |

**No event loop exists anywhere** — a loop requires a consumer to react by publishing a further
event, and since no consumer in this solution does anything beyond logging or marking a row
processed, the precondition for a loop doesn't exist today. This should be re-checked the moment
any of the above dead-ends are fixed in isolation.

---

## 6. Complete RLS / Tenant / Actor Propagation Map

From Phase 7 ([`security/rls-audit.md`](./security/rls-audit.md),
[`security/actor-propagation.md`](./security/actor-propagation.md)) — the definitive
authorization-policy finding, PlantSafety's real RLS versus the rest of the solution, and the
ActorId/CorrelationId hardcoding bug.

**The ~300-endpoint 500 finding, resolved definitively:** Phase 4 left open whether PlantSafety's
client-supplied-`TenantId` read pattern was a live cross-tenant data leak or an availability defect.
Phase 7 resolved it with direct evidence: a full attribute census (369 real `[Authorize]` usages
across 362 controllers) cross-referenced against DI registration found that **only two policies are
ever registered** (`RequireAuthenticatedUser`, `RequireDashboardRead`), while **293 controllers**
use a feature-specific `<Feature>AuthorizationPolicies.Read`-style name (147 distinct policy
classes) and **~13 more** use quoted module-level strings matching `AddPlantSafetyPolicies()` — a
method that is written, compiles, and is **never called anywhere in the solution**. Cross-checking
the 155 files with client-supplied TenantId against the population using a policy that actually
resolves found **zero overlap** — meaning the theoretically-dangerous tenant-leak population and
the population that could actually let a request through are disjoint. **Verdict: (b), a confirmed
availability defect** — every one of these ~306 endpoints throws `InvalidOperationException` inside
ASP.NET Core's authorization middleware, caught by the solution's own exception middleware, and
returned as a clean RFC 7807 500. This fails closed, not open — no caller sees another tenant's
data via this path, they see a 500. **The recommended fix is explicitly not to strip the `Policy=`
argument down to bare `[Authorize]`** — doing so would convert this from an availability bug into a
live cross-tenant leak on any endpoint whose query still derives `TenantId` from client input.

**PlantSafety's real RLS — a genuine strength:** Contrary to what the risk profile above might
suggest, PlantSafety has **real, deny-by-default SQL Server Row-Level Security** on `CoreDB` and
`DiagnosticsDB` — a `SESSION_CONTEXT`-based predicate function, a dynamically self-healing security
policy with both FILTER (read) and BLOCK (write) predicates, wired through the two shared Dapper
execution paths every repository routes through (so it can't drift repo-by-repo), with
`@read_only=1` session context specifically to defeat connection-pooling cross-tenant leakage — and
it is **integration-tested against a live Testcontainers SQL Server instance**, not just asserted.
This is architecturally sound and the strongest tenant-isolation control found anywhere in the
solution.

**Everyone else has nothing equivalent:** DataLake, Intelligence, Azure, and Reporting have **zero**
RLS or EF Core global-query-filter equivalent (confirmed by grep — zero hits across all four).
DataLake and Intelligence have sound, correctly-derived per-query `WHERE TenantId = @TenantId`
filtering in C# — but this filtering sits behind **zero HTTP authentication wired in either
module**, so the identity being filtered on is never established. Reporting has no tenant concept,
no RLS, and no authentication at all — the starkest access-control gap in the solution: an
unauthenticated network caller can generate/export a rendered PDF of aggregate safety data today.

**The ActorId/CorrelationId hardcoding bug:** PlantSafety's API layer genuinely captures
`ActorId`/`CorrelationId` correctly (via `RequestEnvelopeFactory`, sourced from JWT claims and
request middleware) — but **all 58 of PlantSafety's write-repository outbox-enqueue call sites**,
with no exception found, call `outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(),
"system", Guid.Empty.ToString())` — discarding the real values the command handler received,
including in `AccidentDapperRepository`, the exact repository earlier cited as proof the envelope
pattern works end to end. **The correct fix already exists and compiles**:
`IUnitOfWork.EnqueueEvent(...)` threads real values through correctly but has **zero callers**
anywhere in the solution. This is a mechanical, low-risk fix (route 58 call sites through an
already-working method) with high impact: every audit trail / "who did this" question is currently
unanswerable for every mutating action PlantSafety has ever recorded.

---

## 7. Complete Modernization Recommendations — Cross-Module Top 10

Synthesized and re-ranked across all seven per-module recommendation documents
([`recommendations/*.md`](./recommendations/)) by actual solution-wide impact and effort, not
concatenated by module.

| # | Fix | Module(s) | Effort | Risk if not fixed | Source |
|---|---|---|---|---|---|
| 1 | **Register PlantSafety's 147 feature-level authorization policies** (or route through the already-written `AddPlantSafetyPolicies()`). This is the one item on this list that is an active, confirmed, production-breaking defect happening on every request right now. | PlantSafety | S–M | **Availability — confirmed live, not theoretical** | [plantsafety-recommendations.md](./recommendations/plantsafety-recommendations.md) #1, [rls-audit.md](./security/rls-audit.md) §1 |
| 2 | **Route PlantSafety's 58 write-repository outbox calls through the existing, already-correct `IUnitOfWork.EnqueueEvent(...)`** instead of hardcoding `"system"`/`Guid.Empty`. Mechanical, low-risk, and the plumbing already compiles. | PlantSafety | M | **Data integrity — audit trail is unrecoverable for every mutating action ever recorded** | [plantsafety-recommendations.md](./recommendations/plantsafety-recommendations.md) #3, [actor-propagation.md](./security/actor-propagation.md) |
| 3 | **Replace Intelligence's `NullOutboxPublisher` with a real transport, or at minimum stop marking undelivered messages `Processed = true`.** The single most actively-misleading dead-end in the solution — it hides a production gap from monitoring today. | Intelligence | M–L | **Data integrity — messages are recorded as delivered when they were not** | [intelligence-recommendations.md](./recommendations/intelligence-recommendations.md) #1 |
| 4 | **Wire authentication (JWT + `[Authorize]`) into DataLake.Api, Intelligence.Api, and Reporting.Api.** Three modules, same fix, same root cause: strong-or-absent tenant plumbing sitting behind literally no caller-identity check. Reporting is the most urgent (also renders potentially sensitive data with zero tenancy). | DataLake, Intelligence, Reporting | M each (S for Reporting, smallest module) | **Security — confirmed, an unauthenticated caller can supply any tenant/actor value or receive any tenant's data today** | [datalake-recommendations.md](./recommendations/datalake-recommendations.md) #1, [intelligence-recommendations.md](./recommendations/intelligence-recommendations.md) #3, [reporting-recommendations.md](./recommendations/reporting-recommendations.md) #1 |
| 5 | **Register PlantSafety's 22 MassTransit consumers via `AddConsumer<T>()`, AND fix `OutboxProcessor` to publish the deserialized strongly-typed event instead of the raw envelope.** Both changes are required together — neither alone fixes delivery. | PlantSafety | M | **Availability/tech-debt today — blocks 22 safety domains' entire event-driven design from ever working** | [plantsafety-recommendations.md](./recommendations/plantsafety-recommendations.md) #2/#6 |
| 6 | **Fix Intelligence's `TrainingJob`/`Investigation` command handlers to actually invoke the well-guarded aggregate**, instead of bypassing it for direct Dapper calls. Flagged as higher priority than ordinary Create-only gaps because it creates false confidence in a code review. | Intelligence | L | **Data integrity — state transitions have zero enforcement anywhere in the stack if the stored procedures also lack a guard (unconfirmable from source)** | [intelligence-recommendations.md](./recommendations/intelligence-recommendations.md) #3 |
| 7 | **Wire the PlantSafety webhook receiver's promised "async outbox processing"** (currently only stores the raw payload, contradicting its own header comment) and give `AccidentCreatedEvent` + its 6 siblings at least a stub consumer — the module's flagship, best-guarded aggregate has zero event-driven side today. | PlantSafety | M | **Data integrity / documentation-mismatch — the one external-facing endpoint tells its caller data was processed when it wasn't** | [plantsafety-recommendations.md](./recommendations/plantsafety-recommendations.md) #4/#7 |
| 8 | **Feed or remove DataLake's fully-built, hosted, but never-written-to Outbox**, and register consumers for its two correctly-published-but-orphaned events (`IntegrationEventPublishedEvent`, `DataAccessApprovedEvent`). | DataLake | M | **Tech-debt today (dormant, not broken)** — real engineering effort sitting idle | [datalake-recommendations.md](./recommendations/datalake-recommendations.md) #2/#3 |
| 9 | **Decide, deliberately, on ExportImport (wire it up — it fills a genuine solution-wide "no file-upload path exists anywhere" gap) versus SafetyIntegrations (formally shelve unless a concrete external system is named).** Both are fully-built, zero-caller modules, but the right call differs: ExportImport is immediately useful the moment one controller is added; SafetyIntegrations has no identified counterparty to build against. **Add `TenantId`/`ActorId` to `StartImportCommand` before adding any HTTP endpoint to ExportImport, not after.** | ExportImport, SafetyIntegrations | M (ExportImport) / S (decision only, SafetyIntegrations) | **N/A today (unreachable); real design gap the moment either is connected** | [exportimport-recommendations.md](./recommendations/exportimport-recommendations.md), [safetyintegrations-recommendations.md](./recommendations/safetyintegrations-recommendations.md) |
| 10 | **Fix silent ADO.NET string truncation** (`StoredProcedureBuilder.AddString` with a `Size` smaller than the incoming value truncates with no exception — concretely exploitable today via `TriggerAlertCommand`, which has no validator at all) and **decide whether Archive transitions should publish an event** across PlantSafety's rich aggregates (currently a consistent, likely-intentional but undocumented silent-terminal pattern). | PlantSafety | S (Alerts fix) / M (full audit) | **Data integrity — rows silently written with corrupted/truncated data, no error signal anywhere** | [plantsafety-recommendations.md](./recommendations/plantsafety-recommendations.md) #5/#13 |

**Explicitly and deliberately not recommended anywhere in this discovery**: building speculative
consumers/controllers ahead of a named business need (SafetyIntegrations, the 30+ non-Created
PlantSafety lifecycle events); adding Update/Delete/Cancel to the ~50 Create-only PlantSafety
aggregates without a product review (may be intentionally simple); building full SQL Server RLS for
DataLake before its SQL is even under source control; and reflexively treating Azure's log-only
event consumers as a defect rather than confirming with the product team whether that's the
intended final state.

---

## 8. Appendix: Method & Confidence Notes

**What this discovery is:** a systematic *static* analysis of source code — regex/grep extraction
across all 14,090 `.cs` files in the solution, followed by full direct reads of representative
files per pattern group, cross-checked internally (DI registration grep, call-site grep, forced
clean builds where noted) across 9 phases and ~30 documents. Every finding in this blueprint traces
to a specific file:line in one of the linked phase documents.

**What it is not:** nothing in this solution was ever run. There is no live application, no test
execution, no database connection, and no HTTP request behind any finding here. Every "confirmed"
claim means "confirmed by reading the source and its DI/registration wiring," not "confirmed by
observing runtime behavior."

**Confidence varies by population size, and this matters most for PlantSafety.** PlantSafety
accounts for 722 of the solution's 879 endpoints (82%) across 362 controller files and ~100 domain
aggregates — far too many to read individually. The method used throughout was: **directly read one
or more representative files per structural pattern** (e.g., one `RequestEnvelopeFactory` controller,
one Create-only aggregate, one of the 22 `*CreatedConsumer` classes), confirm the pattern is
byte-for-byte or structurally identical across the rest of the group (via scripted grep counts,
`throw new`/`AddDomainEvent` call-site counts, or file-listing census), and mark every
non-individually-read member of that group **"pattern-inferred"** rather than silently treating it
as verified. Every phase document states, per finding, whether it is **directly verified** (the
exact file was read) or **pattern-inferred** (verified by structural consistency across a sample,
not read line-by-line). This blueprint inherits that same labeling; nothing has been upgraded from
pattern-inferred to directly-verified in the synthesis.

**The two headline findings in §1 rest on different confidence levels, and that distinction
matters for how you act on them:**

- The **0-of-90-events-delivered** finding (§5) is built from many individually-verified pieces
  (DI registration reads, `AddConsumer<T>()` grep censuses, full reads of `OutboxProcessor`,
  `NullOutboxPublisher`, and representative consumers/handlers in every module) plus
  pattern-inference for the ~17 of PlantSafety's 22 consumer bodies not individually re-read this
  pass (their byte-for-byte structural identity to the 5 that were read was scripted-confirmed, not
  assumed). This finding is **high-confidence** as static analysis goes, but static analysis cannot
  rule out a fallback or side-channel delivery mechanism that grep didn't find — e.g., a reflection-
  based dispatcher, a configuration-driven binding set up outside source control, or an
  environment-specific DI override applied at deploy time outside this repository.
- The **~300-endpoint 500 finding** (§6) is the most security/availability-critical claim in this
  document and rests on the strongest evidence gathering used anywhere in this discovery: a full
  attribute census (369 real usages), a full DI registration read, a forced clean `dotnet build`,
  and a traced middleware pipeline confirming the exact ASP.NET Core failure mode. **This should
  still be confirmed with a live runtime or integration test call to a handful of the affected
  endpoints before being treated as fully certain.** Static analysis, however thorough, cannot see a
  custom `IAuthorizationPolicyProvider`, a fallback/default policy configured via a mechanism this
  scan's grep patterns didn't match, or an environment-specific startup override that resolves these
  policy names at runtime in a way not present in this source tree. No such mechanism was found — but
  "not found by grep" is evidence of absence, not proof of it, and this is exactly the kind of claim
  where the cost of being wrong (assuming a live outage that doesn't actually exist, or conversely
  dismissing a real one) is high enough to warrant a runtime check before it drives an incident
  response or a remediation sprint.

**Recommendation for any reader acting on this document**: treat every finding as sound directional
guidance for where to invest remediation effort, but **run a real integration/runtime test against
the specific authorization-policy and event-delivery claims in §5 and §6 before treating either as
fully certain** — both because they are the highest-stakes findings in this document, and because
they are exactly the category of finding (configuration resolved at startup, delivery confirmed only
by absence-of-evidence) that static analysis is structurally least equipped to fully settle on its
own.

---

*This blueprint synthesizes: [`inventory.md`](./inventory.md), [`api-ingress.md`](./api-ingress.md),
[`broker-ingress.md`](./broker-ingress.md), [`file-ingress.md`](./file-ingress.md),
[`classification-matrix.md`](./classification-matrix.md), [`module-flow/`](./module-flow/) (9 files),
[`gaps/`](./gaps/) (3 files), [`shape-audit/`](./shape-audit/) (5 files),
[`lifecycle-coverage/`](./lifecycle-coverage/) (4 files), [`security/`](./security/) (2 files),
[`events/`](./events/) (2 files), and [`recommendations/`](./recommendations/) (7 files).*
