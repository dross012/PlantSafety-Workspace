# Domain Mapping — ContractorSafety

See `_shared-findings.md` §F1 (tenancy — directly confirmed for this cluster via
`PermitToWorkDapperRepository`), §F2 (handler-bypass — directly confirmed absent for this cluster),
§F5 (PermitToWork duplicate cluster, resolved with direct evidence — see `permittowork.md` for the
side-by-side comparison; ContractorSafety.PermitToWork's own transition table is repeated there).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| ActiveMonitoringAuditing | Create/RecordObservation/Resolve/ScheduleFollowUp | Status-machine guards (3 guard + 6 event sites, Phase 1 counts) | Pattern-inferred Y (counts present; not individually re-opened this phase) | Observation description required (pattern-inferred) | None found | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| ActiveMonitoringAuditing | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| IncidentTrackingPerformanceEvaluation | Create/BeginInvestigation/DetermineEligibility/DocumentCapa | No `Close`/`Resolve` found — same "raised but never formally closed" shape as `MachineProcessMonitoring` (Phase 1 finding) | Pattern-inferred Y for the 4 that exist; **structurally incomplete lifecycle** — nothing to formally close | CAPA documentation fields required (pattern-inferred) | None found | Not enforced (F1) | Directly verified (counts, Phase 1) | Real lifecycle gap: an eligible/documented CAPA has no formal completion transition |
| IncidentTrackingPerformanceEvaluation | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| PermitToWork (ContractorSafety) | Create/RecordHazardAssessment/Issue/Suspend/Close/Expire | See `permittowork.md` — **full body read this phase**, all 6 transitions confirmed individually guarded | **Y** for all 6 (directly verified this phase) | Confined-space atmospheric-testing rule confirmed unique to this aggregate | None | **Confirmed unenforced this phase** — `PermitToWorkDapperRepository.GetByIdAsync` filters only by `PermitToWorkId`, never `TenantId` (F1, direct evidence) | Directly verified (full aggregate + repository read, this phase) | Also see F5 for the duplicate-cluster resolution vs. `PermitWorkflow` |
| PermitToWork (ContractorSafety) | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| PreQualificationVetting | Create/Approve/BeginReview/Expire/Reject | Approve/Reject pair present — "positive undo path" shape (Phase 1 finding) | Pattern-inferred Y (counts) | Vetting criteria fields required (pattern-inferred) | Should plausibly check contractor's `SiteOrientation`/training status before Approve — no such cross-aggregate call confirmed | Not enforced (F1) | Directly verified (counts, Phase 1) | Candidate cross-module gap, not confirmed by handler read this phase |
| PreQualificationVetting | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| SiteOrientation | Create/DeliverOrientation/Expire/VerifyComprehension | 4 guard + 5 event sites (Phase 1 counts) | Pattern-inferred Y | Comprehension score/threshold plausible (not confirmed) | Should plausibly gate `PreQualificationVetting.Approve` — no confirmed link | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| SiteOrientation | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| ContractorVisitor | Create only | — | N/A | 0 throws/0 events found (Phase 1) — thinnest guard shape in the cluster | None | Not enforced (F1) | Directly verified (method presence) | Update/Close/Cancel/Delete missing; no confirmed SQL write path either (see `emerging-createonly.md`) |

**Handler-bypass check (§F2) — direct evidence:** `ClosePermitToWorkCommandHandler`,
`ExpirePermitToWorkCommandHandler`, `IssuePermitToWorkCommandHandler`,
`RecordHazardAssessmentPermitToWorkCommandHandler`, `SuspendPermitToWorkCommandHandler` — all 5
read/grepped this phase — follow `Load → aggregate.Verb(...) → repository.UpdateAsync`. **No
Intelligence-style bypass found in this cluster.**

**Tenancy check (§F1) — direct evidence:** `PermitToWorkDapperRepository.GetByIdAsync` takes only
`PermitToWorkId`; no `TenantId` filter anywhere in the read or write path. Confirmed exploitable
pattern for this cluster specifically, generalized (same generator shape) to the rest of PlantSafety.

**Summary:** 1 new lifecycle-completeness gap confirmed (`IncidentTrackingPerformanceEvaluation` has
no formal close), 2 candidate (not confirmed) cross-module gaps flagged for Phase 3, tenancy
confirmed unenforced by direct repository read.
