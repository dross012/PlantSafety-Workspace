# SQL Shape Audit — PermitToWork

Phase 6 of 10. Covers `PermitWorkflow`, `PermitTemplate` (rich), `PermitExpiration`, `PermitScheduling`
(create-only). See `contractorsafety.md` for the possibly-related `ContractorSafety.PermitToWork`.

## Worst issues in this module

1. **`RejectPermitWorkflowCommand.Reason` — confirmed truncation risk, a 7th solution-wide instance,
   worse ratio than any other found this phase.** `RejectPermitWorkflowCommandValidator.cs:22`:
   `MaximumLength(1000)`. `PermitWorkflowDapperRepository.cs` persists it as `RejectedReason` at
   **`AddString("RejectedReason", aggregate.RejectedReason, 400)`** (confirmed at both the Reject write
   site, line 101, and re-persisted on a later write, line 149) — a validator-to-param gap of 600
   characters, the largest confirmed gap of any truncation-risk finding this phase (the other 6 are all
   a 56-char gap, 256-vs-200). A rejection reason between 401 and 1000 characters passes validation and
   is silently cut to 400 chars before the write. Directly verified (`RejectPermitWorkflowCommandValidator.
   cs`, `PermitWorkflowDapperRepository.cs` read in full for this field, including the read-side
   `RejectedReason` row property confirming the column is real and read back, not dead).
2. **`PermitWorkflow.Archive` is a suspected silent-archive instance** (inherited) — guard verified,
   event-publish not re-checked.
3. **`PermitTemplate.Archive` — newly found this phase, not yet individually checked for the silent-
   archive pattern** (inherited flag from `module-inventory/permittowork.md`).
4. **`PermitExpiration`/`PermitScheduling` both have a dormant `_Update` proc with no domain method using
   it** (inherited) — SQL capacity exists for a transition the domain model doesn't yet expose; not a
   shape bug, but worth noting the schema is ahead of the domain here, the reverse of most gaps in this
   audit.
5. **No CHECK constraint found on `PermitWorkflow`/`PermitTemplate`'s status columns.**

## Method

Direct read this phase: `RejectPermitWorkflowCommandValidator.cs`,
`PermitWorkflowDapperRepository.cs` (in full, including the `RejectedReason` write and read-back paths).
Other transitions' field-level shape pattern-inferred from `command-handler-map/_shared-findings.md` F10.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| PermitWorkflow.Create | Field set not re-opened this phase beyond the inherited trace | Not verified | Structurally possible, no CHECK found | Not individually re-verified this phase | Pattern-inferred | |
| PermitWorkflow.AssessHazards | Field set not re-opened (`riskScore`, `requiredControls` per inherited notes) | Not verified | Same | Not individually re-verified | Pattern-inferred | Guard + non-negative `riskScore` + non-empty `requiredControls` checks (inherited) |
| PermitWorkflow.ImplementControls | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PermitWorkflow.Approve | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PermitWorkflow.Issue | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PermitWorkflow.RecordClosureSignature | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PermitWorkflow.Reject | Yes — `RejectedReason` is genuinely persisted (unlike SafetyPolicies' dropped `Reason`) | Not verified | Same | **Yes — confirmed** (`Reason` 1000 validator vs `RejectedReason` 400 param, largest gap found this phase) | Directly verified | Worst-issue #1 |
| PermitWorkflow.Expire | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| PermitWorkflow.Archive | Field set not re-opened | Not verified | Same | Not individually re-verified | Directly verified (guard, inherited); pattern-inferred (event, shape) | Worst-issue #2 |
| PermitWorkflow.Cancel/Delete | N/A — transitions do not exist (Reject covers the negative path) | N/A | N/A | N/A | Directly verified | |
| PermitTemplate.Create | Field set not re-opened | Not verified | Same | Not individually re-verified | Directly verified (method presence, this phase); pattern-inferred (shape) | |
| PermitTemplate.Publish | Field set not re-opened | Not verified | Same | Not individually re-verified | Directly verified (method presence); pattern-inferred (shape) | |
| PermitTemplate.Deprecate | Field set not re-opened | Not verified | Same | Not individually re-verified | Directly verified (method presence); pattern-inferred (shape) | |
| PermitTemplate.Retire | Field set not re-opened | Not verified | Same | Not individually re-verified | Directly verified (method presence); pattern-inferred (shape) | |
| PermitTemplate.Archive | Field set not re-opened | Not verified | Same | Not individually re-verified | Directly verified (method presence); **unconfirmed** (silent-archive status) | Worst-issue #3 |
| PermitTemplate.Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
| PermitExpiration.Create only | Field set not re-opened | Not verified | N/A | Not individually re-verified | Directly verified (dormant Update proc, inherited) | Worst-issue #4 |
| PermitScheduling.Create only | Field set not re-opened | Not verified | N/A | Not individually re-verified | Directly verified (dormant Update proc, inherited) | Worst-issue #4 |
