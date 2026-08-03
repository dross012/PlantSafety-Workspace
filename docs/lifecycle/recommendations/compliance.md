# Recommendations — Compliance

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. Compliance is the
largest single concentration of the XC-4 zero-implementation finding in the solution — all 8
aggregates (`Certification`, `Compliance`, `ManagementOfChange`, `Permits`, `Policy`, `Procedures`,
`RegulatoryCalendar`, plus the `Auditing` marker) are confirmed zero-implementation for their domain
write-repository interface. This module's recommendations are almost entirely "apply XC-4," not new
module-specific analysis — that itself is the key finding worth stating plainly.

1. **Apply XC-4 to all 7 real Compliance aggregates** (`Certification`, `Compliance`,
   `ManagementOfChange`, `Permits`, `Policy`, `Procedures`, `RegulatoryCalendar`) — `Create` on any of
   them fails at DI-container resolution today, before a single SQL statement runs. Do not build all
   7 speculatively; confirm with product owners which of these are actually needed in the near term
   (this module carries the largest un-scoped engineering estimate in the entire recommendations
   set). Cite: `gaps/_severity-ranking.md` #4 (ranked "the single worst — and most solution-wide —
   shape-alignment finding of the entire audit"), `sql-shape/_shared-findings.md` G1/G5,
   `gaps/compliance.md`. Effort: L (7 repository implementations from scratch; several also need
   stored procedures written from scratch). Risk: Availability — guaranteed runtime failure on
   `Create`. Sequencing: independent of XC-1/XC-2/XC-3 — these aggregates never reach the code paths
   those fixes touch until this lands.

2. **Prioritize `RegulatoryCalendar` first within the XC-4 scope** — it is the worst-confirmed case
   in the entire 13-aggregate population: zero classes implement `IRegulatoryCalendarRepository` at
   all (not even a naming-collision decoy), and the 2 procs its same-named Infrastructure class calls
   don't exist either. Cite: `command-handler-map/_shared-findings.md` F13, `sql-shape/_shared-findings.md`
   G5, `gaps/compliance.md`, `gaps/emerging-createonly.md`. Effort: part of item 1's L-effort pass,
   called out separately here because it's the clearest "start here" candidate if the team can only
   tackle one aggregate first. Risk: Availability (worst-confirmed instance). Sequencing: none.

3. **Resolve the naming-collision traps before writing new repository code** — `Procedures` has a
   same-named `ProceduresDapperRepository` class that implements an unrelated Application-layer
   interface, and `Permits`'s SQL-visible concept name (`PermitsToWork`) differs from the C# class
   name (`Permits`) — both landmines that could mislead whoever picks up item 1. Fix: rename or
   delete the misleading classes/document the naming mismatch clearly before or during the XC-4 work
   for these two aggregates, so the real implementation isn't confused with the decoy. Cite:
   `sql-shape/_shared-findings.md` G2, `gaps/compliance.md`. Effort: S (documentation/rename) as a
   prerequisite to item 1's work on these two aggregates specifically. Risk: Tech-debt/landmine —
   low severity alone, but compounds item 1's risk if not resolved first. Sequencing: **do before**
   writing the real `Permits`/`Procedures` repository implementations in item 1, to avoid the new
   code colliding with or being mistaken for the decoy classes.

4. **`Policy` has the largest SQL footprint of any Create-only aggregate (12 `usp_Policy*` procs)
   yet none is a genuine base-entity Create/Update** — worth a targeted read to confirm whether any
   of the 12 could be repurposed/extended rather than writing entirely new procedures as part of
   item 1's work. Fix: read the 12 existing procs before scoping the `Policy` repository build to
   avoid duplicating logic that may already partially exist. Cite: `sql-shape/_shared-findings.md`
   G1, `gaps/compliance.md`. Effort: S (investigation) as a prerequisite to `Policy`'s share of item
   1. Risk: none directly — an efficiency/scoping recommendation. Sequencing: do before, or as part
   of, item 1's `Policy` work.

5. **Formally retire the `Auditing` (Compliance marker) duplicate** — Create-only, read-only SQL,
   legacy/dead-ish, fully covered in `auditing.md`'s own recommendations. Cite: `gaps/compliance.md`,
   `gaps/auditing.md`. Effort: see `auditing.md` item 5. Sequencing: coordinate with the cross-module
   duplicate-concept cleanup effort referenced in `analytics.md`.

**Note on tenancy/RLS/actor-propagation (XC-1/XC-2)**: none of Compliance's 8 aggregates currently
reach a repository, so XC-1/XC-2 do not apply until XC-4 lands for each. When writing the new
repository implementations in item 1, build them using XC-1's correct `tenantId`-first pattern from
the start (mirroring the `Features/*` generation) rather than retrofitting it later, and route their
outbox writes through `IUnitOfWork.EnqueueEvent` (XC-2) from day one rather than copying the
hardcoded `"system"`/`Guid.Empty` pattern used elsewhere in the solution.

See `_cross-cutting.md` for XC-4 (zero-implementation repositories, this module's dominant finding).
