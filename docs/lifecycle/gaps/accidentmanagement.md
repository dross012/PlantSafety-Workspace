# Gaps — AccidentManagement

1 aggregate (`Accident`). See `_legend.md` for citation codes. Best-covered lifecycle in the solution
per Phase 1, yet still fails all 4 cross-cutting security/attribution checks.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| Accident | Delete, Cancel missing `[Inv]` (plausibly correct-by-design for a regulatory record) | No — every transition guarded, incl. best-in-solution `CloseInvestigation` cross-field check `[Inv]`/`[RE-G4]` | **Yes** — `Archive` guards the transition but never calls `AddDomainEvent`; all other 7 transitions publish correctly (silent-archive pattern #1 of 4) `[DM-F3]` | **Yes** — classic `AccidentDapperRepository.GetByIdAsync(id)`/`UpdateAsync` carries no TenantId param `[DM-F1]`/`[RLS§2]` (direct read, not pattern-inferred) | **Yes — Bug#1+#2** — in the 23-aggregate cluster hit by both the C#-outbox hardcode and SQL `SYSTEM_USER` `[Actor§2a]` | No confirmed shape violation; 1 of 23 aggregates with real (best-effort, anonymous) AuditLog coverage `[CH-F8]`; `CreateCorrectiveActionCommand.Description` is an unconfirmed/likely truncation candidate, no instance observed `[SQL-G3]` | **Yes** — directly-read example case for the whole RLS finding: `usp_Accident_GetById` takes only `@AccidentId`, no tenant filter of its own, relies entirely on RLS, which never engages for this call shape `[RLS§2]` |

**Net for this module**: the aggregate with the richest guards, the best cross-field rule enforcement, and confirmed real (if best-effort) audit logging is nevertheless in the worst-case bucket for every security/attribution dimension — tenancy, RLS, and both actor-propagation bugs simultaneously. Coverage-richness and security-correctness are orthogonal axes in this codebase.
