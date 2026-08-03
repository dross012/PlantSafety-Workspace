# Recommendations — ContractorSafety

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. This module's 5
rich sub-aggregates are entirely new to this audit and show no confirmed bypass/silent-event/
cross-module gaps of their own — the residual exposure is almost entirely the generic cross-cutting
items, with one real module-specific open question (the PermitToWork duplicate-concept risk).

1. **Resolve the `ContractorSafety.PermitToWork` vs. `PermitToWork.PermitWorkflow` duplicate-concept
   risk** — confirmed distinct-but-overlapping (not identical), a genuine data-integrity exposure:
   the same physical permit could be recorded as either/both aggregate, with no shared identity or
   sync mechanism between them. This is the top open follow-up flagged by Phase 2 and reconfirmed
   here. Fix: a product/architecture review to determine whether these are genuinely two distinct
   bounded contexts (plausible — contractor permits and general work permits may be legitimately
   different processes) or an unintentional split that should be merged/cross-referenced. If
   distinct, add an explicit cross-reference field so both can be queried together for compliance
   reporting; if unintentional, plan a consolidation. Cite: `domain-mapping/_shared-findings.md` F5,
   `gaps/_severity-ranking.md` #13, `gaps/contractorsafety.md`, `gaps/permittowork.md`. Effort: M
   (review) + variable depending on outcome. Risk: **Data integrity (compliance-reporting risk)** —
   a permit tracked in one system and missed by a query against its sibling is a real audit-exposure
   risk, not a runtime-crash risk. Sequencing: coordinate with `permittowork.md`'s equivalent item —
   this is a two-module decision, not resolvable from either module's file alone.

2. **`IncidentTrackingPerformanceEvaluation` has no Close/Resolve transition** — the same "raised but
   never formally closed" gap shape as `Observability.MachineProcessMonitoring`. Fix: confirm with
   product whether this is intentional (an append-only performance log) or a genuine missing
   lifecycle step; if the latter, add a guarded `Close`/`Resolve` method following this module's
   existing guard patterns. Cite: `gaps/_severity-ranking.md` #16, `gaps/contractorsafety.md`,
   `gaps/observability.md`. Effort: S-M if confirmed needed. Risk: Product-completeness gap, not a
   security/integrity defect — fold into the solution-wide Create-only/incomplete-lifecycle review
   already recommended by the Ingestion Discovery (`plantsafety-recommendations.md` Top-5 #10) rather
   than scoping separately. Sequencing: none.

3. **Confirm the live-vs-legacy status of all 5 sub-aggregates** — none was controller-confirmed this
   audit (all findings here are pattern-inferred, not directly verified). Fix: a quick controller-
   routing confirmation pass (grep the ContractorSafety controllers, confirm each of the 5
   aggregates has a live, reachable route) before investing further remediation effort — a cheap
   confirmation step that de-risks every other recommendation in this file. Cite:
   `gaps/contractorsafety.md`. Effort: S. Risk: low directly, but de-risks prioritization of items
   4-5 below. Sequencing: do first, before XC-1/XC-2 work on this module, since a dead aggregate
   doesn't need the fix.

4. **Apply XC-1 (tenancy + RLS engagement) to all 5 sub-aggregates** — pattern-inferred classic
   repository shape across the cluster. `PermitToWork (ContractorSafety)`'s own repository shares the
   same shape as the directly-confirmed `PermitToWorkDapperRepository` example cited in
   `security/rls-audit.md`. Cite: `domain-mapping/_shared-findings.md` F1, `gaps/contractorsafety.md`.
   Effort: part of XC-1's solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land
   before XC-3 (auth-policy registration) proceeds for this module's endpoints.**

5. **Apply XC-2 (actor/correlation propagation) to all 5 sub-aggregates' outbox calls** — all Bug#1
   only (not in the 23-both-bugs cluster). Cite: `security/actor-propagation.md` §2b,
   `gaps/contractorsafety.md`. Effort: part of XC-2's pass. Risk: Critical (see XC-2). Sequencing:
   none — independent, run any time.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
