# Phase 3 — Cross-Cutting Findings (referenced from every per-module command-handler-map file)

Builds on `domain-mapping/_shared-findings.md` (F1 tenancy, F2 handler-bypass, F3 silent-archive,
F4 Alert duplicate, F5 PermitToWork/PermitWorkflow, F6 AuditEngagement.Close). This file adds the
findings needed to fill the Command / Handler / Repository / Domain Event / Outbox / Audit columns
uniformly across all 25 per-module files, established by **direct code read this phase**.

## F7 — Outbox delivery is a universal, aggregate-agnostic mechanism (not per-transition)
`PlantSafety.Infrastructure\Common\Pipelines\OutboxPipelineBehavior.cs` is a generic MediatR
`IPipelineBehavior<TRequest,TResponse>` registered once for the whole PlantSafety application host.
After **any** command handler returns, it flushes `IOutboxWriter`'s pending events (written earlier,
inside the same unit-of-work transaction, by whichever repository's `AddAsync`/`UpdateAsync` ran) to
the `OutboxMessages` table. **Consequence: "Outbox Entry?" for every transition in every per-module
file reduces to one question — did the domain method call `AddDomainEvent`?** If yes → Y (written to
Outbox unconditionally, same mechanism, every module, no exceptions found). If no (silent-archive
transitions; Intelligence's bypassed `TrainingJob`/`Investigation` methods, where the domain method
is never even invoked) → N. This phase found no aggregate-specific outbox wiring anywhere — it is
100% generic. Per the completed Ingestion Discovery's `events/publishing-map.md` /
`consumption-map.md` (cited per-row below), a row landing in Outbox is nonetheless **never
delivered to a consumer that does real work** — see F9.

## F8 — SQL-level AuditLog insert is real, but wired into only 22 of 93 PlantSafety aggregates' write procedures — and is best-effort/anonymous even there
Every `_Insert`/`_Create`/`_Update`/`_Upsert` stored procedure was greped for the shared
`INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (...)` block. Of 130 write procedures found, only
**46 (23 aggregates × Insert+Update)** contain it:
`AccidentManagement.Accident`; `Observability.AutomatedIncidentDetection`,
`EnvironmentalAssetMonitoring`, `FacilityTrafficMonitoring`, `MachineProcessMonitoring`,
`PPEBehavioralMonitoring`; `PPE.PPEAssignment`, `PPECategory`, `PPEInventory`, `PPETrainingFit`,
`PPEZoneMapping`; `PermitToWork.PermitExpiration`, `PermitScheduling`, `PermitTemplate`,
`PermitWorkflow`; `RiskManagement.Risk`; `SafetyPolicies.SafetyPolicy`; `Scheduling.EmergencyDrillPlanning`,
`HazardIsolationSequencing`, `PMScheduling`, `ShiftFatigue`, `WorkOrderPriority`; `Scorecards.Scorecard`.
**Every other aggregate's write procedures — including all of EmployeeSafety, MachineSafety,
SafetyProcedures, ContractorSafety, Auditing (`AuditEngagement`/`AuditFinding` — directly verified,
neither proc has the block), Checklists, Compliance, DigitalTwin, AiGovernance, Dashboards,
Analytics, SafetyEvents, and the emerging-createonly cluster — write to their table with zero
AuditLog row.** Two further caveats apply even to the 23 that do have it (read directly from
`usp_Accident_Insert.sql`/`usp_Accident_Update.sql`, pattern confirmed identical in the other 22):
1. **Best-effort, silently swallowed**: the `INSERT INTO AuditLog` runs inside its own nested
   `BEGIN TRY/BEGIN CATCH` with an empty `CATCH` block — if the audit insert itself fails (e.g. the
   cross-database synonym is broken, or `AuditDB` is unreachable), the calling transaction still
   commits and nothing surfaces the audit-write failure anywhere.
2. **Not attributable to the acting user or the specific transition**: `ChangedBy` is bound to
   `SYSTEM_USER` (the SQL login executing the batch — typically one shared application service
   account, not the domain-level `PerformedByActor`/`UserId` passed into the command), and `Module`
   is `OBJECT_NAME(@@PROCID)` — the stored procedure name (e.g. `usp_Accident_Update`), which
   identifies the *aggregate* but, per the Phase 1 finding, **cannot distinguish which transition**
   (Approve vs Reject vs Archive vs any other status write through the same generic `_Update` proc)
   produced that row. A forensic reconstruction of "who did what" from `AuditLog` alone is not
   possible even for the 23 covered aggregates.
**Per-row consequence for this phase's tables:** "Audit Entry?" = **Y (SQL, best-effort, anonymous)**
for every transition (Create and non-Create alike) on the 23 aggregates named above; **N** for every
transition on the other ~70 PlantSafety aggregates, all of DataLake/Intelligence/Azure, and Reporting.
The separate `AuditLogFilter.cs` (`PlantSafety.Api\Filters\AuditLogFilter.cs`) is an `ILogger`-based
structured-log line emitted for every mutating HTTP verb at the API layer — real, but it is a log
line ("the Infrastructure layer's Serilog sink is responsible for routing these log entries to the
audit store," per its own header comment), not a confirmed database write; no Serilog sink
configuration routing these to a durable, queryable audit store was found this phase. It is **not**
counted as an "Audit Entry" in the per-row tables below because it is a per-HTTP-request log emission,
not a per-transition persisted record, and its "audit store" destination is unconfirmed. The
`IAuditLogRepository`/`AuditLogDapperRepository` pair (`Operations.AuditViewer`) is explicitly and
permanently **read-only** (`SearchAsync` only, doc comment states "must never gain an
INSERT/UPDATE/DELETE method") — it is the query surface over the same `AuditLog` table F8 describes,
not an independent write path.

## F9 — Zero domain events reach a real, working consumer anywhere in the solution (inherited from Ingestion Discovery Phase 8)
Cross-referenced from `docs/ingestion/events/publishing-map.md` and `consumption-map.md` (completed
discovery, not re-derived this phase). Three tiers, cited per-row by short code below:
- **[EVT-A] "Created" events (22 types, one per Create-only/rich aggregate's factory) —** raised,
  written to Outbox, but their MassTransit `IConsumer<T>` classes are never registered
  (`AddConsumer<T>()` missing), **and even if registered, `OutboxProcessor` publishes the raw
  `OutboxEnvelope` wrapper type, not the strongly-typed event**, so the consumer's declared generic
  type would never match. Zero live delivery.
- **[EVT-B] Non-Create lifecycle events (~30+ types across all rich PlantSafety aggregates — Alert,
  Risk, SafetyPolicy, PermitWorkflow, Scorecard, ChecklistTemplate/Completion, AuditEngagement/
  AuditFinding, CultureAssessment/StopWorkEvent, PMScheduling/WorkOrderPriority, PPE/Observability,
  and by extension the EmployeeSafety/MachineSafety/SafetyProcedures/ContractorSafety events this
  phase catalogs) —** raised and written to Outbox correctly, but **no consumer class of any kind
  exists** for any of them — not even the unregistered stub the 22 Created events got. This is the
  default status for essentially every row in this phase's tables marked "Outbox: Y".
- **[EVT-C] AccidentManagement's 7 events (1 Created + 6 investigation-lifecycle)** — same as EVT-B,
  explicitly re-confirmed absent (zero `Accident*Consumer` class of any kind).
- **[EVT-N/A]** — used where the transition itself never calls `AddDomainEvent` (silent transitions)
  or where the domain method is never invoked at all (Intelligence bypass) — there is nothing to
  cross-reference because no event instance is ever constructed.
**Practical reading for every row below:** "Domain Event Published: Y, Outbox: Y" almost never means
"and something happened as a result" — cite EVT-A/EVT-B/EVT-C to make that explicit per row rather
than re-stating the full explanation each time.

## F10 — Command/Handler file layout: one dedicated Create pair, one consolidated non-Create pair, per rich aggregate
Confirmed directly for `AccidentManagement.Accident` (`Commands/CreateAccidentCommand[Handler].cs`
+ `Commands/AccidentInvestigationCommands.cs` [6 records] + `Commands/AccidentInvestigationCommandHandlers.cs`
[6 handler classes]) and `Checklists.ChecklistCompletion` (`Commands/CreateChecklistCompletionCommand[Handler].cs`
+ `Commands/StartChecklistCompletionCommand[Handler].cs` + `Commands/CompleteChecklistCompletionCommand[Handler].cs`,
one file pair per transition in this feature area rather than consolidated — **the consolidation
convention is not uniform solution-wide**, it varies per module; both patterns were directly observed).
Every handler read this phase (10+ across AccidentManagement, Checklists, Auditing, Observability,
ContractorSafety) follows the identical shape: `_repository.GetByIdAsync(id) → null-check →
aggregate.<DomainMethod>(...) → _repository.UpdateAsync(aggregate) → map Result`. No handler read
this phase queries a second aggregate/repository to cross-validate state (reconfirms F6's "no
genuine cross-module repository call from within a command handler" finding, now cross-checked
against `EscalateDetectionToIncidentCommandHandler` — see F11 — and found to hold there too).
Command/handler **class names** in the per-module tables below follow the confirmed
`<Verb><Aggregate><Noun>Command`/`Handler` naming convention; unread ones are pattern-inferred from
this convention and the domain method name, flagged accordingly.

## F11 — Re-verification: `AutomatedIncidentDetection.EscalateToIncident` does NOT make a genuine cross-module call (resolves Phase 2 open item)
Direct read of `EscalateDetectionToIncidentCommandHandler`
(`PlantSafety.Application\Observability\Commands\EscalateDetectionToIncidentCommandHandler.cs`):
its only dependency is `IAutomatedIncidentDetectionRepository`. `IncidentReferenceId` arrives as a
raw, caller-supplied `Guid` on the command and is passed straight into
`aggregate.EscalateToIncident(incidentReferenceId, ...)` with **no repository call into
SafetyEvents/IncidentManagement to verify the referenced Incident exists**, no check of its status,
and no write-back to it. **Conclusion: this is the same category of gap as F6
(AuditEngagement.Close's unchecked ComplianceScore) — a same-process reference stored on faith, not
an actual cross-module interaction** — despite the method name implying real integration with
IncidentManagement.

## F12 — Re-verification: `ChecklistCompletion` never checks the referenced `ChecklistTemplate`'s status (resolves Phase 2 open item)
Direct read of `ChecklistCompletion.cs` (all 5 lifecycle methods: `Create`/`Start`/`RecordItemResult`/
`FlagFailedItem`/`Complete`) confirms `ChecklistTemplateId` is stored as a bare `Guid` foreign-key
field with no navigation, and none of the 5 methods reference it beyond storage. Confirmed at the
Application layer too: grep of every `Checklists` command handler for `ITemplateRepository`/
`IChecklistTemplateRepository` (or any other template-repository interface) returned **zero
matches** — no handler in this feature area has a dependency capable of loading the template at
all. **Conclusion: confirmed gap, not merely suspected** — a `ChecklistCompletion` can be started
and completed against a `ChecklistTemplate` that has since been Retired/Deprecated/never Published,
with nothing in either the domain or application layer able to detect it.

## F13 — Re-verification: `RegulatoryCalendar`'s C#-only-orphan status is worse than Phase 1 found — the domain repository interface has zero implementations at all
Phase 1 counted `RegulatoryCalendar` among 10 aggregates with "no confirmed SQL destination." Direct
read this phase of `PlantSafety.Domain\Compliance\RegulatoryCalendar\Interfaces\IRegulatoryCalendarRepository.cs`
(declares `GetByIdAsync`/`ListAsync`/`AddAsync`/`UpdateAsync`) plus a solution-wide grep for
implementations found **zero classes implement this interface anywhere** — not in
`PlantSafety.Infrastructure`, not in any worktree. The only Infrastructure class with
"RegulatoryCalendar" in its name,
`PlantSafety.Infrastructure\Compliance\RegulatoryCalendar\Persistence\RegulatoryCalendarRepository.cs`,
does **not** implement `IRegulatoryCalendarRepository` at all — it is a bare, unrelated read-only
query class (`GetByIdAsync`/`GetAllAsync` only, no `AddAsync`/`UpdateAsync`) that calls
`usp_RegulatoryCalendar_GetById`/`usp_RegulatoryCalendar_GetAll`, **neither of which exists as a
`.sql` file anywhere in the solution** (confirmed by file search, zero results). **Conclusion:** if
any command handler ever called `RegulatoryCalendar.Create` and tried to persist it via
`IRegulatoryCalendarRepository.AddAsync`, dependency injection would fail to resolve an
implementation at all — this is one level more broken than "the proc doesn't exist," it is "the C#
write-repository doesn't exist, and the read-only class sharing its name calls two more procs that
also don't exist."
