# Modernization Recommendations — Reporting

Phase 9 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Reporting is the smallest module in the solution (5 endpoints, one controller) and architecturally
the simplest - no domain aggregates, no repository/DbContext writes, no events. Its single defining
problem is also the starkest in the whole solution: zero authentication of any kind on a surface
that renders and returns potentially sensitive aggregate safety data.

## Top 5 fixes, in priority order

1. Wire authentication middleware (JWT bearer + [Authorize]) into Reporting.Api immediately. This
   is the starkest access-control gap found anywhere in the solution: no
   AddAuthentication/AddJwtBearer/[Authorize] anywhere in Program.cs, and app.UseAuthorization()
   is called with nothing to authorize against. Effort: S (smallest module in the solution - 5
   endpoints, 1 controller). Risk of not fixing: Security (confirmed - an unauthenticated network
   caller can trigger PDF generation/export of aggregate safety-report data today, with zero proof
   of identity).
2. Add tenant scoping to generate/export/preview. Once authentication exists, add TenantId
   derivation from the authenticated claim (not client input) to every report request, consistent
   with how PlantSafety's envelope pattern derives it. Effort: S-M. Risk of not fixing: Security
   (a report surface with no tenant concept can, once authenticated, still return any tenant's
   data to any authenticated user).
3. Add actor capture (who generated/exported which report, when) - currently zero ActorId
   anywhere in this module. This is also the natural first step toward the persisted-audit-trail
   improvement noted below. Effort: S, once authentication exists.
4. Decide, explicitly, whether this module should persist an audit trail of "who generated what
   output when." Currently nothing is persisted at all - every endpoint either returns
   catalog/metadata or streams a PDF directly in the HTTP response with no repository/DbContext
   involved. See the design-judgment note below. Effort: M if the decision is yes. Risk of not
   fixing: none-just-tech-debt if a pure-render, no-persistence design is confirmed intentional;
   compliance/audit gap if generation of safety-related output is expected to be traceable after
   the fact.
5. Once authentication lands, add validators for GetReportMetadataQuery and the preview query -
   currently no dedicated validator was found for either, unlike generate/export which do have
   one. Effort: S. Risk of not fixing: none-just-tech-debt/low.

## Design judgment: is "no auth, no tenancy" acceptable for a pure-read surface?

This document's judgment: this is a gap, not an acceptable design, specifically because of what
the data is. A pure-read, no-persistence module can reasonably have a simpler security posture
than a write-heavy module - that part of the architecture is fine and doesn't need to change. But
"simpler" cannot mean "none": generate/export render PDFs of aggregate safety data (the module's
own naming and the fact that the generation orchestrator presumably queries PlantSafety/other
modules' tenant-scoped tables to build the document strongly implies this). Rendering another
tenant's safety compliance data to an unauthenticated caller is a real confidentiality exposure
regardless of how architecturally simple the module's write-side is. Recommendation: authentication
and tenant-scoping are non-negotiable here (Top-5 #1-2); whether to also add persistence for an
audit trail (Top-5 #4) is the one genuinely optional, product-level decision in this document, and
depends on whether "who looked at this output and when" needs to be answerable after the fact for
compliance purposes - a question this document cannot resolve from code alone and flags for the
team to decide.

## Security

### 1. Zero authentication anywhere in the module
Problem: the module's Program.cs was fully grepped for auth wiring - none found. No [Authorize]
attribute exists on the controller, no AddAuthentication/AddJwtBearer/FallbackPolicy is
configured. app.UseAuthorization() is called but has nothing to authorize against. This is the
smallest module in the solution by endpoint count (5) but the starkest access-control gap of the
entire API surface (classification-matrix.md, security/rls-audit.md section 3, module-flow doc for
this module).

Fix: Add AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...) and
[Authorize] to the controller (or a fallback policy requiring authentication solution-wide in this
API), mirroring Azure's/PlantSafety's already-correct pattern. Given this module has only 5
endpoints and one controller, this is one of the smallest, highest-value security fixes available
anywhere in the solution.

Effort: S.

Risk of not fixing: Security - confirmed, live, exploitable today. Not a theoretical risk: any
network caller can invoke the generate/export actions right now with zero authentication.

### 2. No tenant scoping anywhere
Problem: this module has no TenantId concept at any layer - no field on any command/query, no
accessor on the controller. Once authentication is added (item 1), this becomes the next layer of
the same underlying gap: an authenticated user from Tenant A could still request and receive
Tenant B's data if nothing scopes the underlying query (gaps/tenant-violations.md section 5).

Fix: Add a TenantId derived from the authenticated JWT claim (never client-supplied) to every
query/command in this module, and ensure the generation-orchestrator internals (not opened in this
discovery - flagged as a follow-up read) actually filter by it when pulling data from
PlantSafety/whatever module backs the output content.

Effort: S-M (depends on what the orchestrator internals look like - not yet read by this
discovery).

Risk of not fixing: Security - same severity class as item 1; both need to land together, since
fixing authentication alone without tenant-scoping still permits cross-tenant data leakage among
authenticated users.

## Data integrity / governance (the one optional item)

### 3. No persisted audit trail of generation/export activity
Problem: the generate/export command handlers have no repository/DbContext dependency injected at
all - the output is built in memory and streamed directly back in the HTTP response. There is no
record anywhere of who generated what, when, or for which tenant (module-flow doc for this module,
shape-audit doc for this module).

Fix: If the team decides this needs to be traceable (see judgment above), add a minimal generation
log table/repository write (type, tenant, actor, timestamp) inside the generate/export command
handlers, without otherwise changing the module's pure-render architecture. This is the natural
point to also close the actor-capture gap (Top-5 #3).

Effort: M (introduces the module's first persistence dependency, which is a real architectural
change even if small in scope).

Risk of not fixing: none-just-tech-debt if a stateless, non-audited render is confirmed
acceptable; compliance/audit gap if regulators or internal compliance processes expect to know who
accessed which output and when - this is exactly the kind of question a plant safety platform's
compliance team is likely to ask, so this document leans toward recommending it, while stopping
short of mandating it outright since it's a product/compliance policy decision, not a technical
defect.

## Deliberately not recommended

- Not recommending SQL Server RLS for this module - it has no persistence layer at all today
  (item 3 is optional and, even if built, would be a single new table, not a reason to stand up
  RLS infrastructure). If item 3 is built and grows into a larger persisted surface, revisit RLS
  at that point, not now.
- Not recommending changing the module's pure-render architecture - the lack of a
  repository/DbContext in generate/export today is an intentional, working design (in-memory
  render to HTTP response), not a gap; only the audit-trail question above touches this, and only
  if the team decides it's needed.
