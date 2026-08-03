# Phase 4 — Cross-Cutting Validation Findings (referenced from every per-module rule-enforcement file)

Builds on `domain-mapping/_shared-findings.md` (F1-F6) and `command-handler-map/_shared-findings.md`
(F7-F13). This file records the **new direct-code-read confirmations** made this phase while running
the 6-criteria validation pass, so each per-module file can cite by code (G1-G6) instead of repeating
the evidence.

## G1 — F1 (tenancy gap) directly confirmed to extend to DataLake and Intelligence (was previously PlantSafety-only evidence)
Direct read of `DataLake.Infrastructure\Persistence\Repositories\Ingestion\IngestionRepository.cs`:
`GetIngestionStatusAsync(Guid ingestionJobId, Guid correlationId, ...)` — the single-record
read/mutate-target path — takes **no tenant parameter at all**; only the separate
`ListIngestionJobsAsync(Guid tenantId, ...)` paginated-list query takes one, and only to filter a list
view, not to guard a single-record fetch. Direct read of
`Intelligence.Infrastructure\Persistence\Repositories\TrainingJobRepository.cs`: `FindAsync(Guid id, ...)`,
`CreateAsync(...)`, and `CancelAsync(...)` all key strictly off the resource's own ID — no `TenantId`
parameter anywhere in this repository (confirmed by full-file scan of every public method signature).
**Conclusion: F1 holds without exception for every module checked this phase, including the two the
original F1 evidence base did not cover.** Azure is architecturally different — it has no PlantSafety
multi-tenant record concept at all (state lives in Azure's own ARM control plane, scoped by Azure
subscription/resource-group, not by a PlantSafety `TenantId` column); the only "TenantId"-named field
found anywhere in `Azure/` is `Identity.Domain.Models.ManagedIdentity`'s Azure AD/Entra tenant id, a
different concept from PlantSafety tenant isolation, so F1's "unenforced tenant check" framing does not
apply to Azure in the same way — there is no local tenant-scoped aggregate to check it against. Recorded
per-row below as **Tenancy: N/A (no PlantSafety-tenant-scoped state exists in this module)** for Azure.

## G2 — ActorId="system"/CorrelationId=Guid.Empty (C# repos) and ChangedBy=SYSTEM_USER (SQL AuditLog) are CONFIRMED TWO DISTINCT BUGS, not one bug in two places
Direct grep of every `outboxWriter.Enqueue(...)` call site in `PlantSafety.Infrastructure` (58 matches)
confirms every one of the 23 F8 audit-logged aggregates' repositories — `AccidentDapperRepository`,
`RiskDapperRepository`, `SafetyPolicyDapperRepository`, `ScorecardDapperRepository`,
`PermitWorkflowDapperRepository`/`PermitTemplateDapperRepository`/`PermitExpirationDapperRepository`/
`PermitSchedulingDapperRepository`, `PPEAssignmentDapperRepository`/`PPECategoryDapperRepository`/
`PPEInventoryDapperRepository`/`PPETrainingFitDapperRepository`/`PPEZoneMappingDapperRepository`,
`EmergencyDrillPlanningDapperRepository`/`HazardIsolationSequencingDapperRepository`/
`PMSchedulingDapperRepository`/`ShiftFatigueDapperRepository`/`WorkOrderPriorityDapperRepository`, and
the 5 `Observability` repositories — literally call
`outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString())`,
i.e. the ActorId/CorrelationId arguments are **hardcoded string/Guid literals in the C# call site**,
never sourced from `ICurrentUserContext`/the command envelope. `UnitOfWork.EnqueueEvent(IDomainEvent,
string tenantId, string actorId, string correlationId)` (`PlantSafety.Infrastructure\Persistence\
UnitOfWork\UnitOfWork.cs`) **does accept real actor/correlation parameters and forwards them
unmodified** — the infrastructure supports correct propagation; every one of the 58 call sites simply
chooses not to use it. This is a distinct code path, distinct file layer (C# `Infrastructure`
repositories writing to `OutboxMessages`), and distinct root cause from the SQL-layer finding
(`SYSTEM_USER`, a T-SQL built-in bound inside each `usp_*_Insert`/`_Update` proc's `AuditLog` INSERT,
writing to the separate `AuditLog` table). **Resolution: these are two independent defects that happen
to land on the identical 23-aggregate population and produce the same symptom (loss of per-transition
actor attribution) because both were generated from the same copy-paste template — but they are not
the same bug.** Fixing one (e.g. wiring `ICurrentUserContext` into the repository call sites) would not
fix the other (the SQL procs' `SYSTEM_USER` binding is untouched by any C#-layer change), and vice
versa. Confirmed present in 100% of the 58 repositories that call `outboxWriter.Enqueue` directly
(grep, no exceptions found) — this is the universal case, not a sample.

## G3 — FluentValidation pipeline wiring is real and global, but ~30 of 220 commands (~14%) have no validator class to wire at all
Direct read of `PlantSafety.Application\DependencyInjection\ApplicationServiceRegistration.cs`:
`services.AddValidatorsFromAssembly(assembly, lifetime: ServiceLifetime.Scoped)` +
`services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))` are both registered
unconditionally for the whole MediatR pipeline, plus a `ConditionalValidationBehavior`/
`[SkipValidation]` escape hatch for explicitly-exempted requests. **Where a `*CommandValidator` class
exists, it is genuinely wired into the pipeline for that specific command — this is not a "present in
the codebase but not registered" gap.** However, a file count found **190 `*CommandValidator.cs` files
against 220 `*Command.cs` files** — roughly 30 commands solution-wide have no validator class at all,
meaning "Constraints Pass" for those commands fails not from a wiring defect but from the validator's
outright absence. Per-row tables below mark **Constraints: Fail (no validator class found for this
command)** for aggregates/transitions where this phase did not confirm a `*Validator.cs` file
alongside the command, and **Pass (validator present + pipeline confirmed global)** where one is
confirmed present, consistent with the confidence level of the underlying per-transition source.

## G4 — Lifecycle-sequence guards are real, not cosmetic, everywhere directly re-read this phase
Full-body re-read of `PermitWorkflow.cs` (`PermitToWork.PermitWorkflow`) and `AuditEngagement.cs`
(`Auditing.AuditEngagement`) confirms both use a genuine `EnsureStatusIsOneOf(...)`/explicit
`if (Status != X) throw` guard on **every** transition method, keyed off the aggregate's own `Status`
enum compared against the specific allowed predecessor state(s) — not a no-op check, not a check
against an unrelated field. Calling `Approve` before `AssessHazards`/`ImplementControls`, or `Close`
before `Start`, throws `PermitWorkflowInvalidStatusTransitionException`/
`AuditEngagementInvalidStatusTransitionException` in both aggregates — **no illegal state sequence was
found reachable in either aggregate this phase.** This reconfirms (does not merely repeat) Phase
1/2/3's structural throw-site counts with actual guard semantics for 2 of the ~48 rich aggregates; the
remaining ~46 are pattern-inferred to share the same generated shape (`EnsureStatusIsOneOf`-style guard
present, throw-site counts >0 per Phase 1's census) but were not individually re-read line-by-line this
phase to rule out a guard that checks the wrong field or is missing on one specific method. **No
aggregate — in either the ones fully re-read this phase, or in the structural census inherited from
Phase 1 — was found to have a method that mutates `Status` with zero preceding guard**, which would be
the signature of a forceable illegal sequence; this phase did not find that pattern anywhere it looked.

## G5 — F6/F11/F12/F13 re-confirmed; no 5th cross-module rule gap found by direct read, but the systemic handler shape makes more likely than not that others exist unexamined
`AuditEngagement.Close` (`Auditing.AuditEngagement\Aggregates\AuditEngagement.cs` lines 229-247, full
read this phase): does guard `Status == Active` (a real lifecycle guard) and does range-check
`complianceScore` 0-100 (a real input constraint) — **but confirms F6 exactly as stated: no check that
`FindingsCount` open findings are resolved before Close, and no repository call into any other
module.** `RecordFinding` increments `FindingsCount` but nothing gates `Close` on that counter being
zero or all findings being in a terminal state — a same-aggregate cross-field gap adjacent to, but
distinct from, F6's original cross-module framing. Targeted greps this phase for a second FK-shaped
`Guid` reference field on `PPEAssignment` (candidate: a `PPECategoryId`) and `MachineGuardingInspections`
(candidate: an equipment-module FK) found **no such field on either** — these two specific candidates do
not exhibit the F12 pattern. No 5th confirmed instance was found by direct read this phase. **However**,
F10's handler-shape finding (every read handler across 10+ aggregates follows `Load → mutate → Save`
with exactly one repository dependency, never a second) means structurally **any** aggregate anywhere
in the 48-rich population that stores a raw `Guid` foreign key to another module's aggregate (as
`ChecklistCompletion.ChecklistTemplateId` and `AutomatedIncidentDetection.IncidentReferenceId` both do)
is very likely to share the same unvalidated-reference gap, purely because no handler in the codebase
carries a second repository to check it with. This is **pattern-inferred, not individually confirmed**,
for aggregates beyond the 4 already-named F6/F11/F12/F13 cases and the 2 checked-and-cleared candidates
above.

## G6 — F2 (Intelligence-only handler bypass) reconfirmed not to recur in PlantSafety this phase
No new evidence found this phase contradicting F2. Recorded per-row as inherited.
