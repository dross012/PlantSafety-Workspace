# PlantSafety post-cutover smoke tests — 2026-08-19 window

Companion to `plantsafety-cutover-plan.md`. Covers steps 4–6 of the
maintenance-window procedure (22:20–23:30 CDT) plus final post-cutover
verification. All times CDT.

## Status

**Not executed.** These tests run only after Groups A–E's connection
strings have been repointed (steps 1–3, 21:35–22:20) and only gate step 7
(`SQLEXPRESS` → `READ_ONLY`, 23:30). If any test here fails, do not
proceed to step 7 — follow `rollback-plan.md` instead.

## 22:20–22:50 — API smoke tests against `localhost\PLANTSAFETY`

1. Build `PlantSafety.Api` and `Reporting.Api` with the repointed
   `appsettings.json`/`Appsettings.json` (Group A) and Operations module
   config (Group B).
2. Start both APIs locally.
3. Confirm each API's health check endpoint returns healthy.
4. Confirm a read-only operation succeeds against each:
   - `PlantSafety.Api`: a `GET` against a `CoreDB`-backed read endpoint.
   - `Reporting.Api`: a `GET` against an `AuditDB`/`IntegrationDB`-backed
     read endpoint.
5. Confirm no errors logged referencing `SQLEXPRESS` (would indicate a
   missed Group A/B/C/D file).
6. Stop both APIs once confirmed — this is a smoke test, not standing up
   the service for production use tonight.

If either API fails to start, fails its health check, or a read errors
out: stop, do not proceed to step 5 (manual backup script test) or step 7
(`READ_ONLY`). Diagnose against the connection strings just changed —
most likely cause is a missed file from Group A/B/C/D.

## 22:50–23:10 — Automation end-to-end test

Run one of the Group E scripts manually against the new instance:

```
Invoke-PlantSafetyBackup.ps1 -BackupType Full
```

against `localhost\PLANTSAFETY`. This confirms the repointed automation
actually works end to end (correct instance, correct paths, correct
permissions for the `NT SERVICE\MSSQL$PLANTSAFETY` account), not just
that the `.ps1` file compiles/parses.

Confirm:
- The script completes without error.
- A backup file is produced in the expected location.
- No error referencing `SQLEXPRESS` in the script's own log output.

If this fails: stop, do not proceed to step 7. This is exactly the
scenario Group E's design note warns about — repointing the `.ps1` file
alone isn't enough if the already-registered Scheduled Task wasn't also
re-registered (step 3, 22:00–22:20) with the updated arguments.

## 23:10–23:30 — Post-cutover verification (pre-`READ_ONLY`)

1. **Row-count comparison, all 18 databases**, source (`SQLEXPRESS`) vs.
   target (`PLANTSAFETY`) — reuse the same comparison used during the
   original backup/restore verification and the pre-cutover fresh
   backup/restore (20:50–21:35). Must still match exactly; if anything
   diverges here, something wrote to one side but not the other during
   the window — stop and investigate before step 7.
2. **Confirm the Group E Scheduled Tasks logged success** for the manual
   run above — check `C:\Users\dross\SQLBackups\PlantSafety\TaskLogs\` or
   wherever the repointed scripts write logs.
3. Only if 1 and 2 both pass, proceed to step 7 (23:30–23:55) in
   `plantsafety-cutover-plan.md`.

## After step 7 (post-23:55, and into the following days)

- Repeat the row-count comparison once more after `SQLEXPRESS` is set
  `READ_ONLY`, to confirm nothing changed as a side effect of that
  operation itself (it shouldn't, but verify — don't assume).
- Confirm the Scheduled Tasks under Group E run successfully at least
  once more on their normal schedule (not just the manual test above)
  against the new instance.
- Leave `SQLEXPRESS`'s `PlantSafety.*` databases `READ_ONLY` for a soak
  period before treating `localhost\SQLEXPRESS` as a decommission
  candidate — see the Training repo's
  `docs/operations/instance-decommission-checklist.md`.

## Log template

```
API smoke test (PlantSafety.Api) result:     ______
API smoke test (Reporting.Api) result:        ______
Group E manual backup script result:          ______
Row-count comparison (pre-READ_ONLY) result:  ______
Decision: proceed to step 7 / stop and roll back:  ______
```
