# Event Map — SafetyPolicies

See `_shared-findings.md` (F14-F17) and `command-handler-map/safetypolicies.md`. **Fully directly
verified this phase**: read of `SafetyPolicyCreatedEvent.cs`, `SafetyPolicyApprovalEvents.cs`
(`SafetyPolicySubmittedForApprovalEvent`, `SafetyPolicyApprovedEvent`, `SafetyPolicyRejectedEvent`),
`SafetyPolicyLifecycleEvents.cs` (`SafetyPolicyPublishedEvent`, `SafetyPolicyAcknowledgedEvent`,
`SafetyPolicyReviewCycleStartedEvent`, `SafetyPolicyReviewCompletedEvent`,
`SafetyPolicyRetiredEvent`). The aggregate body (`SafetyPolicy.cs`) itself was not re-opened this
phase (pattern-inferred 1:1 `AddDomainEvent` mapping, consistent with every other aggregate checked).

| Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Create (Draft) | Pattern-inferred Y | `SafetyPolicyCreatedEvent` | Y — identity/actor/correlation only | **N — no `SafetyPolicy*Consumer` of any kind found** | Pattern-inferred (event body directly read) | |
| SubmitForApproval | Pattern-inferred Y | `SafetyPolicySubmittedForApprovalEvent` | Y — `SubmittedBy (UserId), SubmittedAtUtc` | **N** | Pattern-inferred (event body directly read) | |
| Approve | Pattern-inferred Y | `SafetyPolicyApprovedEvent` | Y — `ApprovedBy (UserId), ApprovedAtUtc` — **exactly the "who approved it" field the task flagged as commonly missing; it is present here** | **N** | Pattern-inferred (event body directly read) | A positive result against the task's stated hypothesis |
| Reject | Pattern-inferred Y | `SafetyPolicyRejectedEvent` | Y — `RejectedBy (UserId), Reason (string), RejectedAtUtc` — fully complete, including the free-text reason | **N** | Pattern-inferred (event body directly read) | As well-guarded and as well-instrumented as the forward path (inherited) |
| Publish | Pattern-inferred Y | `SafetyPolicyPublishedEvent` | Y — `EffectiveAtUtc, NextReviewDueUtc (DateTime)` — publishing carries forward the next review date, a genuinely useful downstream field | **N** | Pattern-inferred (event body directly read) | |
| RecordAcknowledgment | Pattern-inferred Y | `SafetyPolicyAcknowledgedEvent` | Y — `AcknowledgedBy (UserId), PolicyVersion (int), TotalAcknowledgmentCount (int), AcknowledgedAtUtc` — carries a running tally, useful for a compliance-rate consumer | Audit Entry unconfirmed per command-handler-map (child-table proc); **event consumer: N** | Pattern-inferred (event body directly read) | Requires `Published` status (inherited guard) |
| StartReviewCycle | Pattern-inferred Y | `SafetyPolicyReviewCycleStartedEvent` | Y — `StartedBy (UserId), StartedAtUtc` | **N** | Pattern-inferred (event body directly read) | |
| CompleteReview | Pattern-inferred Y | `SafetyPolicyReviewCompletedEvent` | Y (**F15 caveat**) — `ReviewedBy (UserId), NewPolicyVersion (int), NextReviewDueUtc, ReviewedAtUtc` — carries the new version number but **not the version being superseded**, so a consumer cannot tell from this event alone what changed between v3 and v4 | **N** | Pattern-inferred (event body directly read) | |
| Retire | Pattern-inferred Y | `SafetyPolicyRetiredEvent` | Y — `RetiredBy (UserId), Reason (string), RetiredAtUtc` | **N** | Pattern-inferred (event body directly read) | |
| Archive | **Suspected N — 3rd silent-archive candidate, still not directly confirmed** (F3 leaves this open; no `SafetyPolicyArchivedEvent` class was found in the Events folder during this phase's directory listing, which is itself suggestive evidence the method — if it exists — has no event to raise) | N/A (no event class found) | N/A | N/A | Pattern-inferred (event-class absence directly observed this phase, strengthening but not fully closing F3's open item) | The event-class absence is a stronger signal than the prior phases had: `SafetyPolicies/Events/` contains no `Archived`-named file at all (grep-confirmed, this phase), whereas `PermitTemplate`/`Dashboard`/`Alert` (Operational) — the 3 confirmed non-silent Archives — each have a dedicated `*ArchivedEvent.cs` file. The absence of that file here is consistent with (though not 100% proof of) SafetyPolicy's Archive being silent |
| Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |

**Duplicate-publish check (F17):** not re-opened this phase for `SafetyPolicy.cs`'s method bodies;
no evidence of a duplicate found in the event-class inventory (9 distinct event types for 9
transitions, 1:1 by name, consistent with no double-publish).

**Module-level summary:** 8 of 9 non-Archive/non-Create-missing transitions publish with complete,
well-formed payloads (F14) — this module has the best "who did it" attribution of any file reviewed
this phase (every state-changing event names its actor by role, not just the generic `Actor` field).
The one gap is F15 (`CompleteReview` cannot show the diff between versions) plus the still-open
Archive question. Zero live delivery for any transition (F9) — no `SafetyPolicy*Consumer` class
exists anywhere in the solution.
