# Recommendations — Auditing

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. Auditing
(`AuditEngagement`/`AuditFinding`) holds the solution's clearest confirmed cross-module reference gap
and is, ironically, not among the 23 aggregates with AuditLog coverage despite being the aggregate
literally named "Auditing" — this module's #1 item is module-specific, not a cross-cutting
cross-reference.

1. **Close the `AuditEngagement.Close` cross-module verification gap.** `Close` guards
   `Status==Active` and range-checks `ComplianceScore` (a real, non-cosmetic guard), but never
   checks whether the engagement's `AuditFinding`s are actually resolved, and accepts
   caller-supplied `ComplianceScore`/`ComplianceRating` with no cross-check against the Compliance
   module's actual state. Fix: add a repository dependency on open-`AuditFinding` status (query
   before allowing `Close`, reject if any finding is still open), and either derive
   `ComplianceScore`/`ComplianceRating` server-side from verified data or add a secondary
   verification step rather than trusting raw caller input. This is one of only 4 confirmed (not
   merely suspected) cross-module-reference gaps in the entire solution — a load-bearing example,
   not a minor one. Cite: `gaps/_severity-ranking.md` #11, `domain-mapping/_shared-findings.md` F6,
   `rule-enforcement/_shared-findings.md` G5, `gaps/auditing.md`. Effort: M (requires adding a second
   repository dependency to a handler that currently follows the single-repository Load→mutate→Save
   shape used everywhere else in the codebase — a genuine architectural change for this handler).
   Risk: **Data integrity** — an audit engagement can be closed today with unresolved findings and an
   unverified compliance score, undermining the entire point of the audit-engagement lifecycle.
   Sequencing: none — independent of the cross-cutting items.

2. **Add AuditLog coverage for `AuditEngagement`/`AuditFinding`** — neither aggregate is among the 23
   with even best-effort AuditLog coverage, despite being a rich, live state machine and the
   aggregate class literally named "Auditing." Fix: per XC-5, add the same `AuditLog` INSERT pattern
   the 23 covered aggregates have, using XC-2's corrected `ChangedBy` binding from the start (don't
   introduce a new `SYSTEM_USER` instance to fix later). Cite: `command-handler-map/_shared-findings.md`
   F8, `gaps/_severity-ranking.md` #7, `gaps/auditing.md`. Effort: S-M as part of XC-5's larger pass.
   Risk: Compliance — "sounds audited" and "is audited" are unrelated for this aggregate today.
   Sequencing: do alongside XC-2 (same mechanism).

3. **Apply XC-1 (tenancy + RLS engagement) to `AuditFindingDapperRepository`.** This repository is
   directly confirmed (not pattern-inferred) to have a blind read-by-id path, even though its
   `UpdateAsync` correctly uses `WithCanonicalHeaders` on the write side — the read path is the one
   that's blind, a useful concrete illustration that write-side correctness doesn't imply read-side
   correctness. Cite: `security/rls-audit.md` §2, `gaps/auditing.md`. Effort: part of XC-1's
   solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land before XC-3
   (auth-policy registration) proceeds for this module's endpoints.**

4. **Apply XC-2 (actor/correlation propagation) to `AuditEngagement`/`AuditFinding`'s outbox calls.**
   Both are hit by Bug#1 only (not in the 23-both-bugs cluster, since their procs don't carry the
   AuditLog block at all per item 2 above). Cite: `security/actor-propagation.md` §2b,
   `gaps/auditing.md`. Effort: part of XC-2's M-effort pass. Risk: Critical (see XC-2). Sequencing:
   none — independent, run any time.

5. **Formally retire the `Compliance.Auditing.Auditing` marker duplicate** — a Create-only, read-only
   (`usp_Auditing_GetLogs`), legacy/dead-ish duplicate of this module's live concept, and one of the
   22 no-op-consumer pairs. Fix: confirm dead status (plausible given read-only SQL), then remove per
   the same duplicate-concept cleanup approach recommended in `analytics.md`/`dashboards.md`/
   `riskmanagement.md`. Cite: `gaps/_severity-ranking.md` #13, `domain-mapping/_shared-findings.md`
   F5-adjacent, `gaps/auditing.md`, `gaps/compliance.md`. Effort: M. Risk: Tech-debt/landmine, not a
   live defect. Sequencing: coordinate with the cross-module duplicate-concept cleanup effort
   referenced in `analytics.md`.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3), XC-2 (actor propagation), and
XC-5 (AuditLog coverage expansion).
