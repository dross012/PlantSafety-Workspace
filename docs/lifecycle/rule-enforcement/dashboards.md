# Rule Enforcement Validation — Dashboards

See `_shared-findings.md`. `Dashboards.Dashboard`/`Widget` (rich) vs `Analytics.Dashboard` (create-only
marker, ambiguous SQL ownership per Phase 1).

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Dashboard — Create/AddWidget/Publish/RemoveWidget/SetDefaultFilters | Pass — 4 guard sites (inherited) | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `DashboardDapperRepository` confirmed hardcoded site | Medium | |
| Dashboard — Archive | Pass — status-guarded | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — same `DashboardDapperRepository` site | Medium | Not individually checked for the silent-archive pattern; 7th confirmed Archive-haver in the solution |
| Dashboard — Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| Widget — Activate/Configure/Create/Deprecate | Pass — 4 guard sites (inherited) | Unconfirmed | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `WidgetDapperRepository` confirmed hardcoded site | Medium | |
| Widget — Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| Dashboard (Analytics.Dashboard, marker) — Create only | Fail — marker only, minimal guard | Fail — SQL destination ambiguous/possibly shared with the rich `Dashboards.Dashboard`'s `_Reads`/`_Writes` procs; if so, calling this marker's Create risks writing into the rich aggregate's table under a different class's identity, or nowhere at all | N/A — not independently confirmed | N/A | Fail (F1, pattern) | Unconfirmed — repository not individually located | Low | Unresolved SQL-ownership ambiguity flagged again this phase; recommend Phase 5 resolve which class truly owns `usp_Dashboard_*` |
