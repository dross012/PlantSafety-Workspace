# Recommendations — Alerts

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. Alerts has the
solution's only confirmed silent-truncation bug and its richest-but-completely-dead guarded
lifecycle (shape #2) — both module-specific, non-cross-cutting findings that should be prioritized
ahead of this module's share of the generic tenancy/actor work.

1. **Fix `TriggerAlertCommand`'s missing validator and confirmed silent truncation.** This is the
   **only confirmed truncation instance in the entire 541-stored-procedure solution**:
   `StoredProcedureBuilder.AddString` truncates `Title`/`LocationZoneName` (SQL-capped at 200 chars)
   before the value reaches SQL Server, silently, with no exception. No FluentValidation validator
   exists for this command at all. Fix: add `TriggerAlertCommandValidator` with
   `MaximumLength(200)` on both fields immediately — this exact fix is already specified in
   `docs/ingestion/recommendations/plantsafety-recommendations.md` Top-5 #5; this document
   reconfirms it as this module's #1 priority rather than duplicating the analysis. Cite:
   `gaps/_severity-ranking.md` #14/#15, `sql-shape/_shared-findings.md` G3, `rule-enforcement/_shared-findings.md`
   G3, `gaps/alerts.md`. Effort: S. Risk: **Data integrity** — rows are silently written with
   corrupted/shortened data today, no error signal anywhere in the stack. Sequencing: none — do this
   immediately, independent of every cross-cutting item.

2. **Decide whether `Operational.Alerts.Alert` (shape #2) — the richest, best-factored guarded
   lifecycle in the solution (9 transitions, dedicated `AlertLifecycleGuard`) — should be wired to a
   real HTTP route, or formally decommissioned.** It is currently 100% unreachable from any
   controller; the guard logic is real and well-built, not a stub, and sits completely idle. Fix:
   confirm with product whether this shape or shape #1 (`Alerts.Alert`, the live one) is the intended
   long-term model; if shape #2 is meant to be exposed, add the missing controller/routes; if it's
   dead, formally deprecate and remove it (including resolving the SQL-identity ambiguity with shape
   #1's `usp_Alert_*` naming collision) rather than leaving two "Alert" concepts in the solution.
   Cite: `gaps/_severity-ranking.md` #8, `domain-mapping/_shared-findings.md` F4, `gaps/alerts.md`.
   Effort: S (wire routes) or M (decommission, since SQL tables/naming collision are involved). Risk:
   **Tech-debt today** (dead code can't misbehave) but a **latent risk** if ever wired up without
   re-auditing the guard logic against current requirements, and a standing landmine for a future
   engineer who might extend the wrong shape. Sequencing: none, but resolve before any future Alerts
   feature work to avoid extending the wrong shape.

3. **Locate or formally close out the 3rd Alert shape** noted by the audit but never re-found this
   phase. Fix: a targeted search (grep for `Alert` across any remaining unindexed folders, or confirm
   with whoever authored the original discovery finding whether it still exists) — either document it
   as resolved/merged, or add it to the shape-#1-vs-#2 decommission decision in item 2. Cite:
   `gaps/alerts.md`. Effort: S. Risk: low — unknown/unconfirmed, but leaving a known-unknown in the
   audit trail is its own small risk for future audits. Sequencing: do alongside item 2.

4. **Apply XC-1/XC-2 to the live `Alerts.Alert` shape.** Pattern-inferred (not individually
   re-confirmed) classic-repository tenancy/RLS blindness, and confirmed Bug#1-only actor-propagation
   hardcoding at this aggregate's outbox call site. Cite: `domain-mapping/_shared-findings.md` F1,
   `security/actor-propagation.md` §2b, `gaps/alerts.md`. Effort: part of XC-1/XC-2's solution-wide
   pass. Risk: Critical/High (see `_cross-cutting.md`). **Sequencing: per XC-1/XC-3's required order
   — do not treat Alerts' auth-policy fix as safe to land ahead of its own XC-1 tenancy fix.**

5. **`RecordResponseAction` has no confirmed `AddDomainEvent` call — a partial fails-to-publish gap
   distinct from the module's other 7 transitions**, which do publish correctly (though into the
   same universally-dead consumer landscape, see `gaps/_severity-ranking.md` #12). Fix: confirm
   whether this is an intentional terminal/no-event action or a genuine gap; if the latter, add the
   missing event call following the pattern already used by this aggregate's other transitions.
   Cite: `gaps/alerts.md`, `command-handler-map/_shared-findings.md` F9. Effort: S. Risk:
   None-just-tech-debt today (no working consumer exists solution-wide regardless), but should be
   fixed before any consumer-wiring work lands (cross-reference: `docs/ingestion/recommendations/plantsafety-recommendations.md`
   Top-5 #2 covers consumer registration generally). Sequencing: fix before any event-consumer
   wiring work targets Alerts specifically.

See `_cross-cutting.md` for XC-1 (tenancy+RLS), XC-2 (actor propagation), XC-3 (auth-policy
registration — required after XC-1).
