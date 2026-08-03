# Recommendations — Dashboards

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. `Dashboards.Dashboard`
is one of only 3 confirmed non-silent Archive-havers in the solution (a genuine positive control to
preserve) — this module's real gap is the duplicate-concept cluster, not a defect in the rich
aggregate itself.

1. **Resolve the `Dashboards.Dashboard`/`Widget` vs. `Analytics.Dashboard` duplicate-concept
   cluster** — the 2nd confirmed Dashboard-concept duplicate in the solution (after Alert), sharing
   the identical "one rich live shape + one Create-only or GET-only dead shape" pattern seen in the
   Alert/Risk/Scorecard clusters, which suggests a systemic scaffolding/generator habit rather than
   independent incidents. SQL ownership between the two `Dashboard` concepts is ambiguous (same
   `usp_Dashboard_*` prefix pool). Fix: confirm which table(s) `Analytics.Dashboard` actually writes
   to (if any) before assuming it's simply dead — unlike the Alert/Risk pairs, this one's SQL
   ownership wasn't independently resolved this audit. If confirmed dead, formally
   deprecate/remove, coordinating with the equivalent items in `analytics.md`/`riskmanagement.md`/
   `alerts.md` as one solution-wide duplicate-concept cleanup effort rather than 4 separate
   decisions. Cite: `gaps/_severity-ranking.md` #13, `domain-mapping/_shared-findings.md` F4-adjacent,
   `gaps/dashboards.md`, `gaps/analytics.md`. Effort: M (mostly deletion + a decommission plan, since
   SQL tables are involved — potentially larger if the SQL-ownership investigation surfaces the two
   sharing a table). Risk: **Tech-debt today**, a live landmine for future development otherwise.
   Sequencing: coordinate with the cross-module duplicate-concept cleanup effort; do the SQL-ownership
   investigation before deleting anything, since this pair's ambiguity is less resolved than the
   other 3 duplicate clusters.

2. **Preserve and use `Dashboard`'s correctly-instrumented Archive as the template for fixing the
   other 3 silent-Archive instances** (`Accident`, `Risk`, `Scorecard`). `Dashboard.Archive` calls
   `AddDomainEvent(DashboardArchivedEvent)` correctly — this is a positive-control finding worth
   pointing other modules' Archive fixes at as a working example, rather than each module inventing
   its own fix pattern independently. Cite: `domain-mapping/_shared-findings.md` F3,
   `gaps/dashboards.md`. Effort: none (documentation/reference value only). Risk: none. Sequencing:
   reference this when executing the Archive-event fixes recommended in `accidentmanagement.md`,
   `riskmanagement.md`, and `scorecards.md`.

3. **Apply XC-1 (tenancy + RLS engagement) to `Dashboard`/`Widget`** — pattern-inferred classic
   repository shape on both. Cite: `domain-mapping/_shared-findings.md` F1, `gaps/dashboards.md`.
   Effort: part of XC-1's solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land
   before XC-3 (auth-policy registration) proceeds for this module's endpoints.**

4. **Apply XC-2 (actor/correlation propagation) to `Dashboard`/`Widget`'s outbox calls** — both hit
   by Bug#1 only. Cite: `security/actor-propagation.md` §2b, `gaps/dashboards.md`. Effort: part of
   XC-2's pass. Risk: Critical (see XC-2). Sequencing: none — independent, run any time.

5. **Confirm the `_Reads`/`_Writes` SQL naming convention this aggregate uses (rather than
   Create/Update) is intentional and documented** — confirmed present under that name, not a
   false-negative in this audit, but worth a brief note in the module's own documentation so a
   future engineer or auditor doesn't mistake the naming convention for a missing Create/Update pair.
   Cite: `gaps/dashboards.md`. Effort: S (documentation only). Risk: none — a clarity item, not a
   defect. Sequencing: none.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
