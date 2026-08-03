# Recommendations — Intelligence

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. Intelligence is
the solution's one confirmed "guard exists, is provably dead at runtime" module — a fundamentally
different fix shape from every other module's gaps, and already covered in detail by
`docs/ingestion/recommendations/intelligence-recommendations.md`. This file cross-references that
document's Top-5 and adds the lifecycle-audit-specific framing.

1. **Rearchitect `TrainingJob`/`Investigation`'s bypassed command handlers to actually construct and
   call the domain aggregate**, instead of calling the Dapper repository directly. Every reachable
   handler (`Start`/`Cancel`/`RecordEvaluation`-style transitions on both aggregates) currently
   bypasses fully-written, genuinely correct guard logic (terminal-state checks, non-empty-reason
   validation) — this creates false confidence, since a code reviewer reading only the domain layer
   would sign off on lifecycle safety that doesn't exist at runtime. This is flagged as a **higher
   remediation priority than the Create-only aggregates found elsewhere in the solution**, because
   it's worse than an admittedly-absent guard. Already this module's own Top-5 #2 in
   `intelligence-recommendations.md`. Cite: `domain-mapping/_shared-findings.md` F2,
   `rule-enforcement/_shared-findings.md` G6, `gaps/_severity-ranking.md` #10, `gaps/intelligence.md`.
   Effort: L (rearchitects the persistence flow for both aggregates; also requires locating/
   reconstructing the stored procedures' actual bodies to confirm whether they independently enforce
   any guard today — Intelligence has no source-controlled SQL). Risk: **Data integrity** — training
   jobs/investigations can be cancelled/completed/failed in any order with zero enforcement if the
   SQL layer doesn't independently guard it (unconfirmed either way). Sequencing: do the SQL-body
   investigation first (see item 3 in `intelligence-recommendations.md`) — it determines whether this
   is "redundant safety net missing" or "zero enforcement exists anywhere," which changes the
   urgency.

2. **Replace `NullOutboxPublisher` with a real `IOutboxPublisher` implementation** — the module's one
   flow that correctly writes to the Outbox and gets silently marked "delivered" while never actually
   sent. Already this module's own Top-5 #1 in `intelligence-recommendations.md`, and independently
   the single most severe dead-end found across the entire ingestion discovery (an active
   stand-in that lies about success, not merely "nothing consumes this"). Cite:
   `gaps/intelligence.md`, `command-handler-map/_shared-findings.md` F9 (this module's version is
   worse than the generic zero-consumer pattern). Effort: M-L. Risk: **Data integrity** — an active
   monitoring/observability hazard; anyone checking outbox health today sees 100% processed, 0%
   backlog, and concludes everything is fine. Sequencing: at minimum, change
   `NullOutboxPublisher` to **not** mark messages `Processed = true` on failure-to-deliver as an
   immediate stopgap, ahead of building the real transport.

3. **Wire authentication middleware into `Intelligence.Api`** — same gap as DataLake, zero
   `AddAuthentication`/`AddJwtBearer`/`[Authorize]` anywhere. Already this module's own Top-5 #3.
   Cite: `gaps/intelligence.md`, `security/rls-audit.md` §5. Effort: M. Risk: **Security** —
   `AuthorizationBehavior`'s business-rule checks run against an unauthenticated identity today.
   Sequencing: independent of PlantSafety's XC-1/XC-3 chain (this is Intelligence's own
   authentication gap on independent infrastructure, not the PlantSafety tenancy/RLS defect).

4. **Add `TenantId` as a first-class property on the `TrainingJob` aggregate itself** (it currently
   has none), keep `Investigation.RequestedByActorId` as a strongly-typed `UserId` rather than
   narrowing to `string`, and add first-class `TenantId`/`ActorId` columns to
   `Intelligence.Infrastructure.Persistence.Outbox.OutboxMessage`. This is Intelligence's own version
   of the actor/tenant type-fidelity problem XC-1/XC-2 address for PlantSafety, but a structurally
   different gap (type narrowing, not hardcoding) requiring its own fix. Already this module's own
   Top-5 #4. Cite: `security/actor-propagation.md` §3, `gaps/intelligence.md`. Effort: M (touches
   aggregate constructors, command handlers, and the outbox schema together — do as one coordinated
   change). Risk: **Data integrity** — cross-tenant mixing risk in future dead-letter/retry tooling;
   currently moot only because item 2's `NullOutboxPublisher` dead-ends the pipeline — **fixing item
   2 without also fixing this makes the gap live.** Sequencing: **must land together with, or before,
   item 2's real transport implementation** — do not fix the publisher before this, or the dead-end
   pipeline becomes a live cross-tenant mixing risk the moment messages actually start flowing.

5. **Instantiate/publish the never-raised events** (`DataLakeIngestionCompletedIntegrationEvent`,
   `TrainingJobStartedIntegrationEvent`, `TrainingJobCancelledIntegrationEvent`) once item 1 lands —
   the natural place for these to originate is the aggregate's own guarded transition methods.
   Already this module's own Top-5 #5. Cite: `gaps/intelligence.md`. Effort: S-M, folded into item
   1's larger fix. Risk: None-just-tech-debt until item 1 lands. Sequencing: **after item 1** — these
   events are meant to be raised from the aggregate methods item 1 wires up.

**Recommended overall sequencing for this module**: item 3 (auth) can run anytime, independent of
everything else. Items 1 and 5 are sequential (5 depends on 1). Item 4 must land no later than item
2 (do not let the publisher go live carrying un-fixed tenant/actor type gaps).

See `docs/ingestion/recommendations/intelligence-recommendations.md` for full detail on all 5 items.
Intelligence does not participate in PlantSafety's XC-1/XC-2/XC-3 sequencing chain (`_cross-cutting.md`)
— its bugs are independently-shaped (guard-bypass, publisher stub, type-narrowing) rather than the
PlantSafety tenancy/RLS/auth-policy trio.
