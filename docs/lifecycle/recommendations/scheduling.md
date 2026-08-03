# Recommendations — Scheduling

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. `PMScheduling.Cancel`
is the single most literal "missing transition" finding that isn't actually missing — it exists,
fully built, correctly guarded and event-raising, and is simply never exposed via any route. This
module-specific item is the priority fix here, well ahead of its share of the generic cross-cutting
work.

1. **Add the missing `cancel` route for `PMScheduling`.** `PMScheduling.Cancel(string reason, ...)`
   is the only `Cancel(...)` method found across all ~93 PlantSafety aggregates, and it's genuinely
   well-built (status guard, non-empty-reason check, raises `PMSchedulingCancelledEvent`, has the
   AuditLog insert wired) — but `SchedulingV1Controller` exposes `schedule`/`record-completion`/
   `escalate` only, no `cancel`. This is close to a pure "add the missing controller action" task
   since the domain/command layer is already done; already recommended by
   `docs/ingestion/recommendations/plantsafety-recommendations.md` Top-5 #12, reconfirmed as this
   module's #1 priority. Fix: add `POST api/v{version}/scheduling/pmscheduling/{id}/cancel`, wired to
   the existing, already-correct domain method and command infrastructure. Cite:
   `gaps/_severity-ranking.md` #8, `inventory.md`, `gaps/scheduling.md`. Effort: S. Risk:
   **Availability/operational** — equipment-decommissioning/PM-cancellation can never be recorded
   through the API today despite the backend fully supporting it. Sequencing: none — this is a
   small, isolated fix, independent of every cross-cutting item.

2. **Apply XC-1 (tenancy + RLS engagement) to all 5 aggregates** (`PMScheduling`,
   `WorkOrderPriority`, `EmergencyDrillPlanning`, `HazardIsolationSequencing`, `ShiftFatigue`) — all
   named explicitly in the 23-aggregate audit-logged cluster, pattern-inferred classic repository
   shape. Cite: `domain-mapping/_shared-findings.md` F1, `gaps/scheduling.md`. Effort: part of XC-1's
   solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land before XC-3 (auth-policy
   registration) proceeds for this module's endpoints — note this includes the newly-exposed
   `cancel` route from item 1, so land item 1 and item 2 together for `PMScheduling` specifically,
   or the newly-reachable Cancel transition inherits the same tenancy blindness as every other
   route the moment it goes live.**

3. **Apply XC-2 (actor/correlation propagation) to all 5 aggregates** — named explicitly in the
   23-aggregate both-bugs cluster. Cite: `security/actor-propagation.md` §2a, `gaps/scheduling.md`.
   Effort: part of XC-2's pass. Risk: Critical (see XC-2). Sequencing: none — independent, run any
   time.

4. **Wire the 3 dormant `_Update` procs** (`EmergencyDrillPlanning`, `HazardIsolationSequencing`,
   `ShiftFatigue`) if update capability is needed for these Create-only aggregates — SQL already
   exists, unused. Cite: `gaps/scheduling.md`, `gaps/_severity-ranking.md` #16. Effort: S per
   aggregate if confirmed needed. Risk: Product-completeness gap. Sequencing: none.

5. **Add an integration test confirming `PMScheduling.Cancel`'s guard behaves correctly once
   exposed** (status check, non-empty-reason validation, event publication) — since this transition
   has never been exercised through a live route, verify it end-to-end rather than assuming the
   unreachable code is bug-free simply because it looks well-built. Cite: `gaps/scheduling.md`.
   Effort: S, done alongside item 1. Risk: low — a verification step, not a known defect. Sequencing:
   do alongside item 1, before considering the route "done."

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
