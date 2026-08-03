# Lifecycle Coverage Audit — Master Blueprint

**Phase 10 of 10 (FINAL).** Root: `C:\Users\dross\source`. Date: 2026-07-26.

This document stands alone. It synthesizes all nine prior phases of this audit (~110 documents
under this folder) into one blueprint for decision-making. You should not need to open any other
file to understand the headline findings and what to do about them — every claim below links to
the phase document that backs it, in case you want the file:line evidence. Companion document: the
already-completed Ingestion Pipeline Discovery (`../ingestion/blueprint.md`), which audited how data
*enters* this solution; this audit covers what happens to that data across its *entire lifecycle*
once it exists — every state transition an aggregate can undergo, not just its creation.

---

## 1. Executive Summary

**Read this paragraph twice before reading anything else in this document.** This audit's single
most important finding is a **sequencing trap**: the companion Ingestion Discovery already found
that ~293-300 of PlantSafety's API endpoints return a 500 error on every call, because the
authorization policies their `[Authorize(Policy=...)]` attributes reference are never registered in
the DI container. The obvious fix is to register those policies. **Do not do that first.** This
audit found that neither PlantSafety's application layer nor its SQL Server Row-Level Security
engages a tenant check for the exact call shape (`GetByIdAsync(id)` → mutate → `UpdateAsync`) that
every one of those ~293+ endpoints uses — meaning today's 500 error is, by accident, the *only*
tenant-isolation control those endpoints have. Register the missing policies before fixing the
tenancy/RLS gap, and every previously-blocked request succeeds instead — with zero defense against a
same-tenant-authenticated caller who supplies another tenant's record ID. **Fixing the availability
bug first does not fail to close the security gap; it is the specific action that makes the gap
newly reachable, converting a contained (if annoying) outage into a live cross-tenant data exposure.**
Full detail: [`recommendations/_cross-cutting.md`](./recommendations/_cross-cutting.md)'s "Required
sequencing" section, [`security/rls-audit.md`](./security/rls-audit.md) §6,
[`gaps/_severity-ranking.md`](./gaps/_severity-ranking.md) gap #1.

**What this audit covered:** 10 phases of static-source analysis across the same 25 module/feature
areas the companion discovery mapped, going one level deeper than that discovery's per-aggregate
pass — this audit is **per-transition**: every public lifecycle-shaped method (Create, Update,
Approve, Reject, Close, Cancel, Archive, Escalate, etc.) on every domain aggregate/entity in the
solution, cross-referenced against its Application-layer command/handler, its SQL stored procedure,
its domain-event publication, and (new this audit) whether SQL Server's Row-Level Security and
actor/correlation-ID propagation actually engage for it. The census found **~344 lifecycle
transitions across 95 real aggregate/entity classes in 25 modules/feature areas** (93 aggregates in
PlantSafety alone, plus DataLake's 7 command-shaped transitions, Intelligence's 2 aggregates/13
transitions, Azure's ~15 ARM-control-plane transitions, and Reporting's zero). **No code was
executed anywhere in this audit** — every finding is what the source says it does, not what was
observed happening at runtime. See §11 for the full confidence discussion.

Beyond the Tier-0 sequencing trap, three other findings define this audit's shape: **zero of the
~344 transitions in the solution have confirmed, correct, end-to-end actor/correlation-ID
propagation** — two independent bugs (a C# outbox hardcode and a SQL `SYSTEM_USER` binding) mean
"who did this" is unanswerable for every mutating action recorded, solution-wide. **13+ aggregates'
domain write-repository has zero implementing class anywhere in the codebase** — calling `Create` on
any of them fails at dependency-injection resolution before a single SQL statement runs, a strictly
worse failure than "the stored procedure is missing." And **SQL AuditLog coverage exists for only
22-23 of PlantSafety's 93 aggregates**, and is best-effort/anonymous even there. None of this is
uniformly dire — the audit also found that lifecycle guards are genuinely real (not cosmetic) almost
everywhere checked, that event payloads are well-formed, and that the Intelligence-style
"guarded-but-bypassed" defect does **not** recur elsewhere in the solution — but the tenancy/RLS/
actor-attribution layer needs focused, correctly-sequenced remediation before this solution's
security and forensic posture can be trusted.

---

## 2. Complete Lifecycle Inventory

Pulled directly from Phase 1 ([`inventory.md`](./inventory.md)) — not re-counted here. Full
per-aggregate detail in [`module-inventory/*.md`](./module-inventory/) (25 files).

| Module | Aggregate classes | Total transitions | Create-only (missing Update/Close/Cancel/Delete entirely) | Rich (≥1 transition beyond Create) |
|---|---|---|---|---|
| **PlantSafety** | 93 | 309 (93 Create + 216 non-Create) | 45 | 48 |
| **DataLake** | 0 (no aggregate layer, command/handler-shaped) | 7 | N/A (structural) | N/A |
| **Intelligence** | 2 (`TrainingJob`, `Investigation`) | 13 | 0 | 2 (both bypassed at runtime — §4) |
| **Azure** | 0 (ARM control plane) | ~15 | N/A | N/A |
| **Reporting** | 0 (no aggregates, no persistence) | 0 | N/A | N/A |
| **Solution total** | 95 real classes | **~344** | 45 | 50 |

**Missing-transition census (full grep, not a sample):**

- **Delete: 93 of 93 PlantSafety aggregates (100%)** — zero `Delete(...)` methods anywhere in
  `PlantSafety.Domain`. Plausibly correct-by-design for regulatory records, but there is no C# or
  SQL path to remove a record from any of ~100 aggregates.
- **Cancel: 92 of 93 (99%)** — the sole exception, `PMScheduling.Cancel`, is fully guarded but
  unreachable from any HTTP route.
- **Archive: 85 of 93 (91%)** — only 8 aggregates have it at all, and 4 of those 8 guard the
  transition but never publish an event (§9 gap #9).
- **10-11 aggregates have zero confirmed SQL persistence of any kind** — the sharpest "would
  fail/no-op if called" finding of Phase 1, later hardened by Phase 6 into "the C# repository class
  doesn't exist either" (§7, §9 gap #4).

Module-index summary (feature areas, aggregate counts, notable finding per area) is in
`inventory.md` §3; see that file's table for the full 25-row breakdown rather than reproducing it
here.

---

## 3. Complete Domain Mapping Summary

From Phase 2 ([`domain-mapping/_shared-findings.md`](./domain-mapping/_shared-findings.md) F1-F6,
plus 25 per-module files). Real invariant enforcement is the norm, not the exception — but four
distinct problems recur:

- **F1 — Systemic tenancy gap (most consequential finding of this phase):** every repository's
  `GetByIdAsync(id)` takes only the aggregate's own ID, never `TenantId`. No domain method anywhere
  checks the caller's tenant against the loaded aggregate. Tenant isolation today depends entirely on
  GUIDs being unguessable. This is the finding §1's sequencing trap and §8's RLS audit both build on.
- **F2 — Handler-bypass is Intelligence-specific, confirmed not to recur:** Intelligence's
  `TrainingJob`/`Investigation` aggregates have real guards the application never calls; a
  representative sample of 5 other clusters (ContractorSafety, EmployeeSafety, MachineSafety,
  SafetyProcedures, Auditing) all correctly invoke the domain method.
- **F3 — Silent-archive does not extend beyond 4 aggregates:** `Accident`/`Risk`/`Scorecard`
  (confirmed) and `SafetyPolicy` (presumed) guard Archive but never call `AddDomainEvent`; the 3
  newly-found Archive-havers (`PermitTemplate`, `Dashboard`, `Operational.Alerts.Alert`) are correctly
  instrumented.
- **F4 — The Alert duplicate cluster resolved: the richer of the two shapes is 100% dead code** — all
  9 transitions of `Operational.Alerts.Alert`, the best-factored guarded lifecycle in the solution,
  have no HTTP route.
- **F5/F6 — Confirmed duplicate-concept and cross-module-gap findings**, detailed in
  [`domain-mapping/_shared-findings.md`](./domain-mapping/_shared-findings.md) and rolled into the
  gap register (§9 gaps #11, #13).

See [`domain-mapping/*.md`](./domain-mapping/) for the full per-aggregate invariant tables.

---

## 4. Complete Command/Handler Mapping Summary

From Phase 3 ([`command-handler-map/_shared-findings.md`](./command-handler-map/_shared-findings.md)
F7-F13). Headline finding: **SQL AuditLog coverage exists for only 22-23 of PlantSafety's 93
aggregates (F8)** — every write procedure for the other ~70 aggregates writes to its table with zero
audit row. Even the 23 covered aggregates' audit inserts are wrapped in an empty-`CATCH` (failures
silently swallowed) and bind `ChangedBy = SYSTEM_USER` / `Module = OBJECT_NAME(@@PROCID)` — neither
attributable to the acting user nor to the specific transition (a generic `_Update` proc can't say
whether it was an Approve or a Reject that fired).

This phase also confirmed **two distinct actor/correlation bugs**, not one bug in two places (fully
detailed in §8): a C#-layer outbox hardcode (`"system"`/`Guid.Empty` at all 58
`outboxWriter.Enqueue` call sites) and an independent SQL-layer `SYSTEM_USER` binding inside each of
the 23 audited aggregates' stored procedures. Fixing one does not fix the other. Other findings:
Outbox delivery is a universal, aggregate-agnostic mechanism (F7) but nothing consumes it (F9,
inherited from the Ingestion Discovery); `AuditEngagement.Close` and `EscalateToIncident` both take
cross-module/cross-aggregate references on faith with no verification call (F6/F11); and
`ChecklistCompletion` never checks its referenced template's status (F12). Full per-aggregate
command/handler/audit tables: [`command-handler-map/*.md`](./command-handler-map/) (25 files).

---

## 5. Complete Rule Enforcement Summary

From Phase 4 ([`rule-enforcement/_shared-findings.md`](./rule-enforcement/_shared-findings.md)
G1-G6), which ran a 6-criteria validation pass (Tenancy, Bypasses Domain Rules, Constraints,
Sequence Guards, Cross-Module Rules, Audit) across all 25 module files:

| Criterion | Result |
|---|---|
| **Tenancy** | Fails everywhere — F1 confirmed to extend even to DataLake and Intelligence, which the original evidence base hadn't covered (G1) |
| **Bypasses Domain Rules** | Passes almost everywhere — the Intelligence bypass pattern does not recur (G6); F6/F11/F12's same-aggregate/cross-module gaps remain a partial exception (G5) |
| **Constraints (validators)** | Passes for ~190 of 220 commands solution-wide; **~30 commands (14%) have no validator class at all** to wire into the (correctly, globally) registered FluentValidation pipeline (G3) |
| **Sequence Guards** | Passes everywhere directly re-read — full-body re-reads of `PermitWorkflow`/`AuditEngagement` confirm genuine `EnsureStatusIsOneOf`-style guards, not cosmetic checks (G4) |
| **Cross-Module Rules** | 4 confirmed gaps (F6/F11/F12/F13); pattern-inferred likely wider, since every handler shape uses exactly one repository dependency (G5) |
| **Audit** | Fails for ~70 of 93 aggregates (§4/F8); best-effort/anonymous even where present |

Full per-transition pass/fail rows: [`rule-enforcement/*.md`](./rule-enforcement/) (25 files).

---

## 6. Complete Event Publishing Summary

From Phase 5 ([`event-map/_shared-findings.md`](./event-map/_shared-findings.md) F14-F17), which
directly read ~60 event `record` definitions across 13 modules. Two findings dominate:

- **F14 — Positive finding:** every PlantSafety domain event follows one uniform, well-formed shape
  (`EventId`, aggregate ID, new-state fields, timestamp, actor, correlation, causation). The
  hypothesized "obviously missing field" bug (an Approved event with no approver) was **not found**
  anywhere.
- **F15 — The one payload gap that recurs almost everywhere:** no event carries the *pre-transition*
  (old) value — only the new one. `RiskScoredEvent` has the new score but no baseline; policy-review
  events carry the new version, not the superseded one. This is moot today only because nothing
  consumes any event (F9/§9 gap #12); it would become consequential the moment a real consumer needed
  to compute a delta.

Publish rates: essentially every rich aggregate's transitions raise a domain event; the 45
Create-only aggregates raise exactly one (their Created event); the 13 zero-implementation
aggregates raise none (the domain method is never reachable). F16 reconfirms that even where events
carry correct actor/correlation data in their own body, the Outbox envelope's copy of that metadata
is unconditionally wrong (§8). F17 found one deliberate double-publish
(`ChecklistCompletion.FlagFailedItem`), no bugs. Full per-transition publish/payload tables:
[`event-map/*.md`](./event-map/) (25 files).

---

## 7. Complete SQL Shape Alignment Summary

From Phase 6 ([`sql-shape/_shared-findings.md`](./sql-shape/_shared-findings.md) G1-G5). The headline
finding, ranked by its own source phase as **"the single worst — and most solution-wide — shape-
alignment finding of the entire audit"**:

**G1/G5 — 13+ aggregates' domain write-repository has zero implementing class anywhere in the
solution** — `RegulatoryCalendar`, `Culture`/`ScoreCards` (Analytics), `Certification`/`Compliance`/
`ManagementOfChange`/`Permits`/`Policy`/`Procedures` (Compliance, 7 aggregates), and
`EmergencyManagement`/`IncidentManagement`/`Observation` (SafetyEvents) — confirmed by grepping each
aggregate's fully-qualified domain-interface namespace against the whole solution. Calling `Create`
on any of these fails at DI-container resolution before a single SQL statement runs — a strictly
worse failure than "the stored procedure doesn't exist." **G2** found this trap recurs via a specific
mechanism twice more: a same-named-but-unrelated class (`CultureRepository`, `ProceduresDapperRepository`)
implements a *different* interface, which could mislead a shallow audit into concluding the write
path works.

Other findings: **G3** — the solution-wide count of confirmed silent-truncation bugs remains exactly
1 (`TriggerAlertCommand`); DataLake/Intelligence's different parameter-binding mechanism (Dapper
inferring size from value length) doesn't share this risk. **G4** — zero confirmed enum-drift
instances anywhere; every candidate remains structurally-possible-but-unconfirmed. Full per-aggregate
shape tables: [`sql-shape/*.md`](./sql-shape/) (25 files).

---

## 8. Complete RLS/Tenant/Actor Propagation Summary

From Phase 7 ([`security/rls-audit.md`](./security/rls-audit.md),
[`security/actor-propagation.md`](./security/actor-propagation.md)) — the audit's most consequential
phase, and the direct source of §1's sequencing trap.

**The SESSION_CONTEXT-never-engages finding, stated plainly:** SQL Server Row-Level Security depends
on `SetTenantSessionContextAsync` populating `SESSION_CONTEXT('TenantId')` — and that function only
fires if the calling repository method's own `DynamicParameters` set happens to include a `"TenantId"`
key. The "classic" `GetByIdAsync(id)`/`UpdateAsync(aggregate)` shape used by ~46+ of PlantSafety's 48
rich aggregates — including all 23 audit-logged ones — **never adds that key**. Confirmed by direct
read of `AccidentDapperRepository`, `RiskDapperRepository`, and `AuditFindingDapperRepository`'s read
path; pattern-inferred for the remainder on the strength of an identical generated shape (140
`GetByIdAsync`/`FindAsync` occurrences across 81 files, cross-referenced against 60
`WithCanonicalHeaders` usages, which never chain onto a `GetByIdAsync`). This means, for this call
shape, **RLS is not wrong — it is never engaged at all, by construction, on every single call.**

Two possible live states follow, and static analysis alone cannot distinguish them: **(a)** if the
RLS migration is actually applied, every `GetByIdAsync` silently returns null and every `UpdateAsync`
silently affects 0 rows, for every tenant — a solution-breaking, silent availability bug, not a
security control; **(b)** if the migration isn't applied, there is zero SQL-layer enforcement and the
Phase 2 tenancy gap (F1) stands completely undiminished. Either way, **RLS does not provide real
protection for the exact call shape every mutating lifecycle transition uses.** A second, newer
repository generation (`Features/*`, ~25 files) does pass `TenantId` correctly and does engage RLS —
but disproportionately overlaps with the 13 zero-implementation aggregates from §7, making the
"correct" behavior a coincidence on code nobody calls.

**Actor/correlation propagation — the totalizing finding:** consolidating this gap against the full
~344-transition inventory, **zero transitions anywhere in the solution have confirmed correct
end-to-end Actor/CorrelationId propagation.** Every transition lands in one of three buckets: hit by
the C#-outbox hardcode alone (~35 aggregates), hit by both the C# bug and the SQL `SYSTEM_USER`
binding (23 aggregates), or never reaches a repository at all because none exists (13
zero-implementation aggregates — "unaffected" only because the call can't execute). DataLake,
Intelligence, and Azure each fail a third, independently-worse way (no `ActorId` column/field at all
in DataLake/Azure's outbox; Intelligence narrows it to an unpreserved string).

Full detail and per-aggregate mapping: [`security/rls-audit.md`](./security/rls-audit.md),
[`security/actor-propagation.md`](./security/actor-propagation.md).

---

## 9. Complete Gap Register

20 deduplicated gaps, ranked by real-world severity, not by row count or module. Full text and
per-gap scope: [`gaps/_severity-ranking.md`](./gaps/_severity-ranking.md); citation codes explained
in [`gaps/_legend.md`](./gaps/_legend.md).

**Tier 0 — Critical (3 gaps):** #1 fixing the auth-policy-500 bug before the tenancy/RLS gap converts
an availability bug into a live cross-tenant exposure (§1); #2 SQL Server RLS never engages for the
load-by-ID/update-by-ID call shape every mutation uses, with two possible live states, both severe
(§8); #3 zero of ~344 transitions solution-wide have confirmed correct actor/correlation propagation
— "who did this" is unanswerable for every mutating action (§8).

**Tier 1 — High (5 gaps):** #4 13+ aggregates' write-repository has zero implementing class,
guaranteed DI-resolution failure (§7); #5 the systemic app-layer tenancy gap underlying #2 (§3, §8);
#6 Delete/hard-delete does not exist anywhere in PlantSafety, 93/93 aggregates — conditional severity
pending a legal retention/erasure review (§2); #7 SQL AuditLog covers only 22-23/93 aggregates,
best-effort and anonymous even there (§4); #8 two guarded-but-100%-dead lifecycles
(`Operational.Alerts.Alert`'s 9 transitions, `PMScheduling.Cancel`) representing wasted engineering
investment, not a live defect.

**Tier 2 — Medium (7 gaps):** #9 4 aggregates silently archive with no event (§3); #10 Intelligence's
handler-bypass, confirmed narrowly scoped to 2 aggregates/13 transitions (§3); #11 4 confirmed
cross-module/cross-field reference gaps, pattern likely wider (§4); #12 zero domain events reach a
working consumer anywhere (§6, inherited from the Ingestion Discovery); #13 duplicate-concept
clusters (Alert, Risk, Dashboard, Auditing, PermitWorkflow/PermitToWork, LOTO modeled 3×, PPE modeled
3×) creating compliance-reporting exposure, not runtime risk; #14 ~30 of 220 commands have no
validator class (§5); #15 the one confirmed silent-truncation bug, `TriggerAlertCommand` (§7).

**Tier 3 — Low/tech-debt (5 gaps):** #16 45 Create-only aggregates never grew past Create (§2); #17
Archive missing on 85/93 aggregates; #18 Cancel missing on 92/93 (subsumed by #8's one instance);
#19 no event carries the pre-transition value, moot until a consumer needs a delta (§6); #20
cosmetic/dead artifacts (`ConcurrencyToken`, `UpdatedBy` always reflecting `CreatedBy`, a copy-paste
exception message).

---

## 10. Complete Modernization Recommendations

From Phase 9 ([`recommendations/_cross-cutting.md`](./recommendations/_cross-cutting.md)), which
covers the fixes that are genuinely solution-wide/architectural, plus 25 per-module recommendation
files. **The required fix order is the single most consequential ordering decision in this entire
audit:**

```
XC-1 (Tenancy check + RLS engagement)  ──must land and be verified before──▶  XC-3 (Auth-policy registration)
XC-2 (Actor/correlation propagation)   ──independent, run in parallel with either──
```

- **XC-1 — Tenancy check + RLS engagement (do first).** Add an explicit `tenantId` parameter to every
  classic-pattern repository's `GetByIdAsync`/`UpdateAsync`, thread the authenticated caller's tenant
  through it (never client input), add the app-layer `if (aggregate.TenantId != callerTenantId) throw`
  check, and update the stored procedures to filter on `@TenantId` explicitly rather than relying on
  RLS alone. Effort: L (~40+ repository classes). **Determine whether the RLS migration is actually
  live against the running database first** — this cannot be resolved from source alone.
- **XC-3 — Register the ~293+ missing authorization policies (only after XC-1 is verified).** Pick
  one canonical registration path and finish wiring it; do not fix by stripping `Policy=` down to bare
  `[Authorize]`, which would convert this into an immediate cross-tenant leak on endpoints deriving
  TenantId from client input. Effort: S-M.
- **XC-2 — Actor/correlation propagation (independent, run any time/in parallel).** Route all 58
  `outboxWriter.Enqueue(...)` call sites through the already-built, already-correct
  `IUnitOfWork.EnqueueEvent(...)`; separately add an `@ActorId`/`@ChangedBy` parameter to the 23
  audit-logged aggregates' stored procedures to replace `SYSTEM_USER`. Effort: M. No dependency on
  XC-1/XC-3, so schedule based on capacity, not priority order.
- **XC-4 — Build the 13+ missing repository implementations**, using XC-1's `tenantId`-first pattern
  from the start so they don't need retrofitting. Prioritize by product need — several of these
  (the 9 `emerging-createonly.md` markers) may be aspirational/unwired features not yet needed;
  confirm scope with product owners before committing to all 13.
- **XC-5 — Expand AuditLog coverage** beyond the current 22-23 aggregates, correcting `ChangedBy`
  binding in the same pass as XC-2 rather than growing the population XC-2 has to fix later.
- **XC-6 — Decide, as a policy question, whether any PlantSafety data needs Delete/soft-delete** for
  right-to-erasure or retention-mandate reasons; do not add Delete to all 93 aggregates speculatively.

See [`recommendations/*.md`](./recommendations/) for the full 25-module per-aggregate recommendation
detail beyond these six cross-cutting items.

---

## 11. Appendix: Method & Confidence Notes

**What this audit is:** a systematic, static-source-only analysis — full, non-sampled structural
census of every `Aggregates/*.cs` file (Phase 1: 106 files, 93 real aggregates), followed by direct
full-body reads of a representative subset per structural pattern in each subsequent phase, with
every non-individually-read member of that group explicitly labeled **pattern-inferred** rather than
silently assumed. No code was executed at any point across all 10 phases — no running application,
no test invocation, no live database connection. Every "confirmed" claim means "confirmed by reading
source and its wiring," never "confirmed by observing runtime behavior."

**Where sampling and pattern-inference apply, stated plainly:**

- **The 45 Create-only aggregates** were confirmed at the structural/signature level (every file was
  scanned for method presence) but not all individually read line-by-line for full-body semantics —
  most were pattern-matched against a directly-read representative rather than opened in full.
- **SQL cross-referencing throughout the audit was file-name matching, not proc-body reading** — a
  plausibly-named stored procedure was treated as "SQL persistence exists," which cannot rule out a
  proc that exists, is called, and still has a latent correctness bug (the `TriggerAlertCommand`
  truncation bug is a concrete example of exactly that gap).
- **Tenancy/RLS findings (F1, §8) are directly confirmed for a handful of aggregates per module**
  (Accident, Risk, PermitWorkflow, MachineGuarding, AuditFinding, plus DataLake's `IngestionRepository`
  and Intelligence's `TrainingJobRepository`) and **pattern-inferred for the remaining ~40+
  classic-pattern aggregates** on the strength of an identical, code-generated repository shape.
- **Cross-module reference gaps (§9 gap #11)** are confirmed for exactly 4 instances and
  pattern-inferred wider, since the audit found every read command handler carries exactly one
  repository dependency — structurally, any aggregate storing a raw Guid FK to another module's
  aggregate is likely to share the same unvalidated-reference gap, but this was not individually
  re-verified for the full 48-aggregate rich population.

**Consistent with how the companion Ingestion Discovery blueprint treated this same caveat:**
this document's three Tier-0 findings — the sequencing trap (§1), the SESSION_CONTEXT-never-engages
finding (§8), and the zero-correct-actor-propagation finding (§8) — are the highest-stakes claims in
this entire audit, and each rests on strong-but-still-static evidence (direct code reads of the exact
mechanism, cross-referenced grep censuses with no exceptions found). **None of the three has been,
or can be, confirmed by watching a real request execute against a real database.** Static analysis
cannot rule out an environment-specific override, a migration applied outside this repository's
tracked history, or a compensating control configured at deploy time. Exactly as the Ingestion
Discovery recommended for its own highest-stakes claims (the ~300-endpoint 500 finding and the
zero-events-delivered finding), **this blueprint recommends running a live runtime or integration
test — hitting a sample of the affected endpoints with cross-tenant IDs, and checking whether
SESSION_CONTEXT is actually populated in a live query — before treating any of the three Tier-0
findings as fully certain enough to drive an incident response or gate a remediation sprint's
sequencing.** The cost of being wrong in either direction (assuming a live exposure that isn't real,
or dismissing one that is) is high enough on these three specific findings to warrant that
confirmation step ahead of everything else in this document.

---

*This blueprint synthesizes: [`inventory.md`](./inventory.md), [`module-inventory/`](./module-inventory/)
(25 files), [`domain-mapping/`](./domain-mapping/) (26 files), [`command-handler-map/`](./command-handler-map/)
(26 files), [`rule-enforcement/`](./rule-enforcement/) (26 files), [`event-map/`](./event-map/)
(26 files), [`sql-shape/`](./sql-shape/) (26 files), [`security/`](./security/) (2 files),
[`gaps/`](./gaps/) (27 files), and [`recommendations/`](./recommendations/) (26 files). Companion
document: [`../ingestion/blueprint.md`](../ingestion/blueprint.md).*
