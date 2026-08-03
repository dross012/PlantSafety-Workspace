# Gaps — SafetyPolicies

1 aggregate (`SafetyPolicy`). See `_legend.md`.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| SafetyPolicy | Cancel/Delete missing `[Inv]` | No — `Reject` is a genuine "undo" transition, as well-guarded as the forward `Approve` path, a positive control `[Inv]` | **Yes (suspected, not confirmed)** — `Archive` presumed to fold into `usp_SafetyPolicy_Update`; a 4th suspected (not directly re-verified) silent-archive instance `[Inv]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1+#2** (named in the 23-aggregate cluster) `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | **Yes (pattern-inferred)** — SafetyPolicy is one of the aggregates §2 of the RLS audit explicitly names as sharing the confirmed-blind classic shape `[RLS§2]` |

**Net for this module**: `RecordAcknowledgment` correctly requires `Published` status — a real invariant. This aggregate is one of the solution's best-guarded (Approve/Reject symmetry) yet is simultaneously in the worst-case bucket for tenancy, both actor bugs, and RLS — reinforcing that transition-richness and security-correctness are unrelated in this codebase, not merely at the AccidentManagement example but as a general pattern.
