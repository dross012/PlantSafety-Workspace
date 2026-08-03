# Gaps — PPE

5 aggregates: `PPEAssignment`, `PPEInventory` (rich); `PPECategory`, `PPETrainingFit`, `PPEZoneMapping`
(create-only, dormant Update procs). See `_legend.md` and `employeesafety.md` for the possibly-related
`EmployeeSafety.Ppe` (6th, differently-named PPE-concept aggregate, not confirmed duplicate).

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| PPEAssignment | Archive/Cancel/Delete missing `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1+#2** (named in the 23-aggregate cluster) `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | **Yes (pattern-inferred)** `[RLS§2]` |
| PPEInventory | Archive/Cancel/Delete missing (best-guarded of the PPE group — 6 guard + 6 event sites) `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1+#2** (named in the 23) `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | **Yes (pattern-inferred)** `[RLS§2]` |
| PPECategory | Update/Close/Cancel/Delete missing — Create-only, dormant `_Update` unused `[Inv]` | No | N/A (inherent) | N/A — Create-only | **Yes — Bug#1+#2 (named in the 23)** `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | N/A |
| PPETrainingFit | Update/Close/Cancel/Delete missing (dormant `_Update` unused) `[Inv]` | No | N/A (inherent) | N/A | **Yes — Bug#1+#2 (named in the 23)** `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | N/A |
| PPEZoneMapping | Update/Close/Cancel/Delete missing (dormant `_Update` unused) `[Inv]` | No | N/A (inherent) | N/A | **Yes — Bug#1+#2 (named in the 23)** `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | N/A |
| EmployeeSafety.Ppe (possible duplicate) | See `employeesafety.md` | See `employeesafety.md` | See `employeesafety.md` | See `employeesafety.md` | See `employeesafety.md` | See `employeesafety.md` | See `employeesafety.md` |

**Net for this module**: all 5 PPE aggregates are in the 23-aggregate cluster hit by both actor-propagation bugs and are AuditLog-covered — the best-instrumented category (audit-wise) in the solution, and still fully exposed on tenancy/RLS/actor attribution. `PPEAssignment` vs. `EmployeeSafety.Ppe`'s method-name overlap (`RecordFitTest`/`FlagForReplacement`/`Retire`) remains an unresolved possible duplicate.
