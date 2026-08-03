# Domain Mapping — Analytics

See `_shared-findings.md` §F1 (tenancy). All 4 aggregates are Create-only markers; condensed
per-aggregate rows per the task's Create-only guidance.

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Culture (Analytics) | Create only | — | N/A | 1 throw, 2 events (uniform templated shape, Phase 1) | None | Not enforced (F1) | Directly verified | No confirmed SQL destination — candidate zero-SQL orphan; distinct from `SafetyCulture.CultureAssessment` |
| Engagement | Create only | — | N/A | Same shape | None | Not enforced (F1) | Directly verified | No confirmed SQL destination |
| PredictiveAnalytics | Create only | — | N/A | Same shape | None | Not enforced (F1) | Directly verified | SQL is read-only (`_List` only) — a `Create` call has no confirmed write path |
| ScoreCards (Analytics, dup of `Scorecards.Scorecard`) | Create only | — | N/A | Same shape | None | Not enforced (F1) | Directly verified | SQL ownership ambiguous (`usp_Scorecards_GetByCategory` may belong to the top-level aggregate); likely legacy/dead |

**Summary:** No new evidence this phase beyond Phase 1's file-name SQL census; all 4 remain
condensed, low-risk Create-only markers whose main open question (SQL destination) is unresolved
and would require opening each Create command handler's repository implementation to settle.
