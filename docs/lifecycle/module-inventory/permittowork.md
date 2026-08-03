# Module Inventory — PermitToWork

4 aggregates under `PlantSafety.Domain.PermitToWork`: `PermitWorkflow` and `PermitTemplate` (rich),
`PermitExpiration` and `PermitScheduling` (create-only). See `contractorsafety.md` for a 5th,
possibly-related `PermitToWork`-named aggregate under `ContractorSafety` — flagged as a distinct bounded
context, not confirmed as a duplicate of `PermitWorkflow`.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| PermitWorkflow | Create | Yes | Yes — `usp_PermitWorkflow_Insert` (AuditLog-backed) | Live | Directly verified (inherited) | |
| PermitWorkflow | AssessHazards | Yes | Yes — `usp_PermitWorkflow_Update` | Live | Directly verified (inherited) | Guard + non-negative `riskScore` + non-empty `requiredControls` checks |
| PermitWorkflow | ImplementControls | Yes | Yes — `usp_PermitWorkflow_Update` | Live | Directly verified (inherited) | |
| PermitWorkflow | Approve | Yes | Yes — `usp_PermitWorkflow_Update` | Live | Directly verified (inherited) | |
| PermitWorkflow | Issue | Yes | Yes — `usp_PermitWorkflow_Update` | Live | Directly verified (inherited) | |
| PermitWorkflow | RecordClosureSignature | Yes | Yes — `usp_PermitWorkflow_Update` | Live | Directly verified (inherited) | |
| PermitWorkflow | Reject | Yes | Yes — `usp_PermitWorkflow_Update` | Live | Directly verified (inherited) | Functions as this aggregate's "Cancel" — well-guarded |
| PermitWorkflow | Expire | Yes | Yes — `usp_PermitWorkflow_Update` | Live | Directly verified (inherited) | |
| PermitWorkflow | Archive | Yes | Yes — presumed folded into `usp_PermitWorkflow_Update` | Live | Pattern-inferred for event-publish (guard directly verified) | Suspected silent-archive instance |
| PermitWorkflow | Cancel (formal) / Delete | **No** | N/A | N/A | Directly verified | `Reject` covers the negative path informally |
| PermitTemplate | Create | Yes | Yes — `usp_PermitTemplate_Insert` | Live | Directly verified (structural scan, this phase) | |
| PermitTemplate | Publish | Yes | Yes — `usp_PermitTemplate_Update` | Live | Directly verified (counts, this phase) | |
| PermitTemplate | Deprecate | Yes | Yes — `usp_PermitTemplate_Update` | Live | Directly verified (counts, this phase) | |
| PermitTemplate | Retire | Yes | Yes — `usp_PermitTemplate_Update` | Live | Directly verified (counts, this phase) | |
| PermitTemplate | Archive | Yes | Yes — presumed folded into `usp_PermitTemplate_Update` | Live | Directly verified (method presence, this phase) | **[NEW]** not individually checked for the silent-archive pattern — do so in Phase 2 |
| PermitTemplate | Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| PermitExpiration | Create only | Yes | Yes — `usp_PermitExpiration_Insert`/`_Update` proc exists (Update proc present even though no domain method uses it yet — dormant capacity) | Live (Create only) | Directly verified | Update/Close/Cancel/Delete: **missing entirely** in the domain model |
| PermitScheduling | Create only | Yes | Yes — `usp_PermitScheduling_Insert`/`_Update` proc exists (same dormant-capacity note) | Live (Create only) | Directly verified | Update/Close/Cancel/Delete: **missing entirely** in the domain model |
