# PlantSafety cutover rollback plan — 2026-08-19 window

Companion to `plantsafety-cutover-plan.md`. All times CDT.

## Status

**Not executed — nothing to roll back yet.** This document describes what
to do only if the cutover needs to be reversed, either during the
maintenance window (20:45–00:00) or during the rollback buffer
(00:00–02:00, 2026-08-20).

## Rollback is trivial before 23:30 (before step 7 / `READ_ONLY`)

Nothing on either instance has been touched destructively up to this
point — `SQLEXPRESS` is untouched, and `PLANTSAFETY` only has config-driven
application traffic (if any), not schema changes.

If any of steps 1–6 (21:35–23:30) fail or don't complete cleanly:

1. Revert the connection-string / instance-name edits made in steps 1–3
   (Groups A, B, C, D, E) back to `SQLEXPRESS`/`(local)\SQLEXPRESS`.
2. If Group E's `Register-*.ps1` scripts were already re-run (step 3,
   22:00–22:20), re-run them again with the reverted `.ps1` files so the
   live Scheduled Tasks point back at `SQLEXPRESS`.
3. Confirm `PlantSafety.Api`/`Reporting.Api` (if built/tested in step 4)
   are stopped or repointed back before anyone uses them again.
4. No data reconciliation needed — `SQLEXPRESS` was never modified.
5. Log the rollback and the reason in the same log template used by
   `pre-cutover-checklist.md`, and treat the window as failed —
   reschedule rather than immediately retrying the same night unless the
   root cause is fully understood.

## Rollback after 23:30 (after step 7 / `SQLEXPRESS` set `READ_ONLY`)

This is the one point in the whole plan with genuine risk — everything
before it is safely reversible by just reverting config; after it,
`SQLEXPRESS` stops accepting writes, so anything written to `PLANTSAFETY`
between cutover and the rollback decision needs to be reconciled or
accepted as lost.

1. `ALTER DATABASE [PlantSafety.<Name>] SET READ_WRITE` for all 18
   databases on `SQLEXPRESS` (reverses the `READ_ONLY WITH ROLLBACK
   IMMEDIATE` from step 7 — this does not restore data, only write access).
2. Revert the connection-string / instance-name edits (steps 1–3) back to
   `SQLEXPRESS`.
3. Re-run Group E's `Register-*.ps1` scripts with the reverted files so
   Scheduled Tasks point back at `SQLEXPRESS`.
4. **Reconcile data written to `PLANTSAFETY` during the cutover window**
   (20:45–23:30, plus however long `PLANTSAFETY` was live after step 7).
   If nothing was written (most likely, since no application traffic is
   expected during a maintenance window with no app running per the
   pre-cutover checklist), this step is a formality — confirm via
   row-count comparison, don't assume. If something *was* written, it
   needs to be manually replayed against `SQLEXPRESS` or explicitly
   accepted as lost; there is no automated reconciliation tool for this.
5. Log the rollback, the reason, and the reconciliation outcome.

## Rollback buffer window: 00:00–02:00 CDT, 2026-08-20

This 2-hour window exists specifically to execute either rollback path
above if a problem surfaces after the maintenance window's steps appear
complete but before the change is considered final. If the buffer expires
without issues surfacing, the cutover is considered final and normal
post-cutover soak (per `docs/operations/instance-decommission-checklist.md`
in the Training repo) begins.

## Decision rule

If in doubt during the window, stop and roll back rather than push
forward — the compressed same-day schedule (3h15m vs. the original
6-hour Saturday window) leaves less room to recover from a mid-window
surprise than the original plan assumed. A rolled-back, rescheduled
cutover costs a delay; a rushed one risks the one thing this whole
consolidation effort has been careful to avoid — real data loss on
PlantSafety's production databases.
