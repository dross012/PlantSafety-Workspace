# Domain Mapping — SafetyPolicies

See `_shared-findings.md` §F1 (tenancy), §F3 (silent archive).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| SafetyPolicy | Create (Draft) | Starts `Draft` | Y (trivial) | Required fields | None | Not enforced (F1) | Directly verified (inherited) | |
| SafetyPolicy | SubmitForApproval | Must be `Draft` | Y (pattern-inferred) | — | None | Not enforced (F1) | Directly verified (inherited) | |
| SafetyPolicy | Approve | Must be `PendingApproval` | Y (pattern-inferred) | Approver identity required | None | Not enforced (F1) | Directly verified (inherited) | |
| SafetyPolicy | Reject | Must be `PendingApproval`; genuine "undo" transition, as well-guarded as the forward path (Phase 1 finding) | **Y** — confirmed | Rejection reason required (pattern-inferred, mirrors PermitWorkflow.Reject shape confirmed this phase) | None | Not enforced (F1) | Directly verified (inherited) | Positive control |
| SafetyPolicy | Publish | Must be `Approved` | Y (pattern-inferred) | — | None | Not enforced (F1) | Directly verified (inherited) | |
| SafetyPolicy | RecordAcknowledgment | Requires `Published` status (Phase 1 finding) | **Y** — confirmed | Acknowledging user identity required | None | Not enforced (F1) | Directly verified (inherited) | |
| SafetyPolicy | StartReviewCycle / CompleteReview | Paired guard | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| SafetyPolicy | Retire | Must be `Published`/reviewed (pattern-inferred) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited) | |
| SafetyPolicy | Archive | Must be `Retired` (pattern-inferred) | Guard pattern-inferred Y; event side **still only pattern-inferred**, not directly re-read this phase (unlike PermitTemplate/Dashboard/Operational.Alerts.Alert which were opened — F3) | None | None | Not enforced (F1) | Pattern-inferred (event); Directly verified (guard, inherited) | Suspected 3rd/4th silent-archive instance, still not conclusively confirmed either way |
| SafetyPolicy | Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

**Summary:** SafetyPolicy remains the best-guarded document-lifecycle aggregate (Reject as strong as
the forward path, RecordAcknowledgment gated on Published). Its Archive event-emission status is
the one open sub-question F3 did not fully close (file not opened this phase; recommended for Phase 3).
