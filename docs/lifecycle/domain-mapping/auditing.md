# Domain Mapping — Auditing

See `_shared-findings.md` §F1 (tenancy), §F6 (new cross-module gap — direct evidence this phase).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| AuditEngagement | Create | Starts `Planned`/`Scheduled` | Y (trivial) | Required fields | None | Not enforced (F1) | Directly verified (inherited) | Not in the 26-proc AuditLog list (Phase 1 finding) |
| AuditEngagement | Schedule | Must be `Planned` (pattern-inferred) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited, counts) | |
| AuditEngagement | Start | Must be `Scheduled` (pattern-inferred) | Pattern-inferred Y | — | None | Not enforced (F1) | Directly verified (inherited, counts) | |
| AuditEngagement | RecordFinding | Must be `InProgress` (pattern-inferred) | Pattern-inferred Y | Finding description required | None | Not enforced (F1) | Directly verified (inherited, counts) | |
| AuditEngagement | Close | Should verify findings resolved/compliance score reflects actual state | **N — confirmed gap this phase**: `CloseAuditEngagementCommandHandler` takes `ComplianceScore`/`ComplianceRating` as raw caller input, no query to Compliance module or check for open `AuditFinding`s (F6) | `ComplianceScore`/`ComplianceRating` bound from request body, no range/consistency validator confirmed | **Should** check Compliance module / unresolved findings — **does not** (F6, new this phase) | Not enforced (F1) | Directly verified (handler body read this phase) | **New dangerous finding**: a caller can Close an audit engagement with a fabricated/incorrect compliance score, nothing cross-checks it |
| AuditEngagement | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| AuditFinding | Create/Remediate/TriggerCapa/Verify | Standard remediation-cycle guards (5 guard + 5 event sites, Phase 1 counts) | Pattern-inferred Y (counts present; bodies not re-opened this phase) | Remediation plan/evidence fields required (pattern-inferred) | Should logically inform `AuditEngagement.Close` (see above) — the two aggregates do not appear cross-checked | Not enforced (F1) | Directly verified (inherited, counts) | Same AuditLog-insert gap as AuditEngagement |
| AuditFinding | Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| Auditing (Compliance duplicate marker) | Create only | No confirmed write proc (compliance.md) | N/A | None | None | Not enforced (F1) | Directly verified | Legacy/dead-ish, read-only SQL only |

**Summary:** `CloseAuditEngagementCommandHandler`'s unchecked `ComplianceScore`/`ComplianceRating`
input is this phase's clearest new "stated invariant not enforced" finding outside the tenancy issue —
a Close transition that should cross-reference `AuditFinding` state (and arguably the Compliance
module) but demonstrably does not.
