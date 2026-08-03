# Gaps — SafetyCulture

2 aggregates: `CultureAssessment`, `StopWorkEvent`. See `_legend.md`. (Distinct from `Analytics.Culture` —
same root word, unrelated concept, see `analytics.md`.)

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| CultureAssessment | Archive/Cancel/Delete missing `[Inv]` | No — real range-check (0-100) on both score fields, a positive control `[Inv]` | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only (assumed)** `[Actor§2b]` | No confirmed shape violation; not AuditLog-covered `[CH-F8]` | **Yes (pattern-inferred)** `[RLS§2]` |
| StopWorkEvent | Archive/Cancel/Delete missing `[Inv]` | No (pattern-inferred) | Pattern-inferred correct `[EV-F14]` | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation | **Yes (pattern-inferred)** `[RLS§2]` |

**Net for this module**: unremarkable relative to the rest of the solution — no bypass, no silent-archive, no confirmed cross-module gap. The residual exposure is entirely the universal tenancy/actor/RLS findings.
