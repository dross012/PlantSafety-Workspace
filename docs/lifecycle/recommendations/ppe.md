# Recommendations — PPE

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. All 5 PPE
aggregates are in the 23-aggregate cluster hit by both actor-propagation bugs and are AuditLog-covered
— the best-instrumented category (audit-wise) in the solution, and still fully exposed on tenancy/
RLS/actor attribution. This module's real open question is the duplicate-concept overlap with
`EmployeeSafety.Ppe`.

1. **Resolve `PPEAssignment` vs. `EmployeeSafety.Ppe`'s possible undocumented overlap** — same
   method-name shape (`RecordFitTest`/`FlagForReplacement`/`Retire`), not confirmed as a duplicate.
   This is the same item flagged in `employeesafety.md` item 3 — defer to that item's investigation
   approach, coordinated across both modules. Note this is potentially a 3-way overlap once
   `SafetyProcedures.PpePolicies` (governing the policy *document*, a distinct concept per its own
   gap file) is considered, so the investigation should explicitly rule PpePolicies in or out rather
   than treating this as a simple 2-way comparison. Cite: `gaps/_severity-ranking.md` #13,
   `gaps/ppe.md`, `gaps/employeesafety.md`, `gaps/safetyprocedures.md`. Effort: S (investigation) +
   variable if consolidation is warranted. Risk: Data integrity (compliance-reporting risk) if
   genuinely duplicated. Sequencing: coordinate with `employeesafety.md` item 3.

2. **Apply XC-1 (tenancy + RLS engagement) to all 5 aggregates** (`PPEAssignment`, `PPEInventory`,
   `PPECategory`, `PPETrainingFit`, `PPEZoneMapping`) — pattern-inferred classic repository shape,
   all named in the 23-aggregate audit-logged cluster. Cite: `domain-mapping/_shared-findings.md` F1,
   `gaps/ppe.md`. Effort: part of XC-1's solution-wide pass. Risk: Critical (see XC-1). **Sequencing:
   must land before XC-3 (auth-policy registration) proceeds for this module's endpoints.**

3. **Apply XC-2 (actor/correlation propagation) to all 5 aggregates** — the named-23 cluster hit by
   both bugs simultaneously. Cite: `security/actor-propagation.md` §2a, `gaps/ppe.md`. Effort: part
   of XC-2's pass. Risk: Critical (see XC-2). Sequencing: none — independent, run any time.

4. **Wire the 3 dormant `_Update` procs** (`PPECategory`, `PPETrainingFit`, `PPEZoneMapping`) if
   update capability is needed — SQL already exists, unused. Cite: `gaps/ppe.md`,
   `gaps/_severity-ranking.md` #16. Effort: S per aggregate if confirmed needed. Risk:
   Product-completeness gap. Sequencing: none.

5. **Preserve `PPEInventory`'s guard density (6 guard + 6 event sites, the best-guarded of the PPE
   group) as a positive-control reference** when doing consolidation work in item 1 — if
   `PPEAssignment`/`EmployeeSafety.Ppe` are ever merged, use `PPEInventory`'s guard pattern as the
   template for the merged aggregate's rule-enforcement, not the thinner of the two candidates. Cite:
   `gaps/ppe.md`. Effort: none (guidance for item 1's execution). Risk: none. Sequencing: apply during
   item 1's consolidation work, if consolidation is the outcome.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
