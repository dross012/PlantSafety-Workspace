# Recommendations — Checklists

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. This module holds
the clearest **confirmed** (not merely suspected) instance of the cross-module-reference gap pattern
the audit worried recurs across the solution's 48 rich aggregates — that is the priority item here,
ahead of this module's share of the generic cross-cutting work.

1. **Fix `ChecklistCompletion`'s unvalidated `ChecklistTemplateId` reference.** None of the 5
   lifecycle methods (`Create`/`Start`/`RecordItemResult`/`FlagFailedItem`/`Complete`) check the
   referenced `ChecklistTemplate`'s status — `ChecklistTemplateId` is a bare, unvalidated FK, and zero
   handlers have a template-repository dependency at all. A completion can run today against a
   Retired or never-Published template with nothing able to detect it. This is direct, confirmed
   proof (not extrapolation) that the cross-module-reference gap pattern is real. Fix: add a
   `IChecklistTemplateRepository` dependency to `ChecklistCompletion`'s command handlers; on
   `Create`/`Start`, load the referenced template and verify its status is `Published` before
   allowing the completion to proceed. Cite: `gaps/_severity-ranking.md` #11,
   `command-handler-map/_shared-findings.md` F12, `rule-enforcement/_shared-findings.md` G5,
   `gaps/checklists.md`. Effort: M (adds a second repository dependency to handlers that currently
   follow the single-repository shape used everywhere else). Risk: **Data integrity** — a completion
   can be recorded against an invalid/retired template today with no detection. Sequencing: none.

2. **Investigate `FlagFailedItem`'s confirmed double-publish** — it calls `RecordItemResult`
   internally, raising 2 events from 1 command. Not silent (both events do fire correctly) but a
   duplication anomaly worth resolving before any event-consumer wiring work lands, since a future
   consumer built assuming 1-event-per-command would double-process this transition. Fix: either
   refactor `FlagFailedItem` to raise a single, purpose-built event, or explicitly document the
   two-event behavior as intentional (e.g. "flagging a failed item is itself an item-result
   recording") so a future consumer author designs for it deliberately. Cite:
   `event-map/_shared-findings.md` F17, `gaps/checklists.md`. Effort: S. Risk: None-just-tech-debt
   today (no working consumer exists to be confused by it — see `_cross-cutting.md`'s cross-reference
   to the solution-wide zero-consumer finding), but should be resolved before Checklists is
   prioritized for consumer-wiring work. Sequencing: fix before any event-consumer work targets this
   aggregate.

3. **Apply XC-1 (tenancy + RLS engagement) to `ChecklistTemplate`/`ChecklistCompletion`.**
   Pattern-inferred classic-repository shape on both. Cite: `domain-mapping/_shared-findings.md` F1,
   `gaps/checklists.md`. Effort: part of XC-1's solution-wide pass. Risk: Critical (see XC-1).
   **Sequencing: must land before XC-3 (auth-policy registration) proceeds for this module's
   endpoints.**

4. **Apply XC-2 (actor/correlation propagation) to both aggregates' outbox calls** — both hit by
   Bug#1 only. Cite: `security/actor-propagation.md` §2b, `gaps/checklists.md`. Effort: part of
   XC-2's pass. Risk: Critical (see XC-2). Sequencing: none — independent, run any time.

5. **Formally retire the `Operational.Checklists.Checklists` marker duplicate** — Create-only,
   read-only SQL (`usp_Checklists_List` only), legacy/dead-ish. Fix: confirm dead status, then remove
   per the cross-module duplicate-concept cleanup approach referenced in `analytics.md`. Cite:
   `gaps/checklists.md`. Effort: M. Risk: Tech-debt/landmine, not a live defect. Sequencing:
   coordinate with the cross-module duplicate-concept cleanup effort.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
