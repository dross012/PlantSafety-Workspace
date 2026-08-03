# Gaps — RiskManagement

2 parallel Risk-concept aggregates: `Risk` (top-level, rich, live) vs `Operational.RiskManagement`
(create-only marker, GET-only dead duplicate). See `_legend.md`.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| Risk (top-level) | Cancel/Delete missing `[Inv]` | No — real cross-field rule: `ApplyControlMeasure` requires justification for lower-effectiveness control hierarchy `[Inv]` | **Yes** — `Archive` guards the transition but never calls `AddDomainEvent`, same pattern as Accident (silent-archive #2 of 4) `[DM-F3]` | **Yes** — directly confirmed (not pattern-inferred): `RiskDapperRepository.GetByIdAsync` builds parameters from `RiskId` alone, no TenantId `[DM-F1]`/`[RLS§2]` | **Yes — Bug#1+#2** (named in the 23-aggregate cluster) `[Actor§2a]` | No confirmed shape violation; AuditLog-covered `[CH-F8]` | **Yes** — directly-confirmed example, `RiskDapperRepository.cs:70-72` `[RLS§2]` |
| RiskManagement (Operational duplicate) | Update/Delete/Cancel/status transitions all missing — Create-only marker `[Inv]` | No | N/A (inherent) | N/A — read-only SQL (`usp_RiskManagement_List` only, no write proc under this name), own controller confirmed GET-only, legacy/dead-ish `[Inv]` | N/A — no confirmed write path to carry an actor through | **Yes — no write path at all**: even if wired, `Create` has nowhere confirmed to persist to `[Inv]` | N/A — moot |

**Net for this module**: `Risk` is a directly-verified (not pattern-inferred) case for both F1's tenancy gap and RLS's SESSION_CONTEXT-never-engages finding — this is one of the 3 concrete examples the security audit built its conclusion from, not an extrapolation. `RiskManagement` is the clearest of the solution's 3+ "dead GET-only duplicate" shapes (with Alert shape #2 and `Compliance.Auditing`).
