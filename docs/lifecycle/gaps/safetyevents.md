# Gaps — SafetyEvents

3 create-only aggregates: `EmergencyManagement`, `IncidentManagement`, `Observation` — all 3 of 3
confirmed zero-implementation for their domain write-repository interface. See `_legend.md`.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| EmergencyManagement | Update/Close/Cancel/Delete missing — Create-only `[Inv]`; distinct from `EmployeeSafety.EmergencyPreparedness` (which has a full working Create/Update pair) — same real-world concern, two differently-shaped aggregates | No (unreachable — no repository to bypass) | N/A (inherent + unreachable) | N/A — no repository exists | **N/A — never reaches a repository** `[SQL-G1]`/`[Actor§2c]` | **Yes — zero implementation** `[SQL-G1]` | N/A — moot `[RLS§3]` |
| IncidentManagement | Update/Close/Cancel/Delete missing `[Inv]`; the thinnest of 3 "incident" concepts in the solution alongside `AccidentManagement.Accident` and `ContractorSafety.IncidentTrackingPerformanceEvaluation` | No (unreachable) | N/A (inherent + unreachable) | N/A | **N/A — never reaches a repository** `[SQL-G1]`/`[Actor§2c]` | **Yes — zero implementation** `[SQL-G1]` | N/A — moot |
| Observation | Update/Close/Cancel/Delete missing `[Inv]` | No (unreachable) | N/A (inherent + unreachable) | N/A | **N/A — never reaches a repository** `[SQL-G1]`/`[Actor§2c]` | **Yes — zero implementation** `[SQL-G1]` | N/A — moot |

**Net for this module**: all 3 of this module's aggregates are in the 13-aggregate `[SQL-G1]` zero-implementation cluster — a `Create` call against any of them fails at DI-container resolution, before a single SQL statement runs. This is a more severe failure than the "no genuine write proc found" framing used for `compliance.md`'s aggregates, because here the C# class that would call any stored procedure doesn't exist either.
