# Module Inventory — Auditing

Live cluster: `Auditing.AuditEngagement`/`AuditFinding`. Duplicate: `Compliance.Auditing.Auditing`
(covered fully in `compliance.md`, referenced here for completeness).

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| AuditEngagement | Create | Yes | Yes — `usp_AuditEngagement_Create` | Live | Directly verified (inherited) | **Not** in the 26-proc AuditLog list — a full state machine that never got the AuditLog insert 26 sibling aggregates have |
| AuditEngagement | Schedule / Start / RecordFinding / Close | Yes (all 4, 6 guard sites total) | Yes — `usp_AuditEngagement_Update` | Live | Directly verified (counts, inherited) | |
| AuditEngagement | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| AuditFinding | Create / Remediate / TriggerCapa / Verify | Yes (all 4, 5 guard + 5 event sites) | Yes — `usp_AuditFinding_Create`/`_Update` | Live | Directly verified (counts, inherited) | Same AuditLog-insert gap as AuditEngagement |
| AuditFinding | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| Auditing (Compliance duplicate marker) | Create only | Yes | Read-only (`usp_Auditing_GetLogs` only — see `compliance.md`) | **Legacy/dead-ish** | Directly verified | One of the 22 no-op-consumer pairs; own controller tree distinct from `AuditEngagementsV1Controller`/`AuditFindingsV1Controller` |

**Audit-logging and rich-lifecycle are independent axes, reconfirmed this phase**: the live, richly
guarded `AuditEngagement`/`AuditFinding` pair was never given the cross-DB AuditLog insert the 26-proc
list has for Accident/Risk/SafetyPolicy/etc., while the create-only, likely-dead `Compliance.Auditing`
duplicate at least has a `GetLogs` read proc. Do not assume "has an AuditLog-sounding name" correlates
with "actually audit-logged."
