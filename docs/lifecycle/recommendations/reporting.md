# Recommendations — Reporting Module (Lifecycle Audit Phase 9 Deliverable)

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. The "Reporting"
module (PDF/export generation) has no domain aggregates, no repository/DbContext writes, and no
lifecycle of any kind — it is out of scope for every one of this audit's 7 gap categories by
construction, and none of the PlantSafety cross-cutting items (XC-1 through XC-6) apply. Its one
defining problem, already fully documented in
`docs/ingestion/recommendations/reporting-recommendations.md`, is the starkest access-control gap
found anywhere in the solution — this file cross-references that document rather than duplicating it.

1. **Wire authentication middleware (JWT bearer + `[Authorize]`) into `Reporting.Api` immediately.**
   No `AddAuthentication`/`AddJwtBearer`/`[Authorize]` exists anywhere; `app.UseAuthorization()` is
   called with nothing to authorize against. This is the smallest module in the solution (5
   endpoints, 1 controller) but the starkest access-control gap of the entire API surface — an
   unauthenticated network caller can trigger PDF generation/export of aggregate safety data today.
   Already this module's own Top-5 #1. Cite: `gaps/reporting.md`, `security/rls-audit.md` §5.
   Effort: S (smallest module in the solution). Risk: **Security** — confirmed, live, exploitable
   today. Sequencing: independent of PlantSafety's XC-1/XC-3 chain (this module has no
   PlantSafety-tenant concept and no aggregates for that sequencing trap to apply to) — fix this
   immediately, do not wait on any other module's remediation.

2. **Add tenant scoping to generate/export/preview, derived from the authenticated JWT claim, never
   client input**, once item 1 lands. Already this module's own Top-5 #2. Cite:
   `gaps/reporting.md`. Effort: S-M. Risk: **Security** — must land together with item 1; fixing
   authentication alone without tenant-scoping still permits cross-tenant data leakage among
   authenticated users. Sequencing: **must land together with item 1**, not as a follow-up sprint.

3. **Add actor capture** (who generated/exported which output, when) — currently zero `ActorId`
   anywhere in this module. Already this module's own Top-5 #3. Cite: `gaps/reporting.md`. Effort: S,
   once authentication exists. Risk: forensic gap, same class as the solution-wide XC-2 finding but
   on independent infrastructure (this module has no outbox/AuditLog mechanism to route through —
   this would be new capability, not a rewire). Sequencing: after item 1.

4. **Decide, explicitly, whether this module should persist a trail of "who generated what output
   when."** Currently nothing is persisted — every endpoint returns catalog/metadata or streams a
   PDF directly with no repository/DbContext involved. This is the one genuinely optional,
   product-level decision in this module's recommendations, per the ingestion discovery's own
   judgment (this document concurs): authentication/tenancy are non-negotiable (items 1-2), whether
   to add persistence for traceability depends on whether compliance needs "who looked at this" to be
   answerable after the fact. Already this module's own Top-5 #4. Cite: `gaps/reporting.md`. Effort:
   M if the decision is yes (introduces the module's first persistence dependency). Risk:
   none-just-tech-debt if a stateless design is confirmed acceptable; compliance gap otherwise.
   Sequencing: after items 1-3, since it's the natural point to also close item 3's actor-capture gap.

5. **Add validators for the metadata query and the preview query** — no dedicated validator found for
   either, unlike generate/export which do have one. Already this module's own Top-5 #5. Cite:
   `gaps/reporting.md`. Effort: S. Risk: none-just-tech-debt/low. Sequencing: after item 1.

See `docs/ingestion/recommendations/reporting-recommendations.md` for full detail on all 5 items,
including the module's design judgment on why "no auth, no tenancy" is a genuine gap, not an
acceptable simplification, for a module that renders aggregate safety data as output.
