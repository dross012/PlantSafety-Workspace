# Recommendations — SafetyProcedures

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. This module's 8
rich sub-aggregates are the largest single "newly discovered rich" cluster in the audit, showing 2
consistent, genuine design sub-patterns (6 document-lifecycle, 2 inspection-cycle aggregates) — a
real design pattern, not inconsistency. This module also contributes the most naming-overlap
candidates of any single module (LOTO, PPE, Auditing all recur here) — resolving those is the
priority ahead of the generic cross-cutting work.

1. **Participate in the 3-way LOTO naming-overlap review** (`LotoProcedures` here, alongside
   `EmployeeSafety.LotoPrograms` and `MachineSafety.LotoSystems`) — already scoped in
   `machinesafety.md` item 3; this file defers to that item's review approach. `LotoProcedures`'s
   distinguishing characteristic is that it governs the written procedure document, plausibly a
   genuinely distinct bounded context from the other two's equipment/employee-training focus. Cite:
   `gaps/_severity-ranking.md` #13, `gaps/safetyprocedures.md`, `gaps/machinesafety.md`,
   `gaps/employeesafety.md`. Effort/Risk: see `machinesafety.md` item 3. Sequencing: coordinate with
   `machinesafety.md` and `employeesafety.md`.

2. **Resolve `PpePolicies`'s relationship to `PPE.PPEAssignment`/`EmployeeSafety.Ppe`** — a 3rd
   PPE-adjacent aggregate, but distinct in kind (it governs the policy *document*, not physical
   units), which this module's own gap file already correctly distinguishes from a duplicate. Fix:
   confirm this distinction explicitly as part of the `ppe.md`/`employeesafety.md` 2-way
   investigation (item 1 in `ppe.md`), ruling `PpePolicies` in or out rather than leaving it as an
   unexamined 3rd data point. Cite: `gaps/safetyprocedures.md`, `gaps/ppe.md`. Effort: S (folds into
   the existing `ppe.md`/`employeesafety.md` investigation). Risk: low — plausibly already correctly
   distinct. Sequencing: coordinate with `ppe.md` item 1.

3. **Resolve `RoutineSafetyAuditsIncidentReporting`'s relationship to `AuditEngagement`/`AuditFinding`
   and `Compliance.Auditing`** — a 4th Auditing-adjacent aggregate (inspection-cycle shape), not
   confirmed as overlapping. Fix: a brief scope-clarification review across all 4 Auditing-adjacent
   concepts (this one, `AuditEngagement`/`AuditFinding`, and the dead `Compliance.Auditing` marker) to
   confirm which are genuinely distinct bounded contexts vs. candidates for consolidation, coordinated
   with `auditing.md`. Cite: `gaps/_severity-ranking.md` #13, `gaps/safetyprocedures.md`,
   `gaps/auditing.md`. Effort: M (4-way review). Risk: low-to-medium, similar class to the other
   duplicate-concept risks in the solution. Sequencing: coordinate with `auditing.md` item 5.

4. **Apply XC-1 (tenancy + RLS engagement) to all 8 sub-aggregates** — pattern-inferred classic
   repository shape across the entire cluster. Cite: `domain-mapping/_shared-findings.md` F1,
   `gaps/safetyprocedures.md`. Effort: part of XC-1's solution-wide pass. Risk: Critical (see XC-1).
   **Sequencing: must land before XC-3 (auth-policy registration) proceeds for this module's
   endpoints.**

5. **Apply XC-2 (actor/correlation propagation) to all 8 sub-aggregates' outbox calls** — all hit by
   Bug#1 only (assumed, per the confirmed ~35-aggregate cluster pattern). Cite:
   `security/actor-propagation.md` §2b, `gaps/safetyprocedures.md`. Effort: part of XC-2's pass.
   Risk: Critical (see XC-2). Sequencing: none — independent, run any time.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
