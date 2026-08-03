# Module Inventory — AccidentManagement

Feature area: `PlantSafety.Domain.AccidentManagement`, controller `PlantSafety.Api\AccidentManagement`.
1 aggregate (`Accident`). All rows below inherited from the Ingestion Discovery's directly-verified
deep read (`docs/ingestion/lifecycle-coverage/plantsafety-lifecycle.md`), reformatted to this phase's
per-transition schema; not independently re-read this phase.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Accident | Create (Report Accident) | Yes | Yes — `usp_Accident_Insert` (writes AuditDB.AuditLog) | Live | Directly verified | |
| Accident | AssignInvestigator (Reported→UnderInvestigation) | Yes | Yes — `usp_Accident_Update` | Live | Directly verified | |
| Accident | RecordRootCause (UnderInvestigation→RootCauseIdentified) | Yes | Yes — `usp_Accident_Update` | Live | Directly verified | |
| Accident | CreateCorrectiveAction | Yes | Yes — `usp_Accident_Update` (child entity) | Live | Directly verified | |
| Accident | CompleteCorrectiveAction | Yes | Yes — `usp_Accident_Update` | Live | Directly verified | Does not re-check parent `Accident.Status` before completing a CAPA — minor gap |
| Accident | FileRegulatoryReport | Yes | Yes — `usp_Accident_Update` | Live | Directly verified | |
| Accident | CloseInvestigation | Yes | Yes — `usp_Accident_Update` | Live | Directly verified | Best-guarded transition in the solution — enforces `RegulatoryFilingRequired` before allowing Close, a real cross-field invariant |
| Accident | Archive (Closed→Archived) | Yes | Yes — presumed folded into `usp_Accident_Update` (no dedicated archive proc found) | Live | Directly verified | Guarded but never calls `AddDomainEvent` — silent archival, no downstream notification |
| Accident | Delete / hard-delete | **No** — missing entirely | N/A | N/A | Directly verified | Absence appears appropriate for a regulatory record, not a gap |
| Accident | Cancel | **No** — missing entirely | N/A | N/A | Directly verified | No cancel/withdraw concept exists for a filed accident report |

**Confirmed live, single implementation** — no duplicate `Accident` aggregate found anywhere in the
solution this phase (unlike Alert/Risk/Auditing/Dashboard/Checklists, see their respective files).
