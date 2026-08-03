# Recommendations — MachineSafety

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. This is the most
internally-consistent feature area in the solution — all 6 aggregates use `Retire` as a uniform
terminal transition, and none was found to have a bypass, cross-module reference, or silent-event
gap. The residual gaps here are entirely the cross-cutting tenancy/actor/RLS findings, applied
uniformly — there is very little module-specific work beyond two open naming-overlap questions.

1. **Apply XC-1 (tenancy + RLS engagement) to all 6 sub-aggregates** (`EmergencyStopDevices`,
   `LotoSystems`, `MachineGuardsBarriers`, `PresenceSensingDevices`, `SafetyValvesRelays`,
   `TwoHandControls`) — pattern-inferred classic repository shape across the cluster, consistent
   with the rest of the newly-discovered rich-aggregate population. Cite:
   `domain-mapping/_shared-findings.md` F1, `gaps/machinesafety.md`. Effort: part of XC-1's
   solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land before XC-3 (auth-policy
   registration) proceeds for this module's endpoints.**

2. **Apply XC-2 (actor/correlation propagation) to all 6 sub-aggregates' outbox calls** — all hit by
   Bug#1 only (assumed, per the confirmed pattern at the other ~35-aggregate cluster). Cite:
   `security/actor-propagation.md` §2b, `gaps/machinesafety.md`. Effort: part of XC-2's pass. Risk:
   Critical (see XC-2). Sequencing: none — independent, run any time.

3. **Resolve `LotoSystems`'s conceptual overlap with `EmployeeSafety.LotoPrograms`** — same
   isolation/verify/release shape, not confirmed as a duplicate; LOTO (lockout-tagout) legitimately
   spans multiple bounded contexts, so this may be intentional. This is 1 of 3 LOTO-named aggregates
   solution-wide (the 3rd is `SafetyProcedures.LotoProcedures`). Fix: a coordinated 3-way review
   (with `employeesafety.md` and `safetyprocedures.md`) to confirm whether these are genuinely 3
   distinct bounded-context views of the same physical procedure (plausible and likely fine — e.g.
   equipment-level isolation state vs. employee-training records vs. the written procedure document
   itself) or an unintentional split needing a shared identity/cross-reference. Cite:
   `gaps/_severity-ranking.md` #13, `gaps/machinesafety.md`, `gaps/employeesafety.md`,
   `gaps/safetyprocedures.md`. Effort: M (3-module review). Risk: low-to-medium — plausibly correct
   by design, but flagged given the general pattern of duplicate concepts elsewhere in the solution
   turning out to be genuine data-integrity risks (e.g. `PermitWorkflow`/`ContractorSafety.PermitToWork`).
   Sequencing: coordinate with `employeesafety.md` item 3 (LotoPrograms) and the equivalent note in
   `safetyprocedures.md`.

4. **Resolve `MachineGuardsBarriers`'s conceptual overlap with `EmployeeSafety.MachineGuarding`** —
   same inspect/correct/verify shape, related but not confirmed duplicate. Fix: same review approach
   as item 3, coordinated with `employeesafety.md`. Cite: `gaps/machinesafety.md`,
   `gaps/employeesafety.md`. Effort: S (part of a combined review with item 3, since both are
   MachineSafety/EmployeeSafety naming-overlap questions). Risk: low — plausibly distinct concepts
   (device-level guards/barriers vs. general machine-guarding inspection). Sequencing: combine with
   item 3's review scope.

5. **No further module-specific action needed** — this module's own gap file explicitly notes no
   bypass, silent-archive, or cross-module-reference gap was found across the cluster, unlike most
   other rich-aggregate modules. Preserve this as a confirmed clean bill of health on rule-enforcement
   grounds specifically (tenancy/RLS/actor gaps still apply per items 1-2). Cite:
   `gaps/machinesafety.md`. Effort: none. Risk: none. Sequencing: none.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
