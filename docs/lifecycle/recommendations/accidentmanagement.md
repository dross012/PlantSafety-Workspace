# Recommendations — AccidentManagement

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. AccidentManagement
is the solution's best-covered lifecycle (1 aggregate, `Accident`, richest guards, best cross-field
rule enforcement) yet fails every cross-cutting security/attribution check — this module is a pure
consumer of the cross-cutting fixes in `_cross-cutting.md`, not a source of new module-specific
engineering work beyond the silent-archive item below.

1. **Apply XC-1 (tenancy check + RLS engagement) to `AccidentDapperRepository`.** `Accident` is one
   of the audit's own directly-confirmed examples (not pattern-inferred) — `GetByIdAsync`/
   `UpdateAsync` carry no `TenantId` parameter, and `usp_Accident_GetById` has no tenant filter of
   its own, relying entirely on RLS, which never engages for this call shape. Cite:
   `gaps/_severity-ranking.md` #2/#5, `gaps/accidentmanagement.md`, `security/rls-audit.md` §2 (this
   is the file's own worked example). Effort: part of XC-1's L-effort solution-wide pass — treat
   `Accident` as the reference implementation others are modeled from, since the audit already used
   it as the load-bearing example. Risk: Critical (see XC-1). **Sequencing: this repository's fix
   must land and be verified before `_cross-cutting.md` XC-3 (auth-policy registration) proceeds
   solution-wide — do not treat Accident as "already secure" because it's well-guarded; guard
   quality and tenancy enforcement are unrelated axes here.**

2. **Apply XC-2 (actor/correlation propagation) to `Accident`'s outbox and AuditLog.** `Accident` is
   in the 23-aggregate cluster hit by both the C#-outbox hardcode and the SQL `SYSTEM_USER` bug
   simultaneously — one of the "richest, best-guarded" aggregates a shallow audit would assume is
   well-instrumented. Cite: `security/actor-propagation.md` §2a, `gaps/accidentmanagement.md`.
   Effort: part of XC-2's M-effort pass. Risk: Critical (see XC-2). Sequencing: none — proceed any
   time, independent of items 1/3/4 below.

3. **Fix the silent-Archive gap: `Accident.Archive` guards the transition but never calls
   `AddDomainEvent`.** This is silent-archive instance #1 of 4 confirmed in the solution. Fix:
   confirm with compliance stakeholders whether archival needs to be observable downstream (e.g. for
   retention tooling); if yes, add the missing `AddDomainEvent(AccidentArchivedEvent)` call — small,
   mechanical, all other 7 transitions on this aggregate already publish correctly, so the pattern to
   copy is in the same file. Cite: `gaps/_severity-ranking.md` #9, `domain-mapping/_shared-findings.md`
   F3, `gaps/accidentmanagement.md`. Effort: S. Risk: **None-just-tech-debt** if archival is
   intentionally silent (plausible — this pattern recurs identically across `Risk`/`Scorecard`/
   `SafetyPolicy`, suggesting deliberate design, not 4 independent oversights); **data-integrity/
   compliance gap** if a retention-tracking consumer is ever built expecting this signal. Sequencing:
   none — independent of every other item in this document.

4. **AuditLog robustness**: `Accident` is 1 of the 23 aggregates with real (if best-effort,
   anonymous) AuditLog coverage — the highest-value candidate to pilot the XC-5 fix (logged failure
   instead of empty-`CATCH` swallow, `ChangedBy` bound to the real actor rather than `SYSTEM_USER`,
   `Module`/`Action` distinguishing which of Accident's many transitions wrote a given row), given
   how much this aggregate is already relied upon as the "working example" elsewhere in the audit.
   Cite: `command-handler-map/_shared-findings.md` F8, `gaps/_severity-ranking.md` #7. Effort: S
   (pilot on this one aggregate) as part of XC-5's larger M/L pass. Risk: Compliance (see XC-5).
   Sequencing: do alongside/after item 2 (XC-2) — same `ChangedBy`/`AuditLog` mechanism; building
   XC-5's fix on top of the still-broken `SYSTEM_USER` binding just duplicates work.

5. **`CreateCorrectiveActionCommand.Description` is an unconfirmed/likely truncation candidate** (no
   instance directly observed, unlike the confirmed `TriggerAlertCommand` bug in `alerts.md`).
   Fix: verify the FluentValidation `MaximumLength` on this field is ≤ the SQL column's actual size;
   add/tighten if missing, following the same audit method used to confirm the Alerts truncation bug.
   Cite: `sql-shape/_shared-findings.md` G3 note, `gaps/accidentmanagement.md`. Effort: S. Risk: Data
   integrity (silent truncation) if unconfirmed and the gap turns out to be real; low-probability
   given this module's otherwise strong validator coverage. Sequencing: none.

See `_cross-cutting.md` for full detail on XC-1 (tenancy+RLS), XC-2 (actor propagation), XC-3
(auth-policy registration — do not start on this module or any other until XC-1 is verified
solution-wide), and XC-5 (AuditLog coverage).
