# Module Inventory — Analytics

4 create-only marker aggregates under `PlantSafety.Domain.Analytics`: `Culture`, `Engagement`,
`PredictiveAnalytics`, `ScoreCards`. All 4 are single-method (`Create` + `ClearDomainEvents`) with
minimal guards (1 throw, 2 events each per this phase's scan — a uniform, likely-templated shape).

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Culture (Analytics) | Create only | Yes | **No SQL found** under exact or loose match for this class specifically (distinct from `SafetyCulture.CultureAssessment`, which has full SQL — see `safetyculture.md`) | Unconfirmed — candidate zero-SQL orphan | Directly verified | Do not confuse with `CultureAssessment` — genuinely different aggregate, different feature area |
| Engagement | Create only | Yes | **No SQL found** under exact or loose match | Unconfirmed — candidate zero-SQL orphan | Directly verified | |
| PredictiveAnalytics | Create only | Yes | Yes — **read-only**: `usp_PredictiveAnalytics_List` only, no write proc found | Unconfirmed | Directly verified | |
| ScoreCards (Analytics, duplicate of top-level `Scorecards.Scorecard`) | Create only | Yes | Ambiguous — `usp_Scorecards_GetByCategory` exists but the name convention (`Scorecards`, plural) suggests it may belong to the top-level `Scorecards.Scorecard` aggregate, not this marker — not disambiguated this phase | Unconfirmed — likely legacy/dead given the create-only shape and the live `Scorecards.Scorecard` elsewhere | Directly verified (method presence); SQL ownership unconfirmed | 3rd confirmed Scorecard-adjacent naming collision in the solution (after Alert, Dashboard) |
| All 4 above | Update/Close/Cancel/Delete | **No** — none exist | N/A | N/A | Directly verified | |
