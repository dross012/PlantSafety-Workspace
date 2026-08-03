# Recommendations — SafetyPolicies

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. `SafetyPolicy` is
one of the solution's best-guarded aggregates (genuine `Approve`/`Reject` symmetry) yet is
simultaneously in the worst-case bucket for tenancy, both actor bugs, and RLS — reinforcing the
audit's general finding that transition-richness and security-correctness are unrelated axes in this
codebase. This module is almost entirely a cross-cutting consumer plus one module-specific
verification item.

1. **Verify `SafetyPolicy.Archive`'s silent-archive status.** Suspected (not directly re-verified
   this audit) to fold into `usp_SafetyPolicy_Update` without publishing an event — the 4th suspected
   instance of the pattern confirmed 3 times elsewhere (`Accident`, `Risk`, `Scorecard`). Fix: a
   direct read of `SafetyPolicy.Archive` to confirm/deny; if confirmed silent, apply the same fix as
   `accidentmanagement.md` item 3 (confirm with compliance whether observability is needed, add the
   missing `AddDomainEvent(SafetyPolicyArchivedEvent)` call, using `Dashboard.Archive`'s pattern as a
   template). Cite: `gaps/_severity-ranking.md` #9, `domain-mapping/_shared-findings.md` F3,
   `gaps/safetypolicies.md`. Effort: S (verification) + S (fix, if confirmed and needed). Risk:
   None-just-tech-debt if confirmed intentional and consistent with the other 3; data-integrity/
   compliance gap if a retention consumer is ever built expecting this signal. Sequencing: none —
   this is the one item this module can resolve independently of the other 24 files' equivalent
   items, since it closes out the full silent-archive population count (currently 4 confirmed of 8
   Archive-havers).

2. **Apply XC-1 (tenancy + RLS engagement) to `SafetyPolicyDapperRepository`** — pattern-inferred
   classic repository shape; this aggregate is explicitly named by `security/rls-audit.md` §2 as
   sharing the confirmed-blind classic shape alongside the directly-confirmed examples. Cite:
   `domain-mapping/_shared-findings.md` F1, `security/rls-audit.md` §2, `gaps/safetypolicies.md`.
   Effort: part of XC-1's solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land
   before XC-3 (auth-policy registration) proceeds for this module's endpoints.**

3. **Apply XC-2 (actor/correlation propagation) to `SafetyPolicy`'s outbox and AuditLog** — named
   explicitly in the 23-aggregate both-bugs cluster. Cite: `security/actor-propagation.md` §2a,
   `gaps/safetypolicies.md`. Effort: part of XC-2's pass. Risk: Critical (see XC-2). Sequencing:
   none — independent, run any time.

4. **Preserve `Reject`'s guard symmetry with `Approve` as a positive-control reference** — a genuine
   "undo" transition, as well-guarded as the forward path, worth citing when reviewing other
   aggregates' approve/reject-style workflows (e.g. `AuditEngagement.Close`, which lacks an
   equivalent level of cross-field rigor per `auditing.md` item 1). Cite: `gaps/safetypolicies.md`.
   Effort: none. Risk: none. Sequencing: none.

5. **Preserve `RecordAcknowledgment`'s status-precondition check** (`Published` status required) as
   another positive-control example, and use it as a template if `ChecklistCompletion`'s missing
   template-status check (`checklists.md` item 1) needs a reference pattern for "verify the
   referenced/required state before allowing a dependent action." Cite: `gaps/safetypolicies.md`,
   `gaps/checklists.md`. Effort: none (reference value). Risk: none. Sequencing: none.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
