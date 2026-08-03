# Rule Enforcement Validation — PermitToWork

See `_shared-findings.md`. `PermitWorkflow` (rich, fully re-read this phase — see G4), `PermitTemplate`
(rich), `PermitExpiration`/`PermitScheduling` (Create-only, batched).

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| PermitWorkflow — Create | Pass | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2) — `PermitWorkflowDapperRepository` confirmed hardcoded site | High | Audit-logged (F8) |
| PermitWorkflow — AssessHazards | **Pass** — guarded to `Requested` only (`EnsureStatusIsOneOf`, directly re-read this phase, G4); non-negative `riskScore` + non-empty `requiredControls` checked | Pass (inherited) | Pass | **Pass** — directly re-verified this phase: cannot be called except from `Requested` | Fail (F1) | Fail (G2) | **High — directly re-read in full this phase** | |
| PermitWorkflow — ImplementControls | Pass — guarded to `HazardAssessed` only | Pass (inherited) | Pass | **Pass** — directly re-verified | Fail (F1) | Fail (G2) | High | |
| PermitWorkflow — Approve | Pass — guarded to `ControlsImplemented` only | Pass (inherited) | Pass | **Pass** — calling `Approve` before `AssessHazards`/`ImplementControls` throws `PermitWorkflowInvalidStatusTransitionException`; directly attempted-to-break and confirmed guarded this phase | Fail (F1) | Fail (G2) | High | |
| PermitWorkflow — Issue | Pass — guarded to `Approved` only | Pass (inherited) | Pass | **Pass** | Fail (F1) | Fail (G2) | High | |
| PermitWorkflow — RecordClosureSignature | Pass — guarded to `Active` only | Pass (inherited) | Pass | **Pass** | Fail (F1) | Fail (G2) | High | |
| PermitWorkflow — Reject | Pass — guarded to 4 valid predecessor states; non-empty reason required | Pass (inherited) | Pass | **Pass** | Fail (F1) | Fail (G2) | High | Functions as this aggregate's Cancel |
| PermitWorkflow — Expire | Pass — guarded to `Active` only | Pass (inherited) | Pass | **Pass** | Fail (F1) | Fail (G2) | High | |
| PermitWorkflow — Archive | Pass — guarded to `Closed` only | Pass (inherited) | Pass | **Pass** | Fail (F1) | Unconfirmed — event-publish behavior not directly re-verified this phase for the silent-archive pattern | High (guard); Medium (event) | Suspected silent-archive instance still open |
| PermitWorkflow — Cancel(formal)/Delete | N/A — does not exist | N/A | N/A | N/A | N/A | N/A | High | `Reject` covers this informally |
| PermitTemplate — Create | Pass | Unconfirmed — validator not individually located this phase | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `PermitTemplateDapperRepository` confirmed hardcoded site | High (Actor/Tenancy); Medium (Constraints) | |
| PermitTemplate — Publish | Pass | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) | Medium | |
| PermitTemplate — Deprecate | Pass | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) | Medium | |
| PermitTemplate — Retire | Pass | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) | Medium | |
| PermitTemplate — Archive | Pass — status-guarded, **confirmed calls `AddDomainEvent`** (F3, event-emitting, not silent) | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | Correctly instrumented (F3), but still loses actor attribution via G2 |
| PermitTemplate — Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| PermitExpiration — Create (Create-only, batched) | Pass — factory-level guard only | Unconfirmed | Pass | N/A — no further transitions exist to sequence | Fail (F1, pattern) | Unconfirmed — repository not individually located this phase | Medium | Update/Close/Cancel/Delete missing entirely; dormant `_Update` proc exists unused |
| PermitScheduling — Create (Create-only, batched) | Pass — factory-level guard only | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Unconfirmed | Medium | Same dormant-`_Update` shape as PermitExpiration |
