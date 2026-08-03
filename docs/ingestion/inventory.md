# Ingestion Pipeline Discovery — Phase 1: Master Inventory

Root scanned: `C:\Users\dross\source`. Date: 2026-07-22.

## Coverage & Method

**Folders scanned (source):** all nine module roots listed in the task — `Azure`, `DataLake`,
`Diagnostics`, `ExportImport`, `Intelligence`, `PlantSafety`, `Reporting`, `SafetyIntegrations`,
`SafetyTooling` — walked recursively for `*.cs`, `*.dtsx`, and `*.sql` files. Total `.cs` files
scanned: **14,090**.

**`repos` folder:** checked and confirmed empty (`ls`/`find` returned nothing but the directory
itself). Skipped per the task instructions — no source to include.

**Folders explicitly excluded from every module** (build noise, not source, or duplicate working
copies): `bin/`, `obj/`, `.vs/`, `.git/`, `node_modules/`, `.idea/`, `.safety/`, `.github/`, and
`.claude/` (the last one matters: `PlantSafety\.claude\worktrees\...` contains **full duplicate
copies** of several PlantSafety test projects from in-flight agent worktrees — these were
deliberately excluded to avoid double-counting; confirmed via `Glob` that duplicates exist at
`PlantSafety\.claude\worktrees\agent-*\...`).

**Method:** Automated regex extraction (Perl script, not hand-transcription) over every `.cs` file
for each ingress pattern below, followed by manual spot-verification (via `Read`/`Grep`) of a
sample from every category and every anomaly (modules with zero hits, suspiciously short/odd
routes, keyword hits that looked like false positives). Known false positives found and excluded
during verification are called out explicitly in each category below and in the four
category-specific files. Every row in `api-ingress.md`, `broker-ingress.md`, and `file-ingress.md`
traces to a real file:line found by the scan; nothing was invented to hit a round number.

**What this pass did *not* do:** it did not execute the code, did not inspect DI registration
order/`Program.cs` wiring for every hit (spot-checked only), and did not open every one of the
~880 API endpoint methods individually — route/verb were extracted from the attribute text itself
and cross-checked against a sample. Anything not directly backed by a grep/read is marked
**unverified/needs follow-up** rather than asserted.

**Rough counts per category (production code, tests/false-positives called out separately):**

| # | Category | Count | Modules with hits |
|---|---|---|---|
| 1 | API controllers/endpoints | 879 | Azure (63), DataLake (52), Intelligence (37), PlantSafety (722), Reporting (5) |
| 2 | Message broker / event consumers | 79 (+3 test-only, excluded) | Azure (18, `INotificationHandler<T>`), DataLake (4, Outbox classes), Intelligence (4, Outbox classes), PlantSafety (53, `IConsumer`/`ConsumeContext`/Outbox) |
| 3 | File ingestion | 6 genuine (+9 comment/using hits, +45 false-positive PDF-generation hits) | ExportImport only |
| 4 | Scheduled jobs | 6 hosted-service classes + several custom cron-metadata types | DataLake (2), Intelligence (4 hosted services + custom cron scheduling in Intelligence.ML/Intelligence.Pipelines), PlantSafety (0 classes — only an options record) |
| 5 | Legacy pipelines | **0 genuine** (772 `.sql` files, all reclassified as standard stored-procedure/schema assets, not legacy pipelines; 0 `.dtsx` SSIS packages found anywhere) | PlantSafety (769 SQL scripts, all normal), SafetyTooling (3 SQL scripts, all test fixtures) |
| 6 | Cross-module ingestion | 1 genuine external-facing entry point + 1 designed-but-unwired adapter | PlantSafety (webhook receiver), SafetyIntegrations (`IInboundAdapter`) |
| 7 | External integrations | 1 genuine (webhook HMAC receiver); "E2/E5/E6" and "SafetyCulture" search terms did not find real integration hooks | PlantSafety |

## 1. API controllers / endpoints

Full detail (verb, route, file:line) in **`api-ingress.md`** — 879 rows across 5 modules.

- **Diagnostics, ExportImport, SafetyIntegrations, SafetyTooling have zero API controllers or
  minimal-API endpoints.** Verified independently by `find`/`Glob` for `*Controller.cs` (0 results
  in each) and by grepping for `Map(Get|Post|Put|Delete|Patch)\(` across the whole tree (0 minimal-API
  results anywhere in the solution — this codebase uses MVC controllers exclusively, not minimal
  APIs).
- Notable endpoint: **`PlantSafety\PlantSafety.Api\Webhooks\WebhookReceiverV1Controller.cs`** —
  `POST api/v1/webhooks/receive` ([AllowAnonymous], HMAC-SHA256 signature verification instead of
  JWT) is the one HTTP endpoint in the entire solution explicitly documented (in its own header
  comment, lines 7-40) as receiving data **from external systems**. It dispatches
  `ProcessInboundWebhookCommand` and acknowledges before async processing via the outbox.
- PlantSafety.Api accounts for 722 of 879 (82%) of all API endpoints found, spread across 362
  controller files — this module is by far the dominant HTTP ingress surface of the whole solution.
- False positive excluded: a `[HttpPost]` example inside a doc-comment in
  `PlantSafety.Api\Attributes\IdempotencyKeyAttribute.cs:17` (that file defines a marker attribute,
  not a controller).

## 2. Message broker consumers / outbox / domain event handlers

Full detail in **`broker-ingress.md`** — 79 production rows.

- **No MassTransit, Azure Service Bus SDK, or RabbitMQ client usage found anywhere** (searched
  literal `MassTransit`/`ServiceBusClient`/`RabbitMQ` across all 14,090 `.cs` files — zero hits).
  PlantSafety's `IConsumer<T>`/`ConsumeContext<T>` types are a **custom in-process abstraction**
  (`PlantSafety.Infrastructure\Messaging\Consumers\IdempotentConsumerBase.cs`), not MassTransit's
  real interfaces of the same name. Treat any assumption of "MassTransit" as **unverified/needs
  follow-up** for later phases.
- PlantSafety has 13 matched pairs of `X CreatedEvent` → `XCreatedConsumer` (Culture, Dashboard,
  Engagement, PredictiveAnalytics, ScoreCards, Auditing, Certification, Compliance,
  ManagementOfChange, Permits, Policy, Procedures, RegulatoryCalendar, Alert, Checklists,
  PPEManagement, RiskManagement, Schedules, EmergencyManagement, IncidentManagement, Observation,
  CompetencyManagement). Traced one example fully: `CultureCreatedEvent` is raised as a domain
  event in `PlantSafety.Domain\Analytics\Culture\Aggregates\Culture.cs:105`, written to the Outbox
  by `PlantSafety.Infrastructure\Persistence\UnitOfWork\OutboxWriter.cs`, drained by
  `PlantSafety.Infrastructure\BackgroundJobs\OutboxProcessor.cs:21`, and consumed by
  `CultureCreatedConsumer.cs` — all within `PlantSafety.Infrastructure`. This is an **intra-module**
  outbox/projection pattern; no evidence was found of a different top-level module (DataLake,
  Intelligence, etc.) consuming a PlantSafety event, or vice versa. Flagged **unverified/needs
  follow-up** for Phase 6 (cross-module integration validation) rather than assumed.
- Azure has 18 `INotificationHandler<T>` handlers, one per subdomain event
  (`ContainerAppRestartedEvent`, `CosmosAccountThroughputUpdatedEvent`, etc.) — same intra-subdomain
  pattern, not verified cross-subdomain.
- DataLake and Intelligence each have a 3-4-class Outbox implementation
  (`OutboxMessage`/`OutboxProcessor`/`OutboxRepository`, Intelligence adds
  `OutboxPublishingResiliencePipeline`), each `BackgroundService`-hosted (see category 4).
- **SafetyIntegrations has a custom `IEventBus`/`IEventBusMessageHandler` abstraction**
  (`SafetyIntegrations\SafetyIntegrations\EventBus\...`, ~29 files: dispatcher, dispatch policies,
  backoff policies, idempotency policies, pipeline middleware) plus a single governed inbound
  boundary interface, `IInboundAdapter`
  (`SafetyIntegrations\SafetyIntegrations\Adapters\Inbound\IInboundAdapter.cs`), implemented by
  `EnterpriseGatewayInboundAdapter.cs`. Its own header comment describes it as "the single governed
  entry point through which an external system or legacy module surface may present data to
  SafetyIntegration." No concrete caller/transport wiring this adapter to an actual HTTP listener or
  queue was found in this pass — **unverified/needs follow-up**.
- 3 rows excluded from the deliverable table as test-only: `OutboxProcessorTests`,
  `OutboxPersistenceTests`, `OutboxFailoverAndRecoveryTests` (PlantSafety test projects) — real
  files, but they test the outbox, they are not ingress points themselves.

## 3. File ingestion (CSV/Excel/PDF/image)

Full detail in **`file-ingress.md`**.

- **Zero `IFormFile` usages anywhere in the solution.** No HTTP multipart file-upload endpoint
  exists in any of the nine modules (confirmed by grep across all 14,090 `.cs` files).
- **Zero `CsvHelper`/`CsvReader` usages anywhere.**
- **ExportImport module has real CSV/JSON/XML/Excel import adapters**, all implementing
  `ExportImport.Application.Shared.Interfaces.IImportFileAdapter`:
  - `ExportImport\ExportImport.Infrastructure\Adapters\Csv\CsvAdapter.cs:25`
  - `ExportImport\ExportImport.Infrastructure\Adapters\Json\JsonAdapter.cs:26`
  - `ExportImport\ExportImport.Infrastructure\Adapters\Xml\XmlAdapter.cs:27`
  - `ExportImport\ExportImport.Infrastructure\Adapters\Excel\ExcelAdapter.cs:29` (via ClosedXML)
  
  Resolved via `ExportImport\ExportImport.Infrastructure\FileAdapterFactory.cs` and invoked by
  `ImportPipelineService.cs:31` / `StartImportCommandHandler.cs:27` /
  `PreviewImportCommandHandler.cs:26` / `ReplayImportCommandHandler.cs:28`.
- **Surprise: this capability is orphaned.** ExportImport has no `*.Api` project, and grepping the
  entire solution for `ExportImport.Application`, `ExportImport.Infrastructure`,
  `StartImportCommand`, and `ImportPipelineService` outside the `ExportImport` folder itself
  returned **zero hits**. Nothing else in the solution currently calls into this module. Flagged
  **unverified/needs follow-up**: either there's a caller this scan's patterns missed (e.g. a
  reflection-based plugin loader), or the import pipeline is built but not yet wired to any trigger.
- **SafetyTooling's 45 `PdfDocument` hits are a PDF *generation* library**
  (`SafetyTooling\SafetyReporting.Pdf`), used to *build* PDF reports, not to ingest uploaded PDFs.
  Included in `file-ingress.md` for audit transparency but explicitly called out as **not real
  ingestion** — a keyword false positive.
- No image-upload handling found anywhere.

## 4. Scheduled jobs

- **No Quartz.NET or Hangfire package usage found anywhere** (literal `Quartz`/`Hangfire`/
  `RecurringJob` searched across all `.cs` files — the only `Quartz`-adjacent hits are the string
  `CronExpression`, which is a **custom-built** scheduling concept, not the Quartz.NET library).
- Real `BackgroundService`/`IHostedService` classes found:
  - `DataLake\DataLake.Infrastructure\Outbox\OutboxCleanupService.cs:22` (BackgroundService)
  - `DataLake\DataLake.Infrastructure\Outbox\OutboxProcessor.cs:25` (BackgroundService)
  - `Intelligence\Intelligence.Azure\Extensions\Hosting\IntelligenceAzureHostingExtensions.cs:24`
    (`IntelligenceEventConsumerHostedService`, IHostedService)
  - `Intelligence\Intelligence.Azure\Extensions\Hosting\IntelligenceDeadLetterHostedService.cs:29`
    (BackgroundService)
  - `Intelligence\Intelligence.Azure\KeyVault\Secrets\KeyVaultStartupLoader.cs:24` (IHostedService —
    startup secret loader, not a recurring ingestion job; included for completeness)
  - `Intelligence\Intelligence.Infrastructure\Persistence\Outbox\OutboxProcessor.cs:34`
    (BackgroundService)
- Custom cron-based scheduling metadata (not a registered `BackgroundService` itself, just the
  schedule model/evaluator) found in:
  - `Intelligence\Intelligence.ML\Training\Schedules\TrainingSchedule.cs` (`CronExpression` property)
  - `Intelligence\Intelligence.Pipelines\Metadata\ScheduleMetadata.cs` (`CronExpression`,
    `IsCronBased`)
  - `Intelligence\Intelligence.Pipelines\Scheduling\PipelineSchedulingService.cs` (calls
    `CronExpressionEvaluator.GetNextOccurrenceUtc(...)`)
- **PlantSafety.Api has scaffolded-but-unimplemented scheduling config**:
  `PlantSafety.Api\Integrations\BackgroundServices\{Jobs,Pipelines,Schedulers,Workers}\` contains
  four `*Options` record classes (`IntegrationJobOptions`, `IntegrationPipelineOptions`,
  `IntegrationSchedulerOptions`, `IntegrationWorkerOptions`) but **no class in any of those four
  folders implements `BackgroundService`/`IHostedService`**, and nothing else in
  `PlantSafety.Api` references these options types (verified by grep). This looks like scheduling
  config for jobs that don't exist yet. Flagged **unverified/needs follow-up**.
- No scheduled-job evidence found in Azure, Diagnostics, ExportImport, Reporting,
  SafetyIntegrations, or SafetyTooling.

## 5. Legacy pipelines

- **Zero `.dtsx` (SSIS) files found anywhere** in the entire scanned tree.
- **Zero standalone/orphan SQL batch scripts found.** All 772 `.sql` files that matched the
  `/sql/`/`/scripts/` path search are conventional per-domain stored-procedure/schema assets living
  inside each module's own `*.Infrastructure\<Domain>\Sql\Procedures\` folder (e.g.
  `PlantSafety.Infrastructure\AccidentManagement\Sql\Procedures\usp_Accident_Insert.sql`) or, for
  the 3 SafetyTooling hits, test fixtures under `Analyzers.IntegrationTests\Sql\Scripts\`
  (`CreateSchema.sql`, `CreateStoredProcedures.sql`, `SeedData.sql`). None of these represent a
  legacy/standalone import pipeline distinct from the current Infrastructure layer.
- The top-level `build/`/`tools/` folders in every module were checked directly; only PlantSafety
  and SafetyTooling have real content there, and it is deployment/build tooling
  (`Build.ps1.txt`, `Deploy.ps1.txt`, `MigrateDatabase.ps1.txt`, Roslyn analyzer build props), not
  data-ingestion scripts.
- **Conclusion: this codebase has no legacy ingestion pipelines.** This is a genuine negative
  finding, not a gap in the scan.

## 6. Cross-module ingestion

- The only two artifacts in the whole solution explicitly designed as a cross-module/external
  boundary are:
  1. `PlantSafety\PlantSafety.Api\Webhooks\WebhookReceiverV1Controller.cs` (HTTP webhook receiver
     from external systems — see category 1/7).
  2. `SafetyIntegrations\SafetyIntegrations\Adapters\Inbound\IInboundAdapter.cs` /
     `EnterpriseGatewayInboundAdapter.cs` (see category 2) — its own doc comment calls it "the
     single governed entry point through which an external system or legacy module surface may
     present data to SafetyIntegration," but no live caller was found wiring it up.
- Beyond those two, every "Created event → consumer" pair found (category 2) turned out on
  inspection to be **intra-module**, not one module's output feeding another module's input. No
  shared-database-table or file-drop cross-module pattern was found — searched for cross-module
  connection strings / shared table references and found none within the scope of this pass.
- **ExportImport's import/export library is unreferenced by any other module** (see category 3) —
  a candidate cross-module ingestion point that appears to be built but not connected.
- Overall: **genuine, wired cross-module ingestion was not found** in this pass. The building
  blocks for it exist (SafetyIntegrations' inbound adapter, ExportImport's format adapters,
  PlantSafety's outbox), but the actual module-to-module wiring is either not present yet or not
  discoverable via static grep (e.g. could be configured at deploy time / via a message broker not
  present in source). This is the single biggest open question for later phases — flagged
  **unverified/needs follow-up**.

## 7. External integrations

- Searched literal tokens `E2`, `E5`, `E6` (word-boundary matched) across all 14,090 `.cs` files:
  **one hit total**, and it is a false positive — the string `"E2."` is placeholder test data in
  `Intelligence\Intelligence.IntegrationTests\AIInvestigations\ListHypothesesQueryHandlerTests.cs:84`
  (`Explanation = "E2."`), not a reference to an external system named E2. **No genuine E2/E5/E6
  integration references exist in code.**
- Searched `SafetyCulture` (word-boundary matched): 464 hits, all in PlantSafety, all inside a
  first-party module/namespace named `PlantSafety.Api.SafetyCulture` (controllers
  `CultureAssessmentsV1Controller.cs`, `StopWorkEventsV1Controller.cs` — a domain called "safety
  culture assessments / stop-work events"). **This is an internal module name, not the third-party
  SafetyCulture SaaS product** — do not conflate the two. No evidence of the actual third-party
  SafetyCulture API/webhook integration was found anywhere.
- Searched `CrossDomain`: 617 hits, all in PlantSafety, all inside the namespace
  `PlantSafety.Api.CrossDomain` (cross-cutting request-context accessors like
  `TenantContextAccessor.cs`, `CorrelationIdAccessor.cs`) — this is shared API infrastructure
  within one module, not evidence of a genuine cross-*module* integration.
- The **one confirmed genuine external-system integration point** in the whole solution is
  `WebhookReceiverV1Controller.cs` (`POST api/v1/webhooks/receive`): HMAC-SHA256-verified inbound
  webhook receiver, `[AllowAnonymous]`, fast-ack + async outbox processing, dispatches
  `PlantSafety.Application.Integrations.Commands.ProcessInboundWebhookCommand`. Its own comments
  note a planned "per-source routing... different HMAC secrets per registered external system" —
  i.e., multi-tenant external-system support is a documented future extension, not yet built.
- No other genuine third-party API client code (HttpClient calls to a named external SaaS,
  OAuth client-credentials flows to an external system, etc.) was identified in this pass. This
  should be revisited with a dedicated `HttpClient`/`IHttpClientFactory` grep pass in a later phase
  — flagged **unverified/needs follow-up**.

## Related deliverables

- `api-ingress.md` — 879 API endpoint rows.
- `broker-ingress.md` — 79 message/event consumer rows.
- `file-ingress.md` — file-format import adapter rows + excluded false positives.
