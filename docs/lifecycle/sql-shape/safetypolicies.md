# SQL Shape Audit — SafetyPolicies

Phase 6 of 10. Covers `SafetyPolicy` (1 aggregate, live).

## Worst issues in this module

1. **`Reject`/`Retire`'s `Reason` field is captured, validated, and placed in the domain event — but is
   never written to the `SafetyPolicy` SQL table at all.** `SafetyPolicyLifecycleCommandValidators.cs`
   validates `Reason` (`NotEmpty().MaximumLength(1024)`, twice — once for Reject at line 55, once for
   Retire at line 134). `SafetyPolicyRejectedEvent`/`SafetyPolicyLifecycleEvents.cs`'s `Reason` field
   confirms the domain event carries it. But `SafetyPolicyDapperRepository.cs` — read in full for this
   phase's field-set check — has **zero `AddString("Reason", ...)` call and zero `Reason`-named column
   anywhere in `SafetyPolicy.sql`/`PolicyAcknowledgment.sql`**. **This is a genuine field-set mismatch of
   the "extra field silently ignored" kind the task specifically asked about**: the reason a policy was
   rejected or retired is fully validated and carried on the command/event, and then discarded before it
   ever reaches durable storage — it survives only in the domain event, which per F9 has zero live
   consumers. Querying `SafetyPolicy` directly for "why was this rejected" is impossible; the only
   surviving copy is buried in an `OutboxMessages.Payload` JSON blob nobody reads. Directly verified
   (`SafetyPolicyDapperRepository.cs` read in full, `SafetyPolicy.sql`/`PolicyAcknowledgment.sql` grepped
   for `Reason`, zero matches).
2. **`SafetyPolicyTitle` — no truncation risk, a positive control**: validator caps at 256, param size
   confirmed 400 (`AddString("SafetyPolicyTitle", ..., 400)`). Directly verified.
3. **`Archive` is a suspected (not confirmed) 4th silent-archive instance** (inherited) — guard directly
   verified, event-publish behavior not re-checked this phase.
4. **`Reject` is as well-guarded as the forward path** (inherited) — a genuine "undo" transition with
   real preconditions, a positive control at the guard-clause level even though its `Reason` field is
   lost at the persistence level (issue #1).
5. **No CHECK constraint found on `SafetyPolicy.Status`.**

## Method

Direct read this phase: `SafetyPolicyDapperRepository.cs` (in full), `SafetyPolicyLifecycleCommandValidators.
cs` (the `Reason` rules specifically), `SafetyPolicyLifecycleEvents.cs`/`SafetyPolicyApprovalEvents.cs`
(the `Reason` event fields), `SafetyPolicy.sql`/`PolicyAcknowledgment.sql` (grepped for `Reason`, zero
hits). This is the method that surfaced issue #1 — cross-referencing validator-field existence against
both the event-record shape (present) and the actual repository/table shape (absent) rather than
stopping at "does a validator exist."

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Create (Draft) | Yes — `SafetyPolicyTitle` matches `usp_SafetyPolicy_Insert` | Not individually re-verified | Structurally possible, no CHECK found | No — validator (256) stricter than param (400) | Directly verified | Worst-issue #2 |
| SubmitForApproval | Field set not re-opened beyond `Status` | Not verified | Same | N/A (no free-text field expected) | Pattern-inferred | |
| Approve | Same | Not verified | Same | N/A | Pattern-inferred | |
| Reject | **No — `Reason` is captured/validated but never persisted to any column** | N/A (field doesn't reach the column layer at all) | N/A | N/A (field never reaches a string param either — dropped before `AddString`) | Directly verified | Worst-issue #1 |
| Publish | Field set not re-opened | Not verified | Same | N/A | Pattern-inferred | |
| RecordAcknowledgment | Field set not re-opened (child-table proc) | Not verified | Same | Not individually re-verified | Pattern-inferred | Requires `Published` status (inherited) |
| StartReviewCycle / CompleteReview | Field set not re-opened | Not verified | Same | Not individually re-verified | Pattern-inferred | |
| Retire | **No — `Reason` is captured/validated but never persisted**, same as Reject | N/A | N/A | N/A | Directly verified | Worst-issue #1 (2nd instance, same root cause) |
| Archive | Field set not re-opened | Not verified | Same | Not individually re-verified | Directly verified (guard, inherited); pattern-inferred (event-publish, shape) | Worst-issue #3 |
| Cancel / Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
