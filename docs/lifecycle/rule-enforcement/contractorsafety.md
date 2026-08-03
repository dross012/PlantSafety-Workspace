# Rule Enforcement Validation — ContractorSafety

See `_shared-findings.md`. 5 rich sub-aggregates + `ContractorVisitor` (create-only).

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| ActiveMonitoringAuditing — Create/RecordObservation/Resolve/ScheduleFollowUp | Pass — 3 guard sites confirmed present (counts, Phase 1) | Unconfirmed — validator not individually located | Pass — no other-module FK found | Pass (guard sites present; not individually re-attempted this phase to break) | Fail (F1, pattern) | Fail (G2) — `ActiveMonitoringAuditingDapperRepository` confirmed hardcoded site | Medium | |
| ActiveMonitoringAuditing — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | Does not exist |
| IncidentTrackingPerformanceEvaluation — Create/BeginInvestigation/DetermineEligibility/DocumentCapa | Pass — 3 guard sites | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) | Medium | **No Close/Resolve** — same "raised, never formally closed" gap as `Observability.MachineProcessMonitoring` |
| IncidentTrackingPerformanceEvaluation — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| PermitToWork (ContractorSafety) — Create/RecordHazardAssessment/Issue/Suspend/Expire/Close | Pass — 4 guard sites | Unconfirmed | Pass — no direct reference to `PermitToWork.PermitWorkflow` found; the two remain independently-modeled per F5 | Pass | Fail (F1, pattern) | Fail (G2) — `PermitToWorkDapperRepository` (ContractorSafety) confirmed hardcoded site | Medium | F5's overlap risk is a data-integrity/reporting gap, not a runtime cross-module call gap — no shared identity exists to fail a check against |
| PermitToWork (ContractorSafety) — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| PreQualificationVetting — Create/Approve/BeginReview/Expire/Reject | Pass — 3 guard sites; Approve/Reject "undo" pair present | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) | Medium | |
| PreQualificationVetting — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| SiteOrientation — Create/DeliverOrientation/Expire/VerifyComprehension | Pass — 4 guard sites (highest density in this cluster) | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `SiteOrientationDapperRepository` confirmed hardcoded site | Medium | |
| SiteOrientation — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| ContractorVisitor — Create only (batched) | Pass — bare marker, minimal guard | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Unconfirmed | Low | Update/Close/Cancel/Delete missing entirely |
