# Event Map — ContractorSafety (ActiveMonitoringAuditing, IncidentTrackingPerformanceEvaluation, PermitToWork, PreQualificationVetting, SiteOrientation, ContractorVisitor)

See `_shared-findings.md` (F14-F17) and `command-handler-map/contractorsafety.md` (esp. F5 —
distinct-but-overlapping vs. `PermitToWork.PermitWorkflow`). **Fully directly verified this phase**:
read of all 6 `PermitToWork` (ContractorSafety) events.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| PermitToWork (ContractorSafety) | Create | Y | `PermitToWorkDraftedEvent` | Y — adds `PermitType (PermitType)` beyond identity/actor | **N** | Directly verified | |
| PermitToWork (ContractorSafety) | RecordHazardAssessment | Y | `PermitToWorkHazardAssessmentRecordedEvent` | Y — `HazardCount (int), AtmosphericTestScore (decimal?)` — a genuinely complete payload, the atmospheric test score is a real safety-relevant field | **N** | Directly verified | |
| PermitToWork (ContractorSafety) | Issue | Y | `PermitToWorkIssuedEvent` | Y — `AuthorizedByUserId (UserId), ExpiresAtUtc` — names the authorizer and forwards the expiry, on par with `PermitToWork.PermitWorkflow`'s equivalent event | **N** | Directly verified | |
| PermitToWork (ContractorSafety) | Suspend | Y | `PermitToWorkSuspendedEvent` | Y — `Reason (string)` | **N** | Directly verified | The unique transition F5 flags as having no equivalent in `PermitToWork.PermitWorkflow` — confirmed correctly instrumented, not silent |
| PermitToWork (ContractorSafety) | Close | Y | `PermitToWorkClosedEvent` | Y (**F15 caveat**) — identity/timestamp/actor only, no closure notes/signature field (unlike `PermitWorkflow.RecordClosureSignature`'s richer `ClosureSignedBy`+`ClosureNotes` pair in the other PermitToWork-named aggregate — a real payload-richness gap between the two "distinct but overlapping" bounded contexts F5 flags) | **N** | Directly verified | |
| PermitToWork (ContractorSafety) | Expire | Y | `PermitToWorkExpiredEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | |
| PermitToWork (ContractorSafety) | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| ActiveMonitoringAuditing | Create / RecordObservation / Resolve / ScheduleFollowUp | Pattern-inferred Y (6 event sites confirmed present by count, inherited) | Named per convention, not individually opened this phase | Pattern-inferred Y per F14 | **N** | Pattern-inferred | |
| ActiveMonitoringAuditing | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| IncidentTrackingPerformanceEvaluation | Create / BeginInvestigation / DetermineEligibility / DocumentCapa | Pattern-inferred Y (6 event sites confirmed present, inherited) | Named per convention, not individually opened this phase | Pattern-inferred Y per F14 | **N** | Pattern-inferred | No `Close`/`Resolve` — same "raised but never formally closed" shape as `Observability.MachineProcessMonitoring`'s `RaiseAnomalyFlag` |
| IncidentTrackingPerformanceEvaluation | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| PreQualificationVetting | Create | Pattern-inferred Y — **event name confirmed this phase**: `PreQualificationVettingSubmittedEvent` (not `...CreatedEvent` as command-handler-map's naming convention assumed) | `PreQualificationVettingSubmittedEvent` | Y — `ContractorName (string)` — a genuinely useful identifying field for a Create-shaped event, better than the bare-identity norm | **N** | Directly verified (event body/name) | Naming correction: this phase's direct read shows the class is literally named `Submitted`, not `Created` — the aggregate's own vocabulary treats "Create" as "Submit for vetting" |
| PreQualificationVetting | Approve / BeginReview / Expire / Reject | Pattern-inferred Y | Named per convention, not individually opened this phase | Pattern-inferred Y per F14 | **N** | Pattern-inferred | Approve/Reject pair — same "positive undo path" shape as `SafetyPolicy` |
| PreQualificationVetting | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| SiteOrientation | Create / DeliverOrientation / Expire / VerifyComprehension | Pattern-inferred Y | Named per convention, not individually opened this phase | Pattern-inferred Y per F14 | **N** | Pattern-inferred | |
| SiteOrientation | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| ContractorVisitor | Create only | Unconfirmed | Event class exists (`ContractorVisitorCreatedEvent`, confirmed by file listing) | Pattern-inferred Y per F14 (not opened) | **N** | Directly verified (SQL absence, inherited); event body not opened | No SQL of any kind found — event would write to Outbox regardless of whether the underlying persistence succeeds |

**Duplicate-publish check (F17):** not individually re-opened for the 4 pattern-inferred aggregates
this phase; `PermitToWork`'s 6 events map 1:1 to its 6 transitions (directly confirmed) — no
duplicates found there.

**Module-level summary:** `PermitToWork` (ContractorSafety)'s payloads are complete and, for
`RecordHazardAssessment`/`Issue`/`Suspend`, genuinely rich — but its `Close` event is noticeably
thinner than its `PermitToWork.PermitWorkflow` counterpart (see `permittowork.md`), a concrete
payload-richness asymmetry between the two "distinct but overlapping" F5 bounded contexts.
`PreQualificationVetting`'s Create event is actually named `Submitted`, correcting the naming
assumption carried in `command-handler-map/contractorsafety.md`. Zero live delivery for any of the
~30+ transitions in this module (F9) — no consumer class of any kind exists for any aggregate here.
