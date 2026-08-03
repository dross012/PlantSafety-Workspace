# Recommendations — Azure

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. Azure is
architecturally distinct from PlantSafety (ARM control-plane operator surface, no local aggregates,
no SQL, no RLS-relevant state) — most PlantSafety cross-cutting fixes (XC-1, XC-4, XC-5, XC-6) do not
apply here at all. This module's own `docs/ingestion/recommendations/azure-recommendations.md`
already covers its top items in detail; this file cross-references rather than duplicates, adding
only the lifecycle-audit-specific framing.

1. **Add real actor identity capture to all 18 event handlers** (already the ingestion discovery's
   own Top-5 #2 for this module — see `docs/ingestion/recommendations/azure-recommendations.md`).
   This is Azure's version of the solution-wide actor-propagation problem (XC-2), but a structurally
   different and, per the audit, "third failure shape" — Azure has **no `ActorId` field anywhere in
   the module at all**, not a hardcoded-and-discarded one like PlantSafety. Fix: thread the
   authenticated `ClaimsPrincipal`/user-id claim (already available via Azure's already-correctly-
   wired JWT bearer middleware, unlike DataLake/Intelligence/Reporting) through the command to the
   event and into each handler's log line. Cite: `security/actor-propagation.md` §3,
   `gaps/_severity-ranking.md` #3, `gaps/azure.md`. Effort: M (18 near-identical files, but each
   change is small once the first is templated). Risk: **Security/audit-trail** — an authenticated
   caller can restart a Container App, purge a dead-letter topic, or disable a Key Vault secret and
   nothing records *who*. Sequencing: independent of PlantSafety's XC-1/XC-2/XC-3 chain — Azure has
   no tenant-isolation concept to interact with (see item 4), so there is no equivalent sequencing
   trap here.

2. **Fully verify `PurgeTopicDeadLetterCommandHandler`'s precondition/confirmation behavior** — the
   one Azure operation not yet read line-by-line in either audit, and the one irreversible,
   data-destroying operation in the module's ~15-transition population. Fix: read the handler in
   full; if no confirmation/dry-run/precondition exists and the underlying Service Bus API has no
   undo window, add an application-level confirmation step (e.g. `confirm=true` flag). Cite:
   `gaps/azure.md`, `docs/ingestion/recommendations/azure-recommendations.md` Top-5 #3. Effort: S
   (read) + S-M (guard, if warranted). Risk: **Availability/data-integrity**, unconfirmed severity —
   the one genuinely open question in this module, worth closing before it matters. Sequencing:
   none.

3. **Confirm with the team whether the 18 log-only consumers are the intended final state** or a
   stepping stone — several event doc comments explicitly say cross-module fan-out "can be added in
   future." This is a product conversation, not an engineering task, and the ingestion discovery's
   own judgment (this document concurs) is that the current design is acceptable as scoped. Cite:
   `gaps/_severity-ranking.md` #12 (zero working consumers, solution-wide finding this module is a
   partial exception to — Azure's consumers do genuinely execute, unlike every other module's dead
   consumers), `docs/ingestion/recommendations/azure-recommendations.md`. Effort: N/A (conversation).
   Risk: None-just-tech-debt if confirmed intentional; availability/operational if not. Sequencing:
   none.

4. **No tenancy/RLS work is needed for Azure** — confirmed non-finding, recorded here explicitly so
   it isn't mistakenly folded into the solution-wide XC-1 rollout. Azure has no PlantSafety-tenant
   concept; the only "TenantId"-named field found is the unrelated Entra/Azure AD tenant id. Cite:
   `rule-enforcement/_shared-findings.md` G1 (explicitly excludes Azure), `security/rls-audit.md` §5,
   `gaps/azure.md`. Effort: none. Risk: none. Sequencing: N/A — this item exists to prevent
   mis-scoping XC-1's rollout into a module where it doesn't apply.

5. **Document the intentional "delegate idempotency/precondition-checking to the target Azure
   service" design decision at the module level**, not just per-handler — already recommended by the
   ingestion discovery (`azure-recommendations.md` Top-5 #4). A documentation-clarity item, not a
   functional gap. Effort: S. Risk: None-just-tech-debt. Sequencing: none.

See `docs/ingestion/recommendations/azure-recommendations.md` for full detail on items 1-3 and 5.
See `_cross-cutting.md` for XC-2 (actor propagation, the PlantSafety-specific version of item 1's
underlying problem) — note Azure's fix is independent, not a subset, of PlantSafety's XC-2 pass.
