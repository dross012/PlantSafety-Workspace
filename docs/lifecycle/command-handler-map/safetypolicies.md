# Command-Handler Map — SafetyPolicies

See `_shared-findings.md` for F7 (Outbox universal), F8 (`SafetyPolicy` **is** on the 23-aggregate
SQL-audit list), F9 (event delivery, [EVT-B]), F10 (naming convention). Command/handler names
pattern-inferred; not individually opened this phase.

| Transition | Command (pattern-inferred) | Handler (pattern-inferred) | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Create (Draft) | `CreateSafetyPolicyCommand` | `CreateSafetyPolicyCommandHandler` | `AddAsync` | `SafetyPolicyCreatedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| SubmitForApproval | `SubmitSafetyPolicyForApprovalCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | `SafetyPolicySubmittedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| Approve | `ApproveSafetyPolicyCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | `SafetyPolicyApprovedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| Reject | `RejectSafetyPolicyCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | `SafetyPolicyRejectedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | As well-guarded as the forward path (inherited) — a positive control |
| Publish | `PublishSafetyPolicyCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | `SafetyPolicyPublishedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| RecordAcknowledgment | `RecordSafetyPolicyAcknowledgmentCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` (child-table proc) | `SafetyPolicyAcknowledgmentRecordedEvent` (pattern-inferred) | Y | Unconfirmed — child-table proc not individually checked against F8's list this phase | Pattern-inferred | Requires `Published` status (inherited) |
| StartReviewCycle / CompleteReview | matching commands | matching handlers | `GetByIdAsync` → `UpdateAsync` | pattern-inferred names | Y | **Y** | Pattern-inferred | |
| Retire | `RetireSafetyPolicyCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | `SafetyPolicyRetiredEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| Archive | `ArchiveSafetyPolicyCommand` (existence unconfirmed) | matching handler | `GetByIdAsync` → `UpdateAsync` | **Suspected N — 3rd silent-archive instance, not directly confirmed this phase** (F3 leaves this one "presumed, not confirmed") | Suspected N | **Y** (SQL row still fires regardless per F8/`usp_SafetyPolicy_Update`) | Pattern-inferred (guard directly verified in prior phases; event-publish behavior still unconfirmed) | Still the single highest-value unresolved item in this module — a direct read of `SafetyPolicy.Archive`'s method body would close F3's last open case |
| Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Event delivery reality (F9 [EVT-B]):** all events on this aggregate — Created and non-Created alike
— have **zero consumer class of any kind** (no `SafetyPolicy*Consumer` found anywhere; the 22
"Created"-tier consumer list in `publishing-map.md` does not include a `SafetyPolicy` entry at all).
Net: zero live delivery for any transition in this module regardless of the Outbox column.
