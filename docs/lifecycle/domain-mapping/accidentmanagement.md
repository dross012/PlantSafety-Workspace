# Domain Mapping — AccidentManagement

See `_shared-findings.md` for F1 (tenancy), F2 (handler-bypass), F3 (silent-archive), F6 (cross-module).
1 aggregate: `Accident`. Rows below build on the Phase 1 inherited table with invariant/validation/
cross-module/tenancy analysis; guard bodies were read in full by the prior Ingestion Discovery and
re-confirmed at the summary level this phase (not re-opened line-by-line this phase except where noted).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Accident | Create (Report) | New record starts `Reported` | Y (constructor-only, trivially true) | Required fields (title/site/severity) via constructor args; no FluentValidation layer confirmed | None | Stated only — TenantId set at creation, not verified against caller (F1) | Directly verified (inherited) | |
| Accident | AssignInvestigator | Must be `Reported` to move to `UnderInvestigation` | **Y** (status guard confirmed, inherited) | Investigator UserId non-empty (pattern-inferred) | None | Not enforced (F1) | Directly verified (inherited) | |
| Accident | RecordRootCause | Must be `UnderInvestigation` | **Y** | Root-cause text non-empty (pattern-inferred) | None | Not enforced (F1) | Directly verified (inherited) | |
| Accident | CreateCorrectiveAction | Parent must be in an investigatable state (pattern-inferred) | Pattern-inferred Y | Action description required | None | Not enforced (F1) | Pattern-inferred | |
| Accident | CompleteCorrectiveAction | **Stated but NOT enforced**: does not re-check parent `Accident.Status` before completing a CAPA (Phase 1 finding) | **N** — confirmed gap | None confirmed | Should check parent Accident isn't already Closed/Archived — does not | Not enforced (F1) | Directly verified (inherited) | **Dangerous gap**: a CAPA can be completed against an Accident that has already moved past a state where that should be meaningful |
| Accident | FileRegulatoryReport | Regulatory filing fields required (pattern-inferred) | Pattern-inferred Y | Filing reference/date required | Should logically check `RegulatoryCalendar` module for filing deadline — **no such cross-module call found** | Not enforced (F1) | Directly verified (inherited) | Candidate new cross-module gap alongside F6, not confirmed by handler read this phase |
| Accident | CloseInvestigation | `RegulatoryFilingRequired` must be satisfied before Close | **Y** — best-guarded transition in the solution (Phase 1 finding, inherited) | Confirmed real cross-field invariant | Same-aggregate field only, not a genuine cross-module repository call | Not enforced (F1) | Directly verified (inherited) | Positive control — model for what other Close-type transitions should do |
| Accident | Archive (Closed→Archived) | Must be `Closed` | Y (guard), **N** for the event side — silent archive (Phase 1 finding, inherited) | None | None | Not enforced (F1) | Directly verified (inherited) | No downstream notification on archival — audit/retention consumers cannot react |
| Accident | Delete | N/A — missing entirely | N/A | N/A | N/A | N/A | Directly verified | Absence appropriate for a regulatory record |
| Accident | Cancel | N/A — missing entirely | N/A | N/A | N/A | N/A | Directly verified | No cancel/withdraw concept for a filed accident report |

**Summary for this file:** 1 of 8 real transitions (`CompleteCorrectiveAction`) has a stated invariant
that is confirmed **not** enforced — the single clearest "looks safe, isn't" case in this module.
Tenancy is unenforced across every transition per F1.
