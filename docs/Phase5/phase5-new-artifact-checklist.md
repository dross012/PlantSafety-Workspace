# Phase 5 New Artifact Checklist

## Status of existing code-generation tooling

This codebase already has a full first-party developer-tooling suite, **SafetyTooling**
(`D:\PlantSafety\source\SafetyTooling`), which implements the SafetyCLI / SafetyBuild /
SafetyAnalyzer / SafetyGenerator roles referenced by the Handbook:

| Handbook role  | Project(s) on disk |
|---|---|
| SafetyCLI | `SafetyTooling\CLI` (`SafetyCLI` — `Program.cs`, `Commands\Feature\Add*`, `Commands\Generate\*`, `Commands\Module\*`, `Commands\Doctor`, `Commands\Analyze`) |
| SafetyBuild | `SafetyTooling\Build` (drift detection, `DriftHash.cs`, build rules/tasks) and `SafetyTooling\BuildPipelines` (CI wiring, see `phase5-cicd-gaps.md`) |
| SafetyAnalyzer | `SafetyTooling\Analyzers` (Roslyn analyzers/code fixes, e.g. `SFTY_SQL_001`–`SFTY_SQL_007`, `DependencyRules\Analyzers\ForbiddenReferences\DEP_FORBID_001_RawSqlClientAnalyzer.cs`) |
| SafetyGenerator | `SafetyTooling\Generators` (`Generators\StrongTypeGenerators\StrongIdGenerator.cs` — a real `IIncrementalGenerator`) plus `SafetyTooling\CLI\TemplateLibrary` (per-artifact template folders: `Commands`, `Dtos`, `Events`, `Features`, `Mappings`, `Modules`, `Queries`, `Tests`) driven by `CLI\Commands\Feature\AddCommand`, `AddEvent`, `AddQuery`, `AddDto` |
| SchemaSync / MigrationGenerators | `SafetyTooling\SchemaSync`, `SafetyTooling\MigrationGenerators` (SQL/schema-facing generators) |

**Verified gap:** every file in `SafetyTooling\CLI\TemplateLibrary\*\*.tpl` / `*.json` / `*.drift`
(e.g. `Commands\command.cs.tpl`, `Events\event.cs.tpl`) is a **3-byte placeholder** (BOM only, no
body), and `SafetyTooling\Generators\Templates\CSharp` is an **empty directory**. The CLI command
handlers, options, validators, run-handlers and scaffolder interfaces (`IFeatureEventScaffolder`,
`IFeatureQueryScaffolder`, etc.) are fully built out, but the template *content* they would render
from is not populated. In its current state `safetycli feature add-command|add-event|add-query`
cannot be relied on to produce a working file from these particular templates. Populating those
templates with real Roslyn/T4 template bodies is a nontrivial generator-authoring effort and is out
of scope for this documentation pass (per this task's own scope note: "not a request to build a new
compiler/generator platform"). This is flagged here as the concrete, current-state gap rather than
silently assumed to work.

Because the generator's template bodies are not currently usable end-to-end, this checklist gives a
developer (or a future agent) the manual steps for adding a new Command / Query / Event / Stored
Procedure, derived from the **real, working patterns** Phase 5 itself just established in
`PlantSafety.Domain`, `PlantSafety.Application`, `PlantSafety.Infrastructure`, and
`PlantSafety.Api`. Once the SafetyTooling templates are populated, this checklist doubles as the
acceptance spec for what those templates need to produce.

---

## A. New Command (CQRS write side)

Pattern grounded in `PlantSafety.Application\Workflow\Commands\*` and the cross-module consumers'
`CreateDataLakeActivityCommand` / `ValidateRiskEffectivenessCommand` usage.

1. **Command record** — `PlantSafety.Application\<Module>\Commands\<Verb><Noun>Command.cs`
   - `sealed record` implementing `IRequest<TResult>` (MediatR), with `CommandId`, `CorrelationId`,
     `CausationId`, `RequestedAtUtc`, `TenantId` (and `SiteId` where applicable) as the standard
     envelope fields — see `CreateDataLakeActivityCommand` for the canonical field set.
2. **Command handler** — `<Verb><Noun>CommandHandler.cs` in the same folder, implementing
   `IRequestHandler<TCommand, TResult>`. Resolves the aggregate repository, applies the domain
   operation, persists via the repository, raises/dispatches domain events through the outbox.
3. **Validator** — `PlantSafety.Application\<Module>\Validation\<Verb><Noun>CommandValidator.cs`
   (FluentValidation, matching `ApprovePermitWorkflowCommandValidator` etc. in `PermitToWork\Validation`).
4. **DI registration** — confirm the command/handler/validator assembly is picked up by the
   module's `*ApplicationServiceRegistration.cs` (e.g.
   `Compliance\RulesEngine\DependencyInjection\ComplianceRulesEngineApplicationServiceRegistration.cs`)
   — MediatR/FluentValidation registration in this codebase is assembly-scan based, so a new command
   in an already-registered module assembly does not usually need a manual line, but new modules do.
5. **API endpoint (if externally invocable)** — controller/action or minimal-API endpoint under
   `PlantSafety.Api\<Module>\Controllers\V{n}\` or `Endpoints\`, plus request/response contracts
   under `Contracts\Requests` / `Contracts\Responses` and a mapping profile if DTOs differ from the
   command shape (see `Api\Integrations\Auditing\Mapping\AuditingMappingProfile.cs`).
6. **Authorization policy** — if the endpoint needs one, add/extend a policy such as
   `Api\Workflow\Authorization\WorkflowAuthorizationPolicies.cs` /
   `Api\Integrations\Auditing\Authorization\AuditingAuthorizationPolicies.cs`.
7. **Tests** — add/extend unit tests in `PlantSafety.Tests\<Module>\...` and, if the command touches
   SQL, integration tests in `PlantSafety.IntegrationTests\<Module>\...` (both are monolithic test
   projects organized by module subfolder in this solution — there is no per-module test project to
   create; see `phase5-cicd-gaps.md`).

## B. New Query (CQRS read side)

Same shape as Commands but under `<Module>\Queries\<Verb><Noun>Query.cs` /
`<Verb><Noun>QueryHandler.cs`, typically backed by a `usp_<Entity>_Get*` stored procedure (see
Section D) rather than the write-side repository. Register/expose via API the same way as Commands.

## C. New Domain Event

Pattern grounded in the 8 Phase 5 cross-module consumers (e.g.
`RiskScoredEvent`, `PreventiveMaintenanceTaskCompletedEvent`,
`PredictiveMaintenanceAlertActionScheduledEvent`).

1. **Event record** — `PlantSafety.Domain\<Module>\Events\<PastTenseVerb><Noun>Event.cs`, a
   `sealed record` carrying at minimum `EventId` (strong ID `Value` accessor pattern, e.g.
   `evt.EventId.Value`), `CorrelationId`, `OccurredAtUtc`/equivalent, and the business payload.
   Some events additionally carry `TenantId` directly on the event (see
   `PredictiveSafetyAlertCreatedEvent`) rather than relying solely on the envelope header — follow
   whichever convention the owning aggregate already uses.
2. **Raise from the aggregate** — append the event to the aggregate's domain-events collection at
   the point of the real state transition (do not raise speculatively).
3. **Outbox/dispatch** — confirm the owning command handler's `SaveChangesAsync`/unit-of-work path
   flows domain events into `System.OutboxMessages` (this is the existing, shared mechanism —
   `CorrelationId`/`CausationId` on the outbox row is how unrelated aggregates without a direct FK
   stay correlatable; see the `MaintenanceCompletionFeedbackConsumer` header for a documented
   example of this exact tradeoff).
4. **If the event should trigger cross-module orchestration**, add a consumer under
   `PlantSafety.Infrastructure\Messaging\Consumers\CrossModuleOrchestration\<Pairing>\` inheriting
   `CrossModuleIdempotentConsumerBase<TEvent>` (see `phase5-event-flow.md` for the 8 existing
   examples and the base class contract: `ResolveEventId`, `HandleAsync`, `DispatchAsync`).
5. **Registration** — new consumers must be added to
   `CrossModuleOrchestrationConsumerRegistration.cs` in the same folder.
6. **Tests** — add a consumer test verifying idempotency (duplicate `EventId` is a no-op) and the
   triggered command/side-effect, matching the pattern used for the other 8 consumers.

## D. New Stored Procedure / SQL surface

Pattern grounded in `Workflow\Sql`, `Analytics\UnifiedModel\Sql`, `Compliance\RulesEngine\Sql`,
`Auditing\AuditReport\Sql` (see `phase5-schema-changes.md` for the concrete file lists).

1. **Table script(s)** — `PlantSafety.Infrastructure\<Module>\Sql\Tables\<Entity>.sql`. Header block
   matching the existing convention (`Project` / `Module` / `Layer` / `File` / `Created` / `Author`
   / `SchemaVersion` / `Description`), `USE [PlantSafety.CoreDB]; GO`, then
   `CREATE TABLE [<Schema>].[<Entity>] (...)`. Tenant-scoped tables must carry a NOT NULL
   `TenantId uniqueidentifier` column — this is how the platform's Row-Level-Security discovery
   picks the table up automatically (see `WorkflowDefinition.sql` header).
2. **Choose the right schema** — do not reuse an existing schema name for a conceptually distinct
   surface. Phase 5 deliberately created `[ComplianceRulesEngine]` as a *new* schema separate from
   the pre-existing `[Compliance]` schema specifically to avoid table/procedure name collisions
   (see `ComplianceEvaluation.sql` header) — follow the same judgment call for new schemas.
3. **Numbered follow-on scripts** for indexes/FKs/partitioning use a `0NN-description.sql` naming
   convention applied *after* the base table script in the same `Sql\Tables` folder (e.g.
   `020-additional-indexes.sql`, `030-workflow-graph-foreign-keys.sql`,
   `040-fact-dimension-foreign-keys.sql`, `050-fact-table-partitioning.sql`).
4. **Stored procedures** — `PlantSafety.Infrastructure\<Module>\Sql\Procedures\usp_<Entity>_<Verb>.sql`
   (`GetById`, `GetAllForTenant`/`ListByTenant`, `Insert`, `Update`, `GetHistoryForTarget`, etc. —
   match the verbs already used for sibling entities in the module). Raw inline SQL from
   application code is forbidden by analyzer rule `DEP_FORBID_001_RawSqlClientAnalyzer` / the
   `SFTY_SQL_00x` rule family (`SFTY-SQL-001`..`SFTY-SQL-007` under
   `SafetyTooling\Analyzers\Docs\Rules\SQL\`) — every table access must go through a stored
   procedure wrapper (`SFTY_SQL_002_MissingStoredProcedureWrapperAnalyzer`) with a parameter object
   (`SFTY_SQL_003_MissingParameterObjectAnalyzer`).
5. **Repository wiring** — implement/extend the `I<Entity>Repository` in
   `PlantSafety.Infrastructure\<Module>\` calling the stored procedure via the existing
   `ISqlConnectionFactory` pattern used throughout the consumers (`connectionFactory`,
   `IDbConnection`/`IDbTransaction` parameters).
6. **Migration registration** — confirm the new `.sql` files are picked up by whatever the module's
   existing migration/deployment script enumerates (check a sibling module's `Sql\Tables` folder
   ordering for the convention already in force before assuming).

## E. Feature flag / module registration (if introducing a new top-level module)

Use `SafetyCLI`'s already-functional (non-template) module commands as the reference for what a new
module needs structurally, even though its own file-generation templates are currently empty:
`CLI\Commands\Module\CreateCommand`, `ValidateCommand`, `SyncCommand`, and
`CLI\Commands\Feature\New`, `Enable`, `Disable`, `Flag` show the expected module manifest
(`*.json` feature-pack descriptor) and validation shape used elsewhere in the repo.

## Summary of the concrete gap

- SafetyCLI/SafetyGenerator scaffolding (commands, options, validators, scaffolder interfaces) is
  fully implemented.
- The actual template *bodies* it renders from (`CLI\TemplateLibrary\*\*.tpl` and
  `Generators\Templates\CSharp\*`) are empty placeholders — this is the one concrete, verified gap
  in the existing tooling relevant to Phase 5's new artifact shapes. Filling those templates in is a
  discrete follow-up task, not attempted here per this task's scope.
