# Module Inventory — SafetyPolicies

1 aggregate (`SafetyPolicy`). Inherited from ingestion discovery, SQL cross-refs confirmed this phase.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| SafetyPolicy | Create (Draft) | Yes | Yes — `usp_SafetyPolicy_Insert` (AuditLog-backed) | Live | Directly verified (inherited) | |
| SafetyPolicy | SubmitForApproval | Yes | Yes — `usp_SafetyPolicy_Update` | Live | Directly verified (inherited) | |
| SafetyPolicy | Approve | Yes | Yes — `usp_SafetyPolicy_Update` | Live | Directly verified (inherited) | |
| SafetyPolicy | Reject | Yes | Yes — `usp_SafetyPolicy_Update` | Live | Directly verified (inherited) | A genuine "undo" transition, as well-guarded as the forward path — a positive control |
| SafetyPolicy | Publish | Yes | Yes — `usp_SafetyPolicy_Update` | Live | Directly verified (inherited) | |
| SafetyPolicy | RecordAcknowledgment | Yes | Yes — child-table proc, not individually confirmed this phase | Live | Directly verified (inherited) | Requires `Published` status |
| SafetyPolicy | StartReviewCycle / CompleteReview | Yes | Yes — `usp_SafetyPolicy_Update` | Live | Directly verified (inherited) | |
| SafetyPolicy | Retire | Yes | Yes — `usp_SafetyPolicy_Update` | Live | Directly verified (inherited) | |
| SafetyPolicy | Archive | Yes | Yes — presumed folded into `usp_SafetyPolicy_Update` | Live | Pattern-inferred for event-publish (guard directly verified) | Suspected (not confirmed) third instance of the silent-archive pattern |
| SafetyPolicy | Cancel / Delete | **No** | N/A | N/A | Directly verified | |
