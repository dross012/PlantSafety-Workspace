# Module Inventory — Dashboards

`Dashboards.Dashboard`/`Widget` (rich) vs `Analytics.Dashboard` (create-only marker) — a **2nd confirmed
Dashboard-concept duplicate**, found this phase, alongside the prior discovery's Alert/Risk/Auditing
duplicates.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Dashboard (`Dashboards.Dashboard`) | Create / AddWidget / Archive / Publish / RemoveWidget / SetDefaultFilters | Yes (all 6, 4 guard + 6 event sites) | Yes — `usp_Dashboard_Reads`/`usp_Dashboard_Writes` (note: this module uses a Reads/Writes verb convention, not Create/Update — confirmed present under that naming, not a naive-grep false negative) | Pattern-inferred live | Directly verified (counts, this phase) | Has `Archive` — a 7th confirmed instance of that transition existing across the solution; not individually checked this phase for whether it also silently skips `AddDomainEvent` like Accident/Risk/Scorecard |
| Widget | Activate / Configure / Create / Deprecate | Yes (all 4, 4 guard + 5 event sites) | Yes — `usp_Widget_Reads`/`usp_Widget_Writes` | Pattern-inferred live | Directly verified (counts, this phase) | |
| Dashboard / Widget | Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| Dashboard (`Analytics.Dashboard`, marker) | Create only | Yes (marker aggregate, factory-only) | **Ambiguous** — file-name grep for `usp_Dashboard_*` cannot distinguish this class's SQL from the rich `Dashboards.Dashboard` above, since both are named `Dashboard` (same ambiguity flagged for the Alert duplicate pair) | Unconfirmed | Directly verified (method presence); SQL ambiguity noted this phase | If this marker shares the same `_Reads`/`_Writes` procs as the rich `Dashboards.Dashboard`, one of the two classes is likely writing to the other's table, or one is entirely dead — not resolved this phase, flagged for Phase 2 |
