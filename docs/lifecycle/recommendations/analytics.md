# Recommendations — Analytics

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. Analytics is
entirely 4 create-only marker aggregates (`Culture`, `Engagement`, `PredictiveAnalytics`,
`ScoreCards`) — the real work here is resolving which of these have any live write destination at
all, not chasing cross-cutting security fixes that mostly don't apply (Create-only, no load-by-ID
transition to violate).

1. **Resolve `Culture`'s zero-implementation status.** `ICultureRepository` has zero implementations
   in the solution; a same-named-but-unrelated read-only `CultureRepository` class exists as a
   naming-collision trap that could mislead a future engineer searching for the write path. This is
   part of the XC-4 cross-cutting finding. Fix: per XC-4, write the missing repository implementation
   if this aggregate is still needed, or formally retire it and delete/rename the misleading
   same-named class if it's dead. Cite: `sql-shape/_shared-findings.md` G1/G2,
   `gaps/_severity-ranking.md` #4, `gaps/analytics.md`. Effort: part of XC-4's L-effort pass (M if
   scoped to just this one aggregate). Risk: Availability (Create fails at DI resolution) if still
   needed; none if confirmed dead. Sequencing: independent of XC-1/XC-2/XC-3; do the naming-collision
   cleanup regardless of the resolution, since it's a standing landmine either way.

2. **Confirm `Engagement`'s SQL destination.** No confirmed SQL of any kind was found under
   exact/loose match this audit — a candidate zero-SQL orphan distinct from `Culture`'s
   DI-resolution-level finding (this one may not even have a database table). Fix: targeted grep/
   confirmation with the team; if genuinely unwired, treat as an XC-4 candidate or retire the
   aggregate. Cite: `gaps/analytics.md`. Effort: S (investigation) + variable if a real
   implementation is needed. Risk: Availability if the feature is expected to work; none if
   aspirational/unused. Sequencing: none.

3. **Confirm `PredictiveAnalytics`'s Create path** — only read-only SQL (`_List`) was found, no
   write proc; `Create` likely no-ops or fails today. Fix: same investigation pattern as item 2 —
   resolve whether this is a genuine gap or an intentionally read-only/reference aggregate. Cite:
   `gaps/analytics.md`. Effort: S (investigation). Risk: Availability if Create is expected to work
   for end users today; low otherwise. Sequencing: none.

4. **Resolve the `ScoreCards` (Analytics) vs. `Scorecards.Scorecard` (top-level, live, rich)
   duplicate-concept ambiguity** — the 3rd confirmed duplicate-concept cluster in the solution
   (after Alert and Dashboard), with genuinely ambiguous SQL ownership. Fix: confirm definitively
   which is live (the top-level `Scorecard` almost certainly is, per `scorecards.md`'s confirmation),
   then formally deprecate/remove the Analytics marker duplicate, resolving the shared SQL prefix
   ambiguity as part of the same change. This is the same class of fix recommended for the Alert/
   Dashboard/Risk duplicate pairs in their own module files — treat all of these as one coordinated
   duplicate-concept cleanup effort, not 4+ separate ad hoc decisions. Cite:
   `gaps/_severity-ranking.md` #13, `gaps/analytics.md`, `gaps/scorecards.md`. Effort: M (mostly
   deletion + a decommission plan, SQL tables involved). Risk: **Tech-debt today**, but a live
   landmine for a future engineer with a 50/50 chance of extending the dead duplicate instead of the
   live `Scorecard`. Sequencing: coordinate with the equivalent items in `scorecards.md`,
   `dashboards.md`, `alerts.md`, and `riskmanagement.md` — recommend a single cross-module duplicate-
   concept cleanup pass rather than 4 independent module-level efforts.

5. **All 4 aggregates are Create-only by structural necessity of this analysis** — the missing
   Update/Close/Cancel/Delete transitions are not independently actionable findings; fold this
   module into the solution-wide Create-only product review recommended in
   `docs/ingestion/recommendations/plantsafety-recommendations.md` Top-5 #10 rather than scoping a
   separate Analytics-specific review. Cite: `gaps/_severity-ranking.md` #16, `gaps/analytics.md`.
   Effort: none additional (already covered by the ingestion-discovery recommendation). Risk:
   None-just-completeness. Sequencing: none.

See `_cross-cutting.md` XC-4 (zero-implementation repositories) for the shared engineering pattern
behind items 1-3. Tenancy/RLS/actor-propagation (XC-1/XC-2) do not meaningfully apply to this module
beyond the Create path, which already carries TenantId correctly per the audit's own framing.
