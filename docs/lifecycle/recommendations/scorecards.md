# Recommendations — Scorecards

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. `Scorecard`
(top-level) closes out the solution's silent-archive population at exactly 4 confirmed instances —
that item, plus the copy-paste exception-message bug, are this module's real findings, ahead of its
share of the generic cross-cutting work.

1. **Fix `Scorecard.Archive`'s confirmed silent-archive gap** — the 4th and final confirmed instance
   (`Accident`, `Risk`, `Scorecard` directly confirmed; `SafetyPolicy` suspected) out of 8 total
   Archive-havers in the solution. Fix: same approach as `accidentmanagement.md` item 3 — confirm
   with compliance whether archival needs to be observable downstream, add the missing
   `AddDomainEvent(ScorecardArchivedEvent)` call if so, using `Dashboard.Archive`'s correctly-
   instrumented pattern (`dashboards.md` item 2) as the template. Cite:
   `gaps/_severity-ranking.md` #9, `domain-mapping/_shared-findings.md` F3, `gaps/scorecards.md`.
   Effort: S. Risk: None-just-tech-debt if archival is intentionally silent (plausible, given the
   consistency across all 4 instances); data-integrity/compliance gap if a retention consumer is ever
   built expecting this signal. Sequencing: none — coordinate timing with `accidentmanagement.md`,
   `riskmanagement.md`, and `safetypolicies.md`'s equivalent items so all 4 (or however many are
   confirmed to need it) land as one coordinated decision rather than 4 independent ad hoc fixes.

2. **Fix the copy-paste exception-message bug in `RecordDistribution`** — compares `Status.ToString()`
   to itself in the message text, not the guard condition (the guard itself is genuine, this is a
   cosmetic message bug, not a rule bypass). Fix: correct the exception message to reference the
   actual expected vs. actual status values. Cite: `gaps/_severity-ranking.md` #20,
   `gaps/scorecards.md`. Effort: S (trivial). Risk: **Pure tech-debt** — a misleading error message
   during debugging, no functional consequence. Sequencing: none — do opportunistically, low
   priority.

3. **Resolve the `Scorecard` vs. `Analytics.ScoreCards` duplicate-concept ambiguity** — this is the
   same item as `analytics.md` item 4; this file defers to that item's fix description. Cite:
   `gaps/_severity-ranking.md` #13, `gaps/scorecards.md`, `gaps/analytics.md`. Effort/Risk/Sequencing:
   see `analytics.md` item 4.

4. **Apply XC-1 (tenancy + RLS engagement) to `ScorecardDapperRepository`** — named explicitly in the
   23-aggregate audit-logged cluster, pattern-inferred classic repository shape. Cite:
   `domain-mapping/_shared-findings.md` F1, `gaps/scorecards.md`. Effort: part of XC-1's
   solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land before XC-3 (auth-policy
   registration) proceeds for this module's endpoints.**

5. **Apply XC-2 (actor/correlation propagation) to `Scorecard`'s outbox and AuditLog** — named
   explicitly in the 23-aggregate both-bugs cluster. Cite: `security/actor-propagation.md` §2a,
   `gaps/scorecards.md`. Effort: part of XC-2's pass. Risk: Critical (see XC-2). Sequencing: none —
   independent, run any time.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
Also preserve `CompleteCompilation`'s real business rule (rejects a scorecard with zero recorded
measurements) as a positive-control reference — not a gap, a working guard worth citing elsewhere.
