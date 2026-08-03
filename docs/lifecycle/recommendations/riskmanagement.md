# Recommendations — RiskManagement

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. `Risk` (top-level)
is a directly-verified — not pattern-inferred — example for both the tenancy gap and RLS's
SESSION_CONTEXT-never-engages finding; it is one of the 3 concrete examples the security audit built
its conclusions from. This module's fixes lead with the duplicate-concept cleanup and the silent-
archive fix, both module-specific, ahead of the (still critical) cross-cutting items.

1. **Formally retire `Operational.RiskManagement`** (the Create-only, GET-only-controller-confirmed
   dead duplicate of `Risk`) — one of the solution's clearest "dead duplicate" shapes, alongside
   Alert shape #2 and `Compliance.Auditing`. Fix: confirm no write path exists under this name (the
   audit already confirms `usp_RiskManagement_List` is the only proc, no write proc under this name),
   then formally deprecate and remove, coordinating with the cross-module duplicate-concept cleanup
   effort referenced in `analytics.md`. Cite: `gaps/_severity-ranking.md` #13, `gaps/riskmanagement.md`.
   Effort: M (mostly deletion + a decommission plan). Risk: **Tech-debt today**, a live landmine for
   future development — a new engineer extending "Risk" functionality has a real chance of picking
   the dead duplicate. Sequencing: coordinate with the cross-module duplicate-concept cleanup effort.

2. **Fix `Risk.Archive`'s silent-archive gap** — confirmed (directly, not suspected) to guard the
   transition but never call `AddDomainEvent`, silent-archive instance #2 of 4 in the solution. Fix:
   same approach as `accidentmanagement.md` item 3 — confirm with compliance whether archival needs
   to be observable, add the missing `AddDomainEvent(RiskArchivedEvent)` call if so, using
   `Dashboard.Archive`'s correctly-instrumented pattern (see `dashboards.md` item 2) as the template.
   Cite: `gaps/_severity-ranking.md` #9, `domain-mapping/_shared-findings.md` F3,
   `gaps/riskmanagement.md`. Effort: S. Risk: None-just-tech-debt if intentional;
   data-integrity/compliance gap if a retention consumer is ever built expecting this signal.
   Sequencing: none — independent of every other item here.

3. **Apply XC-1 (tenancy + RLS engagement) to `RiskDapperRepository`, using it as a reference
   implementation** — `RiskDapperRepository.GetByIdAsync` (`RiskDapperRepository.cs:70-72`) is one of
   the audit's own directly-confirmed (not pattern-inferred) examples for both the tenancy gap and
   the RLS-never-engages finding. Cite: `domain-mapping/_shared-findings.md` F1,
   `security/rls-audit.md` §2, `gaps/riskmanagement.md`. Effort: part of XC-1's solution-wide pass —
   prioritize given it's directly confirmed. Risk: Critical (see XC-1). **Sequencing: must land and
   be verified before XC-3 (auth-policy registration) proceeds — this aggregate specifically should
   be one of the first fixes confirmed.**

4. **Apply XC-2 (actor/correlation propagation) to `Risk`'s outbox and AuditLog** — named explicitly
   in the 23-aggregate both-bugs cluster. Cite: `security/actor-propagation.md` §2a,
   `gaps/riskmanagement.md`. Effort: part of XC-2's pass. Risk: Critical (see XC-2). Sequencing:
   none — independent, run any time.

5. **Preserve `ApplyControlMeasure`'s cross-field justification rule** (requires justification for a
   lower-effectiveness control-hierarchy choice) as a positive-control reference during any future
   rule-enforcement work on this or related aggregates — a genuine, real business rule, not
   cosmetic. Cite: `gaps/riskmanagement.md`. Effort: none (documentation/reference value). Risk:
   none. Sequencing: none.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
