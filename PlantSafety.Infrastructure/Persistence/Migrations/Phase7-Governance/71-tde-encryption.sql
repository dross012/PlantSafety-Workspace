-- =============================================
-- Migration:     71-tde-encryption.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Description:   Transparent Data Encryption (TDE) enablement for all in-scope
--                PlantSafety databases (Governance remediation item #1).
--
-- IMPORTANT — ENVIRONMENT CONSTRAINT (live-verified 2026-07-19):
--   This instance is running SQL Server 2022 (17.0.4060.2) EXPRESS EDITION.
--   TDE (Transparent Data Encryption) is NOT supported on Express Edition.
--   Attempting `CREATE DATABASE ENCRYPTION KEY` on this edition fails with:
--     Msg 33117: "Transparent Data Encryption is not available in the
--     edition of this SQL Server instance."
--   This is a hard platform limitation, not a scripting gap — it cannot be
--   worked around from T-SQL. The instance must be upgraded to Standard,
--   Enterprise, or Developer edition before TDE can actually be turned ON.
--
--   This script therefore PREPARES everything required (server-level DMK,
--   server certificate, certificate backup) so that TDE can be switched on
--   with a single `ALTER DATABASE ... SET ENCRYPTION ON` per database the
--   moment the instance is upgraded. The final per-database encryption step
--   is wrapped in TRY/CATCH so it fails soft (logged, not fatal) on Express
--   and will succeed automatically on a supported edition without any script
--   changes.
--
-- Scope: PlantSafety.CoreDB, ArchiveDB, CloudIdentityDB, PredictiveDB,
--        ReportingDB, IntegrationDB, DiagnosticsDB, ExportImportDB,
--        AuditDB, ErrorDB (AuditDB/ErrorDB included — they hold audit-trail
--        data and are judged in-scope per the agent's own discretion clause).
--
-- Idempotency: every step is existence-checked; safe to re-run.
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- 1. Server-level Database Master Key (master DB)
--    NOTE (dev-only): password is a literal here for unattended execution
--    on a local dev instance. PRODUCTION WARNING: never embed DMK/cert
--    passwords in a repo-committed script — use Key Vault-issued secrets
--    injected at deploy time instead.
------------------------------------------------------------
USE [master];
GO
IF NOT EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##')
BEGIN
    CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'Test_DevOnly_TDE_2026!ChangeMe';
    PRINT 'Server Database Master Key created.';
END
ELSE
    PRINT 'Server Database Master Key already exists — skipped.';
GO

------------------------------------------------------------
-- 2. Server certificate for TDE
------------------------------------------------------------
USE [master];
GO
IF NOT EXISTS (SELECT 1 FROM sys.certificates WHERE name = 'PlantSafety_TDE_Cert')
BEGIN
    CREATE CERTIFICATE PlantSafety_TDE_Cert
        WITH SUBJECT = 'PlantSafety TDE Certificate';
    PRINT 'PlantSafety_TDE_Cert created.';
END
ELSE
    PRINT 'PlantSafety_TDE_Cert already exists — skipped.';
GO

------------------------------------------------------------
-- 3. Back up the certificate + private key immediately.
--    Losing an unbacked-up TDE cert makes an encrypted database
--    permanently unrecoverable if the cert is ever lost/corrupted.
--
--    Backup location (THIS RUN, local dev instance only):
--      C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\
--      Persistence\Migrations\Phase7-Governance\tde-cert-backup\
--        PlantSafety_TDE_Cert.cer
--        PlantSafety_TDE_Cert.pvk
--
--    *** PRODUCTION WARNING ***
--    Storing a TDE certificate/private-key backup inside a source-control
--    repository (even in an ignored folder) is NOT acceptable for a real
--    deployment. In production this backup MUST go to a secure, access-
--    controlled secret store (e.g. Azure Key Vault, an HSM, or an offline
--    secure vault procedure) — never a repo path. This local-repo path is
--    only acceptable because this is a disposable local SQLEXPRESS dev
--    instance.
--
--    NOTE: BACKUP CERTIFICATE will error ("Cannot write into file...") if
--    the target file already exists, OR if the SQL Server service account
--    (NT SERVICE\MSSQL$SQLEXPRESS on this box) lacks write permission on
--    the target folder. Both were hit and resolved live during this run
--    (icacls grant to the service account); the guard below simply skips
--    re-backup if the .cer already exists on a re-run.
------------------------------------------------------------
USE [master];
GO
-- File-existence cannot be checked from T-SQL without xp_cmdshell, which is
-- disabled by default and out of scope to enable for this agent; instead we
-- guard with TRY/CATCH so a re-run that finds the file already there just
-- logs and continues rather than failing the whole script.
BEGIN TRY
    BACKUP CERTIFICATE PlantSafety_TDE_Cert
        TO FILE = 'C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\Persistence\Migrations\Phase7-Governance\tde-cert-backup\PlantSafety_TDE_Cert.cer'
        WITH PRIVATE KEY (
            FILE = 'C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\Persistence\Migrations\Phase7-Governance\tde-cert-backup\PlantSafety_TDE_Cert.pvk',
            ENCRYPTION BY PASSWORD = 'PvkBackup_DevOnly_2026!ChangeMe'
        );
    PRINT 'PlantSafety_TDE_Cert backed up (.cer/.pvk).';
END TRY
BEGIN CATCH
    PRINT 'Certificate backup skipped or already present: ' + ERROR_MESSAGE();
END CATCH
GO

------------------------------------------------------------
-- 4. Per-database Database Encryption Key + ALTER DATABASE ENCRYPTION ON
--    Wrapped per-database in TRY/CATCH: on SQL Server Express this will
--    fail with Msg 33117 (edition not supported) — logged as a warning,
--    non-fatal. On Standard/Enterprise/Developer this will succeed and
--    actually turn encryption on.
------------------------------------------------------------
DECLARE @dbs TABLE (name sysname);
INSERT INTO @dbs (name) VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB'),
    (N'PlantSafety.AuditDB'), (N'PlantSafety.ErrorDB');

DECLARE @db sysname, @sql nvarchar(max);
DECLARE dbcur CURSOR LOCAL FAST_FORWARD FOR SELECT name FROM @dbs;
OPEN dbcur;
FETCH NEXT FROM dbcur INTO @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'
    USE ' + QUOTENAME(@db) + N';
    IF NOT EXISTS (SELECT 1 FROM sys.dm_database_encryption_keys WHERE database_id = DB_ID())
    BEGIN TRY
        CREATE DATABASE ENCRYPTION KEY
            WITH ALGORITHM = AES_256
            ENCRYPTION BY SERVER CERTIFICATE PlantSafety_TDE_Cert;
        ALTER DATABASE ' + QUOTENAME(@db) + N' SET ENCRYPTION ON;
        PRINT ''TDE enabled on ' + @db + N'.'';
    END TRY
    BEGIN CATCH
        PRINT ''TDE NOT enabled on ' + @db + N' — '' + ERROR_MESSAGE();
    END CATCH
    ELSE
        PRINT ''DEK already exists on ' + @db + N' — skipped.'';
    ';
    EXEC sp_executesql @sql;
    FETCH NEXT FROM dbcur INTO @db;
END
CLOSE dbcur;
DEALLOCATE dbcur;
GO

------------------------------------------------------------
-- 5. Verification query — re-run any time to check live encryption state.
------------------------------------------------------------
SELECT
    DB_NAME(k.database_id) AS DatabaseName,
    k.encryption_state,
    CASE k.encryption_state
        WHEN 0 THEN 'NO_DATABASE_ENCRYPTION_KEY_PRESENT'
        WHEN 1 THEN 'UNENCRYPTED'
        WHEN 2 THEN 'ENCRYPTION_IN_PROGRESS'
        WHEN 3 THEN 'ENCRYPTED'
        WHEN 4 THEN 'KEY_CHANGE_IN_PROGRESS'
        WHEN 5 THEN 'DECRYPTION_IN_PROGRESS'
        WHEN 6 THEN 'PROTECTION_CHANGE_IN_PROGRESS'
    END AS EncryptionStateDesc,
    k.percent_complete,
    k.encryptor_type
FROM sys.dm_database_encryption_keys k
ORDER BY DatabaseName;
GO
