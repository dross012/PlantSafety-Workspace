# Domain Mapping — Dashboards

See `_shared-findings.md` §F1 (tenancy), §F3 (silent archive — resolved this phase for `Dashboard`).

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Dashboard | Create | Starts `Draft`/unpublished | Y (trivial) | — | None | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| Dashboard | AddWidget/RemoveWidget/SetDefaultFilters | Guarded (4 guard sites total across all 6 transitions, Phase 1 counts) | Pattern-inferred Y | Widget reference validity plausible | Should verify referenced `Widget` exists/is `Activated` — no confirmed cross-aggregate check (Dashboard and Widget are separate aggregates in this module) | Not enforced (F1) | Directly verified (counts, Phase 1) | Candidate cross-aggregate gap within the same module — not confirmed |
| Dashboard | Publish | Uses `DashboardLifecycleRules` helper class (confirmed this phase) | Pattern-inferred Y (helper class exists; exact rule body not opened) | — | None | Not enforced (F1) | Directly verified (helper class name, this phase) | |
| Dashboard | Archive | Guarded via `DashboardLifecycleRules.CanArchive(Status)`; **confirmed this phase to call `AddDomainEvent`** (`DashboardArchivedEvent`) — does NOT silently archive (F3) | **Y** — guard + event both confirmed | — | None | Not enforced (F1) | Directly verified (full method body, this phase) | Resolves Phase 1's flagged open question — 7th confirmed Archive instance, correctly instrumented |
| Dashboard/Widget | Cancel/Delete | N/A | N/A | N/A | N/A | N/A | Directly verified | Missing entirely |
| Widget | Activate/Configure/Create/Deprecate | 4 guard + 5 event sites | Pattern-inferred Y | Configuration schema plausible | None found | Not enforced (F1) | Directly verified (counts, Phase 1) | |
| Analytics.Dashboard (marker duplicate) | Create only | Factory-only | N/A | None | None | Not enforced (F1) | Directly verified (method presence, Phase 1) | SQL ambiguous — shares `usp_Dashboard_*` prefix pool with the rich `Dashboards.Dashboard`, not disambiguated; if it shares the same procs, one class may be silently writing into the other's table — still unresolved (would require opening the repository implementation, not attempted this phase) |

**Summary:** `Dashboard.Archive` is now confirmed non-silent (F3), the 2nd of 3 newly-found
Archive-havers resolved this phase. The `Analytics.Dashboard` vs `Dashboards.Dashboard` SQL-prefix
ambiguity remains open — recommend Phase 3 open the repository implementation for
`Analytics.Dashboard`'s Create command to see which proc it actually calls.
