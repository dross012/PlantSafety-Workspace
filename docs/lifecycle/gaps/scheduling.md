# Gaps — Scheduling

5 aggregates: `PMScheduling`, `WorkOrderPriority` (rich); `EmergencyDrillPlanning`,
`HazardIsolationSequencing`, `ShiftFatigue` (create-only, dormant Update procs). See `_legend.md`.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| PMScheduling | **Cancel exists** (the sole exception to "92 of 93 aggregates missing Cancel"), fully guarded, correctly raises `PMSchedulingCancelledEvent`, SQL-persistable — but **confirmed unreachable from any HTTP route**, the headline dead-lifecycle finding of the original discovery, reconfirmed structurally this audit `[Inv]` | No — Cancel's guard is real, simply unreachable, not bypassed `[Inv]` | No (Cancel does publish, if ever invoked); other transitions publish correctly `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1+#2** (Scheduling's 5 sub-aggregates named explicitly in the 23-cluster) `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | **Yes (pattern-inferred)** `[RLS§2]` |
| WorkOrderPriority | Archive/Cancel/Delete missing `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1+#2** (named in the 23) `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | **Yes (pattern-inferred)** `[RLS§2]` |
| EmergencyDrillPlanning | Update/Close/Cancel/Delete missing — Create-only, dormant `_Update` unused `[Inv]` | No | N/A (inherent) | N/A — Create-only | **Yes — Bug#1+#2 (named in the 23)** `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | N/A |
| HazardIsolationSequencing | Update/Close/Cancel/Delete missing (dormant `_Update` unused) `[Inv]` | No | N/A (inherent) | N/A | **Yes — Bug#1+#2 (named in the 23)** `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | N/A |
| ShiftFatigue | Update/Close/Cancel/Delete missing (dormant `_Update` unused) `[Inv]` | No | N/A (inherent) | N/A | **Yes — Bug#1+#2 (named in the 23)** `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | N/A |

**Net for this module**: `PMScheduling.Cancel` is the single most literal "missing transition" finding that isn't actually missing — it exists, fully built, correctly guarded and event-raising, and is simply never exposed via any route. All 5 aggregates share the 23-aggregate worst-case actor-attribution exposure.
