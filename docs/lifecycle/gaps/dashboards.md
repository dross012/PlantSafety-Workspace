# Gaps — Dashboards

`Dashboards.Dashboard`/`Widget` (rich) vs `Analytics.Dashboard` (create-only marker duplicate). See `_legend.md`.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| Dashboard (`Dashboards.Dashboard`) | Cancel/Delete missing `[Inv]` | No (pattern-inferred) | **No** — directly confirmed non-silent: `Archive` calls `AddDomainEvent(DashboardArchivedEvent)`, one of 3 confirmed correctly-instrumented Archive-havers `[DM-F3]` | **Yes (pattern-inferred)** — classic shape `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation; uses a `_Reads`/`_Writes` SQL naming convention rather than Create/Update — confirmed present under that name, not a false-negative `[Inv]` | **Yes (pattern-inferred)** `[RLS§2]` |
| Widget | Cancel/Delete missing `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation | **Yes (pattern-inferred)** `[RLS§2]` |
| Dashboard (`Analytics.Dashboard`, marker, 2nd Dashboard-concept duplicate) | Update/Close/Cancel/Delete missing — Create-only `[Inv]` | No | N/A (inherent) | Unconfirmed — SQL ownership ambiguous with the rich `Dashboards.Dashboard` (same `usp_Dashboard_*` prefix pool) `[Inv]` | Unconfirmed — same ambiguity | Ambiguous SQL ownership — may be writing to the rich Dashboard's table or be entirely dead, not resolved `[Inv]` | Unconfirmed |

**Net for this module**: the 2nd confirmed Dashboard-concept duplicate in the solution (after the Alert pair) — genuinely useful information for Phase 9, since it means at least 3 duplicate-concept clusters (Alert, Risk/RiskManagement, Dashboard) share the identical "one rich live shape + one create-only or GET-only dead shape" pattern, suggesting a systemic scaffolding/generator habit rather than 3 independent incidents.
