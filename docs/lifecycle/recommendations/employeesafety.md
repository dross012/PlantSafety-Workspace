# Recommendations — EmployeeSafety

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. This module's 7
rich sub-aggregates are entirely new to this audit; `MachineGuarding` is one of only 3 aggregates in
the solution where the tenancy gap was directly verified (not pattern-inferred), making this module's
XC-1 application a load-bearing example rather than a routine one.

1. **Fully verify `EnvironmentalHealthMonitoring`'s guard coverage** — flagged (not confirmed) as
   having the lowest guard-to-method ratio of this cluster (1 guard site for 4 methods). Fix: a full
   read of this aggregate to confirm whether the other 3 transitions have real preconditions or are
   genuinely unguarded — this is the one concrete follow-up read this module's own gap file
   recommends. Cite: `gaps/employeesafety.md`. Effort: S (read) + variable if a real gap is
   confirmed. Risk: unconfirmed — could be Data integrity (missing guard) or a non-issue (guards
   exist but weren't counted the same way). Sequencing: none.

2. **Apply XC-1 (tenancy + RLS engagement) to this cluster, using `MachineGuarding` as the reference
   implementation** — `MachineGuardingDapperRepository.GetByIdAsync` is one of only 3 aggregates in
   the entire solution where F1's tenancy gap was **directly confirmed**, not pattern-inferred,
   making it a stronger proof point than most of the audit's other findings. Cite:
   `domain-mapping/_shared-findings.md` F1, `security/rls-audit.md` §2, `gaps/employeesafety.md`.
   Effort: part of XC-1's solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land
   before XC-3 (auth-policy registration) proceeds for this module's endpoints — MachineGuarding
   specifically should be one of the first fixes verified, given it's a directly-confirmed instance,
   not an assumed one.**

3. **Resolve `Ppe (EmployeeSafety)` vs. `PPE.PPEAssignment`'s possible undocumented overlap** — same
   method names (`RecordFitTest`/`FlagForReplacement`/`Retire`) across two aggregates, not confirmed
   as a duplicate but a real open question. Fix: a targeted comparison of both aggregates' actual
   persistence/SQL destinations to determine whether this is a genuine 7th duplicate-concept cluster
   or two legitimately distinct bounded contexts (plausible, given PPE also appears a 3rd time in
   `safetyprocedures.md`'s `PpePolicies`, governing the policy document rather than physical units).
   Coordinate with `ppe.md`'s equivalent item. Cite: `gaps/_severity-ranking.md` #13,
   `gaps/employeesafety.md`, `gaps/ppe.md`. Effort: S (investigation) + variable if consolidation is
   warranted. Risk: Data integrity (compliance-reporting risk) if genuinely duplicated. Sequencing:
   coordinate with `ppe.md`.

4. **Confirm how `LotoPrograms`/`MachineGuarding` instances actually come into existence** — no plain
   `Create` method name was found for either; they may be constructor-only or use a differently-named
   factory (`MachineGuarding` uses `Retire` as its terminal transition, consistent with a
   non-standard lifecycle naming convention this module uses). Fix: a quick read to confirm the
   actual instantiation path, documenting it if it's simply a naming convention difference (not a
   gap) so future audits don't re-flag it. Cite: `gaps/employeesafety.md`. Effort: S. Risk: none if
   confirmed as naming-convention-only; Availability if a genuine gap. Sequencing: none.

5. **Apply XC-2 (actor/correlation propagation) to this cluster's outbox calls** — all 7
   sub-aggregates hit by Bug#1 only (assumed, following the confirmed pattern at the other
   ~35-aggregate Bug#1-only cluster). Cite: `security/actor-propagation.md` §2b,
   `gaps/employeesafety.md`. Effort: part of XC-2's pass. Risk: Critical (see XC-2). Sequencing:
   none — independent, run any time.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
