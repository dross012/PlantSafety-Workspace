# Domain Mapping — PermitToWork

See `_shared-findings.md` §F1 (tenancy), §F3 (silent archive), §F5 (duplicate-cluster resolution —
direct evidence this phase).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| PermitWorkflow | Create | Starts `Requested` | Y (trivial) | Trims text fields; no FluentValidation confirmed | None | Not enforced (F1) | Directly verified (this phase, full body) | |
| PermitWorkflow | AssessHazards | Must be `Requested` | **Y** (confirmed `EnsureStatusIsOneOf` call) | `RequiredControls` non-empty; `RiskScore >= 0` | None | Not enforced (F1) | Directly verified (this phase, full body) | |
| PermitWorkflow | ImplementControls | Must be `HazardAssessed` | **Y** | `ControlsConfirmedBy` non-empty | None | Not enforced (F1) | Directly verified (this phase) | |
| PermitWorkflow | Approve | Must be `ControlsImplemented` | **Y** | — | None | Not enforced (F1) | Directly verified (this phase) | |
| PermitWorkflow | Issue | Must be `Approved` | **Y** | `validityHours` optional; if present used for expiry calc | None | Not enforced (F1) | Directly verified (this phase) | |
| PermitWorkflow | RecordClosureSignature (Close) | Must be `Active` | **Y** | — | None | Not enforced (F1) | Directly verified (this phase) | Functions as this aggregate's Close |
| PermitWorkflow | Reject | Must be one of Requested/HazardAssessed/ControlsImplemented/Approved | **Y** — multi-state guard confirmed | Reason non-empty | None | Not enforced (F1) | Directly verified (this phase) | Functions as this aggregate's Cancel |
| PermitWorkflow | Expire | Must be `Active` | **Y** | — | None | Not enforced (F1) | Directly verified (this phase) | |
| PermitWorkflow | Archive | Must be `Closed` | **Y** (guard confirmed); event side **not directly re-opened this phase** — still pattern-inferred | — | None | Not enforced (F1) | Directly verified (guard); pattern-inferred (event) | Suspected silent-archive, not conclusively resolved |
| PermitWorkflow | Cancel(formal)/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | `Reject` covers the negative path informally |
| PermitTemplate | Create/Publish/Deprecate/Retire | Standard document lifecycle | Pattern-inferred Y (counts) | — | None | Not enforced (F1) | Directly verified (inherited/this phase counts) | |
| PermitTemplate | Archive | Must be `Retired` (pattern-inferred); **confirmed this phase to call `AddDomainEvent`** (`PermitTemplateArchivedEvent`) — does NOT silently archive (F3) | **Y** — guard + event both confirmed | — | None | Not enforced (F1) | Directly verified (full method body, this phase) | Resolves Phase 1's flagged open question for this aggregate |
| PermitTemplate | Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| PermitExpiration | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified | Update/Close/Cancel/Delete missing; dormant Update proc exists in SQL |
| PermitScheduling | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified | Same dormant-Update shape |
| ContractorSafety.PermitToWork | Create (Draft) | Starts `Draft` | Y (trivial) | Trims text fields | None | Not enforced (F1) | Directly verified (this phase, full body) | See F5 — distinct state machine from PermitWorkflow above |
| ContractorSafety.PermitToWork | RecordHazardAssessment | Must be `Draft`; **atmospheric test score required if `RequiresAtmosphericTesting`** (confined-space-specific rule `PermitWorkflow` has no equivalent of) | **Y** — confirmed, including the confined-space-specific branch | `hazardCount >= 0`; atmospheric score required conditionally | None | Not enforced (F1) | Directly verified (this phase, full body) | Genuine domain rule unique to this aggregate |
| ContractorSafety.PermitToWork | Issue | Must be `HazardAssessed` | **Y** | `validityHours > 0` (stricter than PermitWorkflow's optional validity) | None | Not enforced (F1) | Directly verified (this phase) | |
| ContractorSafety.PermitToWork | Suspend | Must be `Issued` — **a transition PermitWorkflow has no equivalent of at all** (F5) | **Y** | Reason captured, no non-empty check found (weaker than Reject's reason check) | None | Not enforced (F1) | Directly verified (this phase, full body) | Confirms F5: genuinely different capability, not a redundant copy |
| ContractorSafety.PermitToWork | Close | Must be `Issued` or `Suspended` | **Y** | — | None | Not enforced (F1) | Directly verified (this phase) | |
| ContractorSafety.PermitToWork | Expire | Must be `Issued` | **Y** | — | None | Not enforced (F1) | Directly verified (this phase) | |
| ContractorSafety.PermitToWork | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |

**Summary:** This is the best-verified feature area this phase — both `PermitWorkflow` and
`ContractorSafety.PermitToWork` full aggregate bodies were read directly. Both are well-guarded
internally (every transition has a real `EnsureStatusIsOneOf` check); the duplicate-cluster question
(F5) is resolved as "genuine overlap, divergent state machines, no shared identity" rather than
either "confirmed duplicate" or "false positive." `PermitTemplate.Archive` is confirmed
non-silent, narrowing the outstanding silent-archive uncertainty to `PermitWorkflow.Archive` and
`SafetyPolicy.Archive` only.
