# Module Inventory — Checklists

Two `Checklist`-concept aggregates: `Checklists.ChecklistTemplate`/`ChecklistCompletion` (rich, live)
and `Operational.Checklists.Checklists` (create-only marker, read-only SQL — same dead-duplicate shape
confirmed elsewhere in `Operational.*`).

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| ChecklistTemplate | Create | Yes | Yes — `usp_ChecklistTemplate_Create` | Live | Directly verified (inherited) | |
| ChecklistTemplate | AddItem | Yes | Yes — `usp_ChecklistTemplate_Update` | Live | Directly verified (guard, inherited); pattern-inferred (event) | |
| ChecklistTemplate | Publish | Yes | Yes — `usp_ChecklistTemplate_Update` | Live | Directly verified (inherited) | Real rule: rejects publishing an empty template (zero items) |
| ChecklistTemplate | Retire | Yes | Yes — `usp_ChecklistTemplate_Update` | Live | Directly verified (guard, inherited) | |
| ChecklistTemplate | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| ChecklistCompletion | Create | Yes | Yes — `usp_ChecklistCompletion_Create` | Live | Directly verified (inherited) | |
| ChecklistCompletion | Start | Yes | Yes — `usp_ChecklistCompletion_Update` | Live | Directly verified (inherited) | |
| ChecklistCompletion | RecordItemResult | Yes | Yes — `usp_ChecklistCompletion_Update` | Live | Directly verified (inherited) | |
| ChecklistCompletion | FlagFailedItem | Yes | Yes — `usp_ChecklistCompletion_Update` | Live | Pattern-inferred (guard not independently confirmed) | |
| ChecklistCompletion | Complete | Yes | Yes — `usp_ChecklistCompletion_Update` | Live | Directly verified (inherited) | |
| ChecklistCompletion | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| Checklists (Operational duplicate) | Create only | Yes (marker aggregate) | **Read-only** — only `usp_Checklists_List` found; no `_Insert`/`_Create`/`_Update` under this name | **Legacy/dead-ish** | Directly verified this phase (SQL check new) | Same dead-duplicate signature confirmed this phase for `Operational.RiskManagement` and `Compliance.Auditing` — a recurring shape across the whole `Operational.*`/`Compliance.*` marker population, not an isolated case |
