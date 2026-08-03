# Recommendations — Emerging/Create-Only Top-Level Aggregates

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. This cluster (11
marker-ish domains: `AiGovernance`, `AiSafetyIntelligence`, `ManagementOfChange`/`RegulatoryCalendar`
[Compliance, covered in `compliance.md`], `ContractorVisitor`, `Governance`, `HumanFactors`,
`OperationalSafety`, `ProcessDefinition`, `Sustainability`, `WorkerWellBeing`) has 10 of 11 aggregates
with **zero confirmed SQL persistence of any kind** — the single most concrete "would fail/no-op if
called" finding in the entire audit. This is fundamentally a scoping question, not an engineering
backlog: most of this module's recommendations are "find out if these are needed" before "build
them."

1. **Confirm with product/architecture which of these 9 zero-SQL aggregates
   (`AiGovernance`, `AiSafetyIntelligence`, `ContractorVisitor`, `Governance`, `HumanFactors`,
   `OperationalSafety`, `ProcessDefinition`, `Sustainability`) are genuinely planned features vs.
   scaffolded-and-abandoned.** This is the single highest-value action for this module — every other
   item below is contingent on this answer, and building 9 repository+SQL implementations
   speculatively (a multi-month effort) would be a serious misallocation if half of these are dead
   scaffolding. Cite: `gaps/_severity-ranking.md` #4 context, `sql-shape/_shared-findings.md` G1,
   `gaps/emerging-createonly.md`. Effort: S (a scoping conversation, not code). Risk: none directly —
   this is a prioritization gate for the rest of this file. Sequencing: **do this before any XC-4
   work is scheduled for this cluster.**

2. **For any aggregate confirmed as a live near-term priority, apply XC-4** (write the repository
   implementation, write the missing SQL from scratch since none exists at all, register in DI).
   `WorkerWellBeing` is the one exception with *some* SQL (`_GetById`/`_List` only, no write proc) —
   start there if any single aggregate in this cluster needs to become real first, since it's
   furthest along. Cite: `gaps/_severity-ranking.md` #4, `gaps/emerging-createonly.md`. Effort: L
   per aggregate confirmed needed (repository + SQL from scratch is a larger lift than XC-4's other
   12 aggregates, which at least have SQL to build against in most cases). Risk: Availability —
   `Create` fails at DI resolution for any of these called today. Sequencing: gated on item 1;
   independent of XC-1/XC-2/XC-3 otherwise (these aggregates never reach those code paths until a
   repository exists).

3. **`ProcessDefinition` is the one aggregate in this cluster with real domain-layer validation
   (2 genuine guard sites) despite zero SQL/events** — a domain factory with real validation and
   nowhere to persist to. Flag this one specifically to product as the strongest "someone already
   started building this correctly, it just needs a persistence layer" candidate, ahead of the other
   8 which have thinner domain layers. Cite: `gaps/emerging-createonly.md`. Effort: none additional
   (a prioritization note for item 1's conversation). Risk: none. Sequencing: fold into item 1.

4. **Do not re-flag `DigitalTwin`/`EnterpriseFabric` as belonging to this zero-SQL cluster** — both
   were checked this audit and confirmed to have real SQL under a shortened-noun naming convention
   (`Twin`/`FabricNode`). This is a negative-finding worth preserving so a future audit doesn't
   re-investigate a already-cleared false positive. Cite: `gaps/emerging-createonly.md`. Effort:
   none. Risk: none — a documentation/audit-hygiene note. Sequencing: none.

5. **`ManagementOfChange`/`RegulatoryCalendar` (the 2 Compliance-module overlaps in this cluster) are
   already covered by `compliance.md`'s recommendations** — do not duplicate remediation effort here;
   this file defers to `compliance.md` items 1-2 for those two specifically. Cite: `gaps/compliance.md`,
   `gaps/emerging-createonly.md`. Effort: see `compliance.md`. Sequencing: see `compliance.md`.

See `_cross-cutting.md` XC-4 for the shared engineering pattern. Tenancy/RLS/actor-propagation
(XC-1/XC-2) do not apply to any aggregate in this cluster until a repository exists for it.
