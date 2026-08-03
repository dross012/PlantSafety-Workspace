# Module Inventory — SafetyCulture

2 aggregates: `CultureAssessment`, `StopWorkEvent`. See `analytics.md` for a create-only,
non-lifecycle-related `Culture` marker under `Analytics` — same root word, different concept (an
Analytics rollup, not a culture-survey instrument), not treated as a duplicate.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| CultureAssessment | Create | Yes | Yes — `usp_CultureAssessment_Create` | Live | Directly verified (inherited) | |
| CultureAssessment | RecordScore | Yes | Yes — `usp_CultureAssessment_Update` | Live | Directly verified (inherited) | Real range-check (0-100) on both score fields — a positive control |
| CultureAssessment | PublishReport | Yes | Yes — `usp_CultureAssessment_Update` | Live | Directly verified (guard presence, inherited); pattern-inferred (exact rule) | |
| CultureAssessment | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
| StopWorkEvent | Create / Investigate / Resolve | Yes (all 3, 3 guard + 4 event sites) | Yes — `usp_StopWorkEvent_Create`/`_Update` | Live | Directly verified (counts, inherited) | |
| StopWorkEvent | Archive / Cancel / Delete | **No** | N/A | N/A | Directly verified | |
