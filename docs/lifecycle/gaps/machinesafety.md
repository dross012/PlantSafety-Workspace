# Gaps — MachineSafety

6 rich sub-aggregates, entirely new to this audit. See `_legend.md`.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| EmergencyStopDevices | Archive/Cancel/Delete missing (uses `Retire` as terminal transition) `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only (assumed)** `[Actor§2b]` | No confirmed shape violation | **Yes (pattern-inferred)** `[RLS§2]` |
| LotoSystems | Archive/Cancel/Delete missing; conceptually parallel to `EmployeeSafety.LotoPrograms` (same isolation/verify/release shape) but not confirmed as a duplicate — LOTO legitimately spans multiple bounded contexts `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation | **Yes (pattern-inferred)** `[RLS§2]` |
| MachineGuardsBarriers | Archive/Cancel/Delete missing; same inspect/correct/verify shape as `EmployeeSafety.MachineGuarding` — related, not confirmed duplicate `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation | **Yes (pattern-inferred)** `[RLS§2]` |
| PresenceSensingDevices | Archive/Cancel/Delete missing `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation | **Yes (pattern-inferred)** `[RLS§2]` |
| SafetyValvesRelays | Archive/Cancel/Delete missing `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation | **Yes (pattern-inferred)** `[RLS§2]` |
| TwoHandControls | Archive/Cancel/Delete missing `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation | **Yes (pattern-inferred)** `[RLS§2]` |

**Net for this module**: the most internally-consistent feature area in the solution — all 6 aggregates use `Retire` as a uniform terminal transition (more consistent than the Archive/Close/Retire variance seen elsewhere), and none was found to have a bypass, cross-module reference, or silent-event gap. The residual gaps are entirely the cross-cutting tenancy/actor/RLS findings, applied uniformly (pattern-inferred, not individually re-confirmed this audit).
