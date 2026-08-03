# Recommendations — SafetyCulture

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. `CultureAssessment`/
`StopWorkEvent` are unremarkable relative to the rest of the solution — no bypass, no silent-archive,
no confirmed cross-module gap. This module is almost entirely a consumer of the cross-cutting fixes;
there is no significant module-specific engineering work to prioritize ahead of them.

1. **Apply XC-1 (tenancy + RLS engagement) to both aggregates** — pattern-inferred classic
   repository shape on both `CultureAssessment` and `StopWorkEvent`. Cite:
   `domain-mapping/_shared-findings.md` F1, `gaps/safetyculture.md`. Effort: part of XC-1's
   solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land before XC-3 (auth-policy
   registration) proceeds for this module's endpoints.**

2. **Apply XC-2 (actor/correlation propagation) to both aggregates' outbox calls** — both hit by
   Bug#1 only. Cite: `security/actor-propagation.md` §2b, `gaps/safetyculture.md`. Effort: part of
   XC-2's pass. Risk: Critical (see XC-2). Sequencing: none — independent, run any time.

3. **Add AuditLog coverage for `CultureAssessment`/`StopWorkEvent`** — neither is currently covered
   (part of the 70-of-93 uncovered population). `StopWorkEvent` in particular records a genuinely
   safety-critical action (an employee halting work due to a hazard) that plausibly warrants an audit
   trail more than many of the aggregates that do have coverage today. Fix: per XC-5, add coverage
   using the corrected `ChangedBy` binding from XC-2, prioritizing `StopWorkEvent` given its
   safety-criticality. Cite: `command-handler-map/_shared-findings.md` F8,
   `gaps/_severity-ranking.md` #7, `gaps/safetyculture.md`. Effort: S-M as part of XC-5's larger
   pass, called out here as a priority candidate within that pass. Risk: Compliance — no record
   exists today that a stop-work event ever happened. Sequencing: do alongside XC-2 (same
   mechanism).

4. **Disambiguate `Analytics.Culture` from `SafetyCulture.CultureAssessment`** in any future
   documentation or search — same root word, unrelated concepts (confirmed, not a duplicate), but a
   naming collision that could confuse a future engineer searching for "Culture." Fix: no code
   change needed; add a brief cross-reference note in each module's own README/header comment
   clarifying the two are unrelated. Cite: `gaps/safetyculture.md`, `gaps/analytics.md`. Effort: S
   (documentation only). Risk: none — a clarity item. Sequencing: none.

5. **Preserve `CultureAssessment`'s 0-100 range-check on both score fields** as a positive-control
   reference — a real, working guard, worth citing as an example of correct validation when auditing
   other score-bearing aggregates (e.g. `AuditEngagement.ComplianceScore`, see `auditing.md` item 1,
   which lacks an equivalent cross-check). Cite: `gaps/safetyculture.md`. Effort: none. Risk: none.
   Sequencing: none.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3), XC-2 (actor propagation), and
XC-5 (AuditLog coverage expansion).
