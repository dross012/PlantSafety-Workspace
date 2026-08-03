# Recommendations — PermitToWork

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. `PermitWorkflow`
is a load-bearing example for 3 separate cross-cutting findings simultaneously (directly-confirmed
tenancy blindness, both actor bugs, and the flagged duplicate-concept cluster with
`ContractorSafety.PermitToWork`) — this module's fixes are almost entirely "apply the cross-cutting
item correctly, using this aggregate as the reference case," plus the one real module-specific
decision below.

1. **Resolve the `PermitWorkflow` vs. `ContractorSafety.PermitToWork` duplicate-concept risk** —
   confirmed distinct-but-overlapping, same real-world subject (a work permit), no shared identity.
   This is the same item flagged in `contractorsafety.md` item 1 — this file defers to that item's
   fix description and cross-references it here rather than duplicating. Cite:
   `domain-mapping/_shared-findings.md` F5, `gaps/_severity-ranking.md` #13, `gaps/permittowork.md`,
   `gaps/contractorsafety.md`. Effort/Risk/Sequencing: see `contractorsafety.md` item 1 — this is a
   two-module decision, not resolvable from either file alone.

2. **Apply XC-1 (tenancy + RLS engagement) to `PermitWorkflow`, using it as the reference
   implementation** — `PermitToWorkDapperRepository.GetByIdAsync` is one of only 3 aggregates in the
   solution where F1's tenancy gap was **directly confirmed**, not pattern-inferred, and is
   explicitly cited by `security/rls-audit.md` as part of the working example set the whole finding
   was built from. Cite: `domain-mapping/_shared-findings.md` F1, `security/rls-audit.md` §2,
   `gaps/permittowork.md`. Effort: part of XC-1's solution-wide pass — treat as a priority instance
   given it's directly confirmed. Risk: Critical (see XC-1). **Sequencing: must land and be verified
   before XC-3 (auth-policy registration) proceeds — this aggregate specifically should be one of
   the first fixes confirmed, given it's a directly-confirmed instance the audit itself relied on.**

3. **Apply XC-2 (actor/correlation propagation) to `PermitWorkflow`/`PermitTemplate`/
   `PermitExpiration`/`PermitScheduling`** — all 4 are named explicitly in the 23-aggregate
   both-bugs cluster. Cite: `security/actor-propagation.md` §2a, `gaps/permittowork.md`. Effort:
   part of XC-2's pass. Risk: Critical (see XC-2). Sequencing: none — independent, run any time.

4. **Verify `PermitWorkflow.Archive`'s silent-archive status** — presumed (not directly re-verified
   this audit) to fold into `usp_PermitWorkflow_Update` without publishing an event, following the
   pattern confirmed in `Accident`/`Risk`/`Scorecard`. Fix: a direct read of `PermitWorkflow.Archive`
   to confirm whether it publishes an event or not; if silent, apply the same fix recommended in
   `accidentmanagement.md` item 3 (confirm with compliance whether this needs to be observable, add
   the missing `AddDomainEvent` call if so). Cite: `domain-mapping/_shared-findings.md` F3,
   `gaps/permittowork.md`. Effort: S (verification) + S (fix, if confirmed silent and needed). Risk:
   None-just-tech-debt if confirmed intentional; data-integrity/compliance gap if a retention
   consumer is ever built expecting this signal. Sequencing: none.

5. **Wire the 2 dormant `_Update` procs** (`PermitExpiration`, `PermitScheduling`) if update
   capability is actually needed for these Create-only aggregates — SQL already exists, unused.
   Cite: `gaps/permittowork.md`, `gaps/_severity-ranking.md` #16. Effort: S per aggregate if
   confirmed needed. Risk: Product-completeness gap. Sequencing: none.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
Note: `PermitWorkflow`'s guard (`EnsureStatusIsOneOf`) was directly re-read and confirmed genuine on
every transition (`rule-enforcement/_shared-findings.md` G4) — its rule-enforcement is solid even
though its security posture is not; do not conflate the two when prioritizing this module's work.
