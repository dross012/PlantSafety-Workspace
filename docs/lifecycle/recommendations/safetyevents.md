# Recommendations — SafetyEvents

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. All 3 aggregates
(`EmergencyManagement`, `IncidentManagement`, `Observation`) are confirmed zero-implementation for
their domain write-repository interface — this module is entirely an XC-4 application, plus one
naming-overlap question worth resolving before building anything new.

1. **Apply XC-4 to all 3 aggregates.** `Create` on any of them fails at DI-container resolution
   before a single SQL statement runs — a more severe failure than the "no genuine write proc found"
   framing used for `compliance.md`'s aggregates, because here the C# class that would call any
   stored procedure doesn't exist either. Confirm with product owners which of these are actually
   needed before building all 3 speculatively. Cite: `gaps/_severity-ranking.md` #4,
   `sql-shape/_shared-findings.md` G1, `gaps/safetyevents.md`. Effort: L (3 repository
   implementations from scratch). Risk: Availability — guaranteed runtime failure on `Create`.
   Sequencing: independent of XC-1/XC-2/XC-3; build using XC-1's correct `tenantId`-first pattern and
   route outbox writes through `IUnitOfWork.EnqueueEvent` (XC-2) from day one.

2. **Resolve the naming/scope distinction between `IncidentManagement` (this module),
   `AccidentManagement.Accident`, and `ContractorSafety.IncidentTrackingPerformanceEvaluation`** — 3
   "incident"-adjacent concepts in the solution, and `IncidentManagement` here is confirmed the
   thinnest of the 3. Fix: before investing in item 1's repository build for `IncidentManagement`
   specifically, confirm with product whether this concept is meant to be genuinely distinct from
   `Accident` (plausible — a broader "incident" concept vs. a specifically OSHA-reportable accident)
   or redundant with it. Building a full repository for a concept that turns out to be redundant with
   the solution's already-rich `Accident` aggregate would be wasted effort. Cite:
   `gaps/_severity-ranking.md` #13, `gaps/safetyevents.md`, `gaps/accidentmanagement.md`,
   `gaps/contractorsafety.md`. Effort: S (scoping conversation) as a prerequisite to `IncidentManagement`'s
   share of item 1. Risk: none directly — a prioritization gate. Sequencing: do before committing
   full XC-4 engineering effort to `IncidentManagement` specifically.

3. **Distinguish `EmergencyManagement` (this module, zero-implementation) from
   `EmployeeSafety.EmergencyPreparedness`** (which has a full, working Create/Update pair) — same
   real-world concern, two differently-shaped aggregates. Fix: confirm whether `EmergencyManagement`
   here is meant to be a distinct top-level concept or should be consolidated with/superseded by
   `EmployeeSafety.EmergencyPreparedness`'s already-working implementation, before building item 1's
   repository for it from scratch. Cite: `gaps/safetyevents.md`, `gaps/employeesafety.md`. Effort: S
   (scoping conversation). Risk: none directly — avoids duplicating a working concept. Sequencing:
   do before committing XC-4 effort to `EmergencyManagement` specifically.

4. **Build `Observation`'s repository third, after resolving items 2-3** — it has no confirmed
   naming/scope overlap with anything else in the solution, making it the most straightforward of the
   3 to simply build once confirmed needed. Cite: `gaps/safetyevents.md`. Effort: part of item 1's
   L-effort pass. Risk: Availability. Sequencing: least entangled of the 3 — proceed once product
   confirms it's needed, no cross-module scoping question to resolve first.

5. **No tenancy/RLS/actor-propagation work applies to this module until item 1 lands** — none of
   these 3 aggregates reach a repository today, so XC-1/XC-2 do not apply. Record this explicitly so
   this module isn't mistakenly included in the XC-1/XC-2 solution-wide rollout scope before its
   repositories exist. Cite: `gaps/safetyevents.md`. Effort: none. Risk: none. Sequencing: N/A —
   scoping note only.

See `_cross-cutting.md` XC-4 for the shared engineering pattern.
