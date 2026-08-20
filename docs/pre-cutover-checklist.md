# PlantSafety pre-cutover checklist — 2026-08-19 window

Companion to `plantsafety-cutover-plan.md`. Covers everything before the
first connection-string edit. All times CDT, window start 20:45.

## Status

**Not executed.** This checklist runs at the start of the maintenance
window, immediately before step 1 of the cutover procedure — not before.

## 20:45–20:50 — Confirm no application is running

- Re-check for a running PlantSafety Windows service.
- Re-check for anything listening on common web ports (`PlantSafety.Api`,
  `Reporting.Api` dev ports).
- As of the last check (2026-08-19, earlier the same day), nothing was
  running and nothing was listening — re-verify this is still true
  immediately before proceeding, since it's what makes this a data-safety
  exercise rather than a live-traffic cutover. If anything is now running,
  **stop and do not proceed** until it's confirmed stopped or the plan is
  revised to account for live traffic.

## 20:50–21:35 — Fresh backup/restore (`WITH REPLACE`)

The verified copy on `PLANTSAFETY` is a point-in-time snapshot. Anything
written to `SQLEXPRESS` since the last verification (manual testing, the
daily `usp_PlantSafety_BackupFull`/`Diff`/`Log` jobs still targeting the
old instance, etc.) is not on `PLANTSAFETY` yet.

1. `EXEC msdb.dbo.usp_PlantSafety_BackupFull @BackupRootPath = N'D:\Manufacturing Program\Backups\PlantSafety\Full\'`
2. `RESTORE VERIFYONLY FROM DISK = ... WITH CHECKSUM` for each of the 18 `.bak` files produced.
3. `RESTORE FILELISTONLY` to confirm logical file names haven't changed.
4. `RESTORE DATABASE [PlantSafety.<Name>] FROM DISK = ... WITH MOVE, REPLACE, CHECKSUM` for all 18 databases — `WITH REPLACE` is required since `PLANTSAFETY` already has these databases from the earlier restore.
5. `DBCC CHECKDB` on all 18, `NO_INFOMSGS, ALL_ERRORMSGS`.
6. Table/row-count comparison, source (`SQLEXPRESS`) vs. target (`PLANTSAFETY`), all 18 databases — must match exactly before proceeding.

Budget: 45 minutes for all 18 databases. If step 4–6 aren't complete by
21:35, that's a signal this window is too tight for a from-scratch full
backup cycle tonight — stop and reschedule rather than skipping
verification to catch up.

## Confirm the window (ongoing, 20:45 onward)

- Nobody is actively using `PlantSafetyPMForm.xlsm` or any tool that might
  write to `CoreDB`/`AuditDB`/`IntegrationDB` via one of the Group C
  standalone import tools (`FmcsaStandardsImport`, `NfpaReferenceImport`,
  `NioshStandardsImport`, `OshaStandardsImport`).
- If anyone else has access to this environment, confirm they're aware of
  the window before 20:45.

## Notify

Notify whoever owns this repo's day-to-day work that the window is
starting. This document doesn't know who that is — that's a step for
whoever runs the window.

## Log template

Record actual clock times as the window runs, for the post-cutover
verification and for calibrating future windows:

```
Checklist item 1 (no app running) confirmed at: ______
Fresh backup/restore started:                    ______
Fresh backup/restore DBCC + row-count confirmed:  ______
Any deviation from the planned timestamps:        ______
```
