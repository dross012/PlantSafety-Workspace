# Module Inventory — ContractorSafety

**Entirely new to this phase** — none of these 5 sub-aggregates (plus the separate `ContractorVisitor`)
were individually read by the prior Ingestion Discovery's ~45-aggregate sample. All 5 rich aggregates
follow an identical SQL pattern: `_Create`, `_GetAll`, `_GetById`, `_GetItems`, `_Update` — confirmed
present by exact-prefix match for every one. Confidence for guard/event *counts* is directly verified
(full-file scripted extraction, this phase); confidence for the *live-vs-legacy* status of each is
pattern-inferred only — no controller was read this phase to confirm which of these are actually wired
to an HTTP route, unlike the deep-read aggregates inherited from the prior discovery.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| ActiveMonitoringAuditing | Create / RecordObservation / Resolve / ScheduleFollowUp | Yes (all 4, 3 guard + 6 event sites) | Yes — `usp_ActiveMonitoringAuditing_Create`/`_Update` | Pattern-inferred live (not controller-confirmed) | Directly verified (counts, this phase) | |
| ActiveMonitoringAuditing | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| IncidentTrackingPerformanceEvaluation | Create / BeginInvestigation / DetermineEligibility / DocumentCapa | Yes (all 4, 3 guard + 6 event sites) | Yes — `usp_IncidentTrackingPerformanceEvaluation_Create`/`_Update` | Pattern-inferred live | Directly verified (counts, this phase) | No `Close`/`Resolve` found — same "raised but never formally closed" shape as `MachineProcessMonitoring` |
| IncidentTrackingPerformanceEvaluation | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| PermitToWork (ContractorSafety) | Create / RecordHazardAssessment / Issue / Suspend / Expire / Close | Yes (all 6, 4 guard + 7 event sites) | Yes — `usp_PermitToWork`-prefixed (not individually disambiguated from other Permit-concept SQL this phase) | Pattern-inferred live | Directly verified (counts, this phase) | **Flagged, not confirmed, as a possible 3rd overlapping concept with `PermitToWork.PermitWorkflow`** (see `permittowork.md`) — contractor-specific permit issuance may be a legitimately distinct bounded context (contractor permits vs. general work permits) rather than a true duplicate; top Phase 2 read priority for this cluster |
| PermitToWork (ContractorSafety) | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| PreQualificationVetting | Create / Approve / BeginReview / Expire / Reject | Yes (all 5, 3 guard + 6 event sites) | Yes — `usp_PreQualificationVetting_Create`/`_Update` | Pattern-inferred live | Directly verified (counts, this phase) | Approve/Reject pair present — same "positive undo path" shape as `SafetyPolicy` |
| PreQualificationVetting | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| SiteOrientation | Create / DeliverOrientation / Expire / VerifyComprehension | Yes (all 4, 4 guard + 5 event sites) | Yes — `usp_SiteOrientation_Create`/`_Update` | Pattern-inferred live | Directly verified (counts, this phase) | |
| SiteOrientation | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| ContractorVisitor | Create only | Yes | Not individually re-confirmed this phase (0 throws/0 events found, consistent with a bare marker) | Unconfirmed | Directly verified (method presence) | Update/Close/Cancel/Delete: missing entirely |
