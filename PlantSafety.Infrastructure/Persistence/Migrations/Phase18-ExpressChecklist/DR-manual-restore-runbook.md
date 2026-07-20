# PlantSafety DR — Manual Restore Runbook (SQL Server Express, `(local)\SQLEXPRESS`)

**Instance details (live-verified 2026-07-19):** `Edition` = Express Edition
(64-bit), `ProductVersion` = 17.0.4065.4, default data/log path
`C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\`.
(Note: prior Phase 12 documentation labeled this build "SQL Server 2025" from
the same version string, while this task's checklist calls it "SQL Server
2022 Express Edition" — a naming discrepancy already flagged in
`Phase12-Reliability\ha-dr-future-work.md`; the live SERVERPROPERTY values
above are authoritative for restore purposes regardless of marketing name.)

**Status:** New document (genuine gap — no human-readable manual-restore runbook
existed anywhere in Phases 1-17 as of 2026-07-19; the Phase 12 restore-test
tooling exists but was proc-only, not a DBA-facing runbook).

**Audience:** A human DBA (or on-call engineer) recovering a PlantSafety
database after data loss/corruption on this Express instance. This is written
for a real incident, not a demo — follow it in order, do not skip
verification steps.

---

## 0. Before you touch anything

- Confirm which database(s) are actually affected. Do not restore a database
  that isn't broken.
- If the live database still exists and is merely suspect (e.g. corruption
  suspected but the instance is up), first try `DBCC CHECKDB('<DbName>')`
  against the LIVE database to see the actual damage before deciding to
  restore. Restoring is destructive to any data written since the last usable
  backup.
- Identify the SQL Server error log / Windows Event Log entry that triggered
  the incident and save it somewhere outside the SQL Server error log
  directory (for the post-incident review) before proceeding.
- **SQL Server Agent is disabled on this instance.** Automated backups run via
  Windows Task Scheduler tasks (see `Register-PlantSafetyBackupTasks.ps1` in
  this folder), not SQL Agent jobs. If backups appear to be missing/stale,
  check `Get-ScheduledTask -TaskName 'PlantSafety - *Backup*'` and
  `Get-ScheduledTaskInfo` for `LastRunTime`/`LastTaskResult` first, and the
  per-run logs under `C:\Users\dross\SQLBackups\PlantSafety\TaskLogs\`.

---

## 1. Locate the backup files

All PlantSafety.* backups live under:

```
C:\Users\dross\SQLBackups\PlantSafety\
  Full\   <DatabaseName>_FULL_<yyyyMMdd_HHmmss>.bak
  Diff\   <DatabaseName>_DIFF_<yyyyMMdd_HHmmss>.bak
  Log\    <DatabaseName>_LOG_<yyyyMMdd_HHmmss>.trn   (currently always empty —
                                                        see note below)
```

- `<DatabaseName>` is the full database name including the `PlantSafety.`
  prefix, e.g. `PlantSafety.CoreDB`.
- The timestamp is the backup START time in UTC, `yyyyMMdd_HHmmss`, e.g.
  `PlantSafety.CoreDB_FULL_20260719_192805.bak`.
- **Log backups**: as of 2026-07-19 every PlantSafety.* database is in
  **SIMPLE recovery model**, so `Log\` will be empty and log backups are not
  part of the restore chain — do not go looking for `.trn` files unless a
  database has been deliberately switched to FULL recovery (a documented,
  human governance decision — see
  `Phase12-Reliability\ha-dr-future-work.md`). If that has happened for the
  database you're restoring, you MUST also restore the chain of `.trn` files
  in timestamp order after the full/diff restore below, each `WITH NORECOVERY`
  except the last, which uses `WITH RECOVERY`.

To find the most recent FULL and DIFF backup for a specific database from a
DBA workstation/RDP session on the SQL Server host:

```powershell
$db = 'PlantSafety.CoreDB'   # <-- set the affected database name
Get-ChildItem "C:\Users\dross\SQLBackups\PlantSafety\Full" -Filter "$db*_FULL_*.bak" |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
Get-ChildItem "C:\Users\dross\SQLBackups\PlantSafety\Diff" -Filter "$db*_DIFF_*.bak" |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
```

Note that the DIFF (if used) must be based on the FULL you're about to
restore — a DIFF backup only applies cleanly on top of the FULL backup that
was live when the DIFF ran. If in doubt, check `msdb.dbo.backupset` (see
step 2) rather than trusting file timestamps alone.

Alternatively, query backup history directly (more authoritative than file
timestamps, since it also tells you the LSN chain):

```sql
SELECT bs.database_name, bs.type, bs.backup_start_date, bs.backup_finish_date,
       bmf.physical_device_name
FROM msdb.dbo.backupset bs
JOIN msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
WHERE bs.database_name = 'PlantSafety.CoreDB'   -- <-- set the affected database name
ORDER BY bs.backup_finish_date DESC;
```

`type = 'D'` is a FULL backup, `type = 'I'` is a DIFFERENTIAL, `type = 'L'` is
a LOG backup.

---

## 2. Decide FULL-only vs FULL+DIFF restore

- If the most recent DIFF backup is newer than the most recent FULL backup
  and both cover the same LSN chain (confirmed via the `msdb.dbo.backupset`
  query above), restore FULL `WITH NORECOVERY` then DIFF `WITH RECOVERY`.
- If there is no usable DIFF (or the DIFF's base FULL isn't the one you're
  restoring), restore the FULL alone `WITH RECOVERY`.
- Uncompressed backups: `BACKUP ... WITH COMPRESSION` is blocked on Express
  (Msg 1844), so every PlantSafety.* backup is an uncompressed `.bak`/`.trn`
  file — no special `RESTORE` handling is needed for compression, but expect
  larger files and correspondingly longer restore I/O time than a compressed
  backup would take.

---

## 3. Restore to an ISOLATED name first — never restore directly over a live database in an active incident until verified

This mirrors exactly what `msdb.dbo.usp_PlantSafety_RestoreTest` already does
automatically (see `Phase12-Reliability\123-restore-test.sql`) — reuse that
procedure for the verification step whenever possible instead of hand-typing
`RESTORE` statements, since it already handles file-move clauses correctly
per backup and cleans up after itself:

```sql
-- Restores the LATEST FULL backup for the named database into
-- <DatabaseName>_DRTest, runs DBCC CHECKDB against it, then drops the test
-- database and its files automatically (success or failure).
EXEC msdb.dbo.usp_PlantSafety_RestoreTest @DatabaseName = N'PlantSafety.CoreDB';
```

Check the result:

```sql
SELECT TOP 5 * FROM msdb.dbo.PlantSafety_MaintenanceLog
WHERE TaskType = 'RESTORE_TEST' AND DatabaseName = 'PlantSafety.CoreDB'
ORDER BY MaintenanceLogId DESC;
```

`Result = 'OK'` with a `Details` message ending "DBCC CHECKDB passed" confirms
the backup file itself is intact and restorable. **Do this before touching the
live/broken database in a real incident** — it tells you the backup is good
before you commit to an outage window for the real restore.

If you need a DIFF applied too (not just the latest FULL — e.g. you need a
point slightly newer than the last FULL), `usp_PlantSafety_RestoreTest` only
restores the FULL; for a FULL+DIFF verification, do it manually per step 4
below but target a `_DRTest`-style isolated name, then drop it when done.

---

## 4. Manual RESTORE syntax (when not using the proc, e.g. FULL+DIFF, or restoring to the real name)

### 4a. Get the logical file names first (they rarely match the physical file names)

```sql
RESTORE FILELISTONLY
FROM DISK = N'C:\Users\dross\SQLBackups\PlantSafety\Full\PlantSafety.CoreDB_FULL_20260719_192805.bak';
```

Note every `LogicalName` and `Type` (`D` = data, `L` = log) returned — you
need one `MOVE` clause per row.

### 4b. Restore the FULL backup

If restoring to the LIVE database name (real incident, backup verified good,
outage window approved):

```sql
-- Kick out any existing connections first
ALTER DATABASE [PlantSafety.CoreDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE [PlantSafety.CoreDB]
FROM DISK = N'C:\Users\dross\SQLBackups\PlantSafety\Full\PlantSafety.CoreDB_FULL_20260719_192805.bak'
WITH REPLACE, CHECKSUM, STATS = 10,
     NORECOVERY,  -- omit NORECOVERY (use RECOVERY instead) if there is no DIFF to apply
     MOVE 'PlantSafety.CoreDB'     TO 'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\PlantSafety.CoreDB.mdf',
     MOVE 'PlantSafety.CoreDB_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\PlantSafety.CoreDB_log.ldf';
     -- add one MOVE per additional file/filegroup listed by RESTORE FILELISTONLY
     -- (several PlantSafety databases have extra filegroups, e.g. Core.HotData,
     -- Core.ColdData, Core.Index, Core.ReadOnly for PlantSafety.CoreDB — confirm
     -- the actual DATA folder path and full file list live via
     -- sys.master_files / RESTORE FILELISTONLY before running this for real,
     -- do not assume the file list above is complete for every database)
```

### 4c. Restore the DIFF on top (only if applying one)

```sql
RESTORE DATABASE [PlantSafety.CoreDB]
FROM DISK = N'C:\Users\dross\SQLBackups\PlantSafety\Diff\PlantSafety.CoreDB_DIFF_20260719_192813.bak'
WITH RECOVERY, CHECKSUM, STATS = 10;
```

### 4d. Bring the database back online

```sql
ALTER DATABASE [PlantSafety.CoreDB] SET MULTI_USER;
```

---

## 5. Post-restore verification (mandatory — do not skip)

1. **Integrity**: `DBCC CHECKDB('PlantSafety.CoreDB') WITH NO_INFOMSGS, ALL_ERRORMSGS;`
   — must return no errors.
2. **Row-count sanity check**: compare row counts on a handful of core tables
   against your last known-good monitoring snapshot or application-level
   expectations. There is no automated row-count-diff tool for this — this is
   a manual judgement step, consistent with the No-Guessing Rule (do not
   assume the restore is "complete" data just because CHECKDB passed —
   CHECKDB validates structural integrity, not business completeness).
3. **Query Store**: confirm `sys.database_query_store_options` still shows
   `actual_state = 1` (ON) post-restore — Query Store settings are part of
   the database and should carry over from the backup, but confirm rather
   than assume.
4. **Application connectivity**: have the application team (or run a smoke
   test against `PlantSafety.Api`) confirm the app can connect and perform a
   basic read/write against the restored database before declaring the
   incident resolved.
5. **RPO actually achieved**: compare the backup's `backup_finish_date`
   (from `msdb.dbo.backupset`, or the timestamp embedded in the restored
   backup's filename) against the incident time to state the actual data-loss
   window in the incident report. Do not guess this number — read it off the
   restored backup's metadata.
6. **Log the incident** in whatever incident-tracking system this
   organization normally uses, including: which backup file was used, the
   measured RPO (step 5), total restore duration, and a link back to this
   runbook.

---

## 6. If the backup itself is bad or missing

If `usp_PlantSafety_RestoreTest` (step 3) or `DBCC CHECKDB` (step 5) fails:

1. Try the next-older FULL backup in `C:\Users\dross\SQLBackups\PlantSafety\Full\`
   for the same database and repeat verification. Every restore attempt
   accepts data loss back to that backup's timestamp — document this
   explicitly in the incident report.
2. If ALL FULL backups for the database are bad or missing, this is a
   Severity-1 DR failure — there is no offsite/secondary copy on this
   single-instance dev topology (see
   `Phase12-Reliability\ha-dr-future-work.md` for what a real HA/DR topology
   — Always On AGs, log shipping — would additionally provide, none of which
   is built here). Escalate immediately; do not attempt to reconstruct data
   from application-level exports as a substitute for a real DBA-led recovery
   without explicit sign-off, since that risks silently losing referential
   integrity across the 18 PlantSafety.* databases.

---

## Reference: where the backup/maintenance automation itself lives

- Backup stored procedures: `msdb.dbo.usp_PlantSafety_BackupFull` /
  `BackupDiff` / `BackupLog` — `Phase12-Reliability\120-backup-jobs-full-diff-log.sql`
- Backup retention cleanup: `msdb.dbo.usp_PlantSafety_BackupCleanup` (90-day
  placeholder retention, pending governance confirmation) —
  `Phase12-Reliability\122-backup-cleanup-retention.sql`
- Restore-test procedure: `msdb.dbo.usp_PlantSafety_RestoreTest` —
  `Phase12-Reliability\123-restore-test.sql`
- Windows Task Scheduler registration (SQL Agent is disabled on this
  instance): `Invoke-PlantSafetyBackup.ps1` +
  `Register-PlantSafetyBackupTasks.ps1` — this folder (`Phase18-ExpressChecklist`)
- Maintenance/restore-test run history: `msdb.dbo.PlantSafety_MaintenanceLog`
- Database size trend (for spotting disk-pressure-driven incidents before
  they happen): `msdb.dbo.PlantSafety_DatabaseSizeLog` —
  `Phase18-ExpressChecklist\180-database-size-log.sql`
