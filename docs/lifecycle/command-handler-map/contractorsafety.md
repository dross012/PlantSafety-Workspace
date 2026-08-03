# Command-Handler Map — ContractorSafety

See `_shared-findings.md` for F7 (Outbox universal), F8 (none of these 6 aggregates on the
23-aggregate SQL-audit list), F9 (event delivery, [EVT-B]), F10, F2 (handler-bypass does NOT recur
here — confirmed by direct handler read of `ClosePermitToWorkCommandHandler`), and
`domain-mapping/_shared-findings.md` F1 (tenancy, confirmed via `PermitToWorkDapperRepository.GetByIdAsync`)
and F5 (this module's `PermitToWork` is distinct-but-overlapping vs. `PermitToWork.PermitWorkflow`,
see `permittowork.md`). `ContractorSafety.PermitToWork.cs` **fully directly verified this phase**
(confirmed all 6 methods correctly call `AddDomainEvent` — no silent transitions in this aggregate).
Command/handler names otherwise pattern-inferred (F10 consolidated-file convention presumed, not
individually opened for the other 4 aggregates).

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| ActiveMonitoringAuditing | Create / RecordObservation / Resolve / ScheduleFollowUp | matching commands | matching handlers | `AddAsync` (Create) / `GetByIdAsync`→`UpdateAsync` (rest, F1/F2 pattern confirmed for this cluster) | pattern-inferred names (6 event sites confirmed present, inherited) | Y | **N** | Pattern-inferred (guard/event counts directly verified, inherited) | |
| ActiveMonitoringAuditing | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| IncidentTrackingPerformanceEvaluation | Create / BeginInvestigation / DetermineEligibility / DocumentCapa | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | pattern-inferred names (6 event sites, inherited) | Y | **N** | Pattern-inferred | No `Close`/`Resolve` — same "raised but never formally closed" shape as `Observability.MachineProcessMonitoring` |
| IncidentTrackingPerformanceEvaluation | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| PermitToWork (ContractorSafety) | Create | `CreatePermitToWorkCommand` (pattern-inferred) | matching handler | `AddAsync` | `PermitToWorkDraftedEvent` — **confirmed raised, this phase's direct read** | **Y** | **N** | **Directly verified** | |
| PermitToWork (ContractorSafety) | RecordHazardAssessment | Command pattern-inferred | matching handler (F2: confirmed `Load→aggregate.Verb()→UpdateAsync`, not bypassed) | `GetByIdAsync` (id-only, F1 — no tenant check) → `UpdateAsync` | `PermitToWorkHazardAssessmentRecordedEvent` — **confirmed raised** | **Y** | **N** | **Directly verified** | |
| PermitToWork (ContractorSafety) | Issue | `ClosePermitToWorkCommandHandler`-family pattern (F1/F2 named example) | matching handler | `GetByIdAsync` → `UpdateAsync` | `PermitToWorkIssuedEvent` — **confirmed raised** | **Y** | **N** | **Directly verified** | |
| PermitToWork (ContractorSafety) | Suspend | Command pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | `PermitToWorkSuspendedEvent` — **confirmed raised** | **Y** | **N** | **Directly verified** | The unique transition F5 flags as having no equivalent in `PermitToWork.PermitWorkflow` — confirmed correctly instrumented, not silent |
| PermitToWork (ContractorSafety) | Close | `ClosePermitToWorkCommandHandler` (confirmed by name, F1) | `ClosePermitToWorkCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `PermitToWorkClosedEvent` — **confirmed raised** | **Y** | **N** | **Directly verified** | Named example in F1/F2 — confirmed to load by ID only (no tenant check) and to actually invoke the domain method (not bypassed, F2) |
| PermitToWork (ContractorSafety) | Expire | Command pattern-inferred | matching handler | `GetByIdAsync` → `UpdateAsync` | `PermitToWorkExpiredEvent` — **confirmed raised** | **Y** | **N** | **Directly verified** | |
| PermitToWork (ContractorSafety) | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| PreQualificationVetting | Create / Approve / BeginReview / Expire / Reject | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | pattern-inferred names | Y | **N** | Pattern-inferred | Approve/Reject pair — same "positive undo path" shape as `SafetyPolicy` |
| PreQualificationVetting | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| SiteOrientation | Create / DeliverOrientation / Expire / VerifyComprehension | matching commands | matching handlers | `AddAsync`/`GetByIdAsync`→`UpdateAsync` | pattern-inferred names | Y | **N** | Pattern-inferred | |
| SiteOrientation | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| ContractorVisitor | Create only | Command pattern-inferred | Unconfirmed | `AddAsync` — no SQL of any kind found (see `emerging-createonly.md`) | Unconfirmed | Unconfirmed | **N** | Directly verified (SQL absence, inherited) | |

**Event delivery reality (F9 [EVT-B]):** no consumer class of any kind exists for any event in this
module — confirmed no `ActiveMonitoringAuditing*Consumer`/`PermitToWork*Consumer`(ContractorSafety
variant)/`PreQualificationVetting*Consumer`/`SiteOrientation*Consumer` anywhere in
`publishing-map.md`/`consumption-map.md`. Net: zero live delivery for every transition, **including
all 6 of `PermitToWork`'s directly-confirmed-raised events** — the events are correctly constructed
and correctly outboxed, but nothing downstream ever reads them.
