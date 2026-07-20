-- =============================================
-- Migration:     142-extended-properties-phase7-13-governance.sql
-- Author:        Documentation Agent
-- Create date:   2026-07-19
-- Description:   Adds MS_Description extended properties to the Phase 7-13
--                governance tables that had no prior documentation:
--                  - System.SchemaMigrations (Phase 7, script 70) in all 8
--                    primary databases -- migration-application ledger.
--                  - msdb.dbo.PlantSafety_MaintenanceLog (Phase 12, script
--                    121) -- cross-database maintenance job run log.
--                  - PlantSafety.AuditDB.dbo.ServerDdlEventLog (Phase 13,
--                    script 132) -- server DDL trigger event log.
--                  - PlantSafety.AuditDB.dbo.ServerLogonLog (Phase 13,
--                    script 133) -- server logon trigger event log.
--                Phase 7 scripts 71-75 (TDE, data classification, naming
--                drift, retention/archival, RBAC) are metadata-only
--                operations against pre-existing objects (no new tables of
--                their own) and are already extensively narrated by inline
--                comment blocks in their migration files; not duplicated
--                here. Phase 9-11 introduced no new tables (RLS/masking/
--                Always Encrypted prep, Service Broker, change tracking,
--                Agent jobs, and performance objects all operate on
--                pre-existing tables or msdb/system catalogs only).
--
-- Idempotency:   sp_updateextendedproperty when the property already
--                exists, sp_addextendedproperty otherwise. Documentation-
--                only; zero rows of application data touched.
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- 1. System.SchemaMigrations, one per primary database (Phase 7, script 70).
------------------------------------------------------------
DECLARE @Databases TABLE (DbName sysname);
INSERT INTO @Databases VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB');

DECLARE @TableDescr nvarchar(400) = N'Ledger of applied schema migration scripts for this database (script name, phase, applied timestamp/by, checksum, success flag). Enforces one-row-per-ScriptName via a unique constraint so a migration cannot be silently re-applied undetected.';
DECLARE @ColDescr TABLE (ColName sysname, Descr nvarchar(400));
INSERT INTO @ColDescr VALUES
    (N'ScriptName', N'File name of the applied migration script; unique per database, used as the idempotency key for this ledger.'),
    (N'Phase', N'Governance phase grouping the script belongs to (e.g. Phase1, Phase7-Governance), matching the Migrations folder structure.'),
    (N'Checksum', N'Optional content checksum of the script at apply time, for future drift detection between the ledger and the script on disk.'),
    (N'Notes', N'Free-text notes about the migration run, e.g. flags retroactively-backfilled rows where AppliedAtUtc was approximated from file mtime.');

DECLARE @DbName sysname, @sql nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT DbName FROM @Databases;
OPEN cur;
FETCH NEXT FROM cur INTO @DbName;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'
    USE ' + QUOTENAME(@DbName) + N';
    IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
               JOIN sys.extended_properties ep ON ep.major_id = t.object_id AND ep.minor_id = 0
               WHERE s.name = N''System'' AND t.name = N''SchemaMigrations'' AND ep.name = N''MS_Description'')
        EXEC sys.sp_updateextendedproperty @name=N''MS_Description'', @value=@d,
            @level0type=N''SCHEMA'', @level0name=N''System'', @level1type=N''TABLE'', @level1name=N''SchemaMigrations'';
    ELSE IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N''System'' AND t.name = N''SchemaMigrations'')
        EXEC sys.sp_addextendedproperty @name=N''MS_Description'', @value=@d,
            @level0type=N''SCHEMA'', @level0name=N''System'', @level1type=N''TABLE'', @level1name=N''SchemaMigrations'';';
    EXEC sp_executesql @sql, N'@d nvarchar(400)', @TableDescr;

    DECLARE @ColName sysname, @ColDescrVal nvarchar(400), @colsql nvarchar(max);
    DECLARE curc CURSOR LOCAL FAST_FORWARD FOR SELECT ColName, Descr FROM @ColDescr;
    OPEN curc;
    FETCH NEXT FROM curc INTO @ColName, @ColDescrVal;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @colsql = N'
        USE ' + QUOTENAME(@DbName) + N';
        IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id
                   WHERE ep.major_id = OBJECT_ID(N''System.SchemaMigrations'') AND c.name = @cn AND ep.name = N''MS_Description'')
            EXEC sys.sp_updateextendedproperty @name=N''MS_Description'', @value=@cd,
                @level0type=N''SCHEMA'', @level0name=N''System'', @level1type=N''TABLE'', @level1name=N''SchemaMigrations'', @level2type=N''COLUMN'', @level2name=@cn;
        ELSE
            EXEC sys.sp_addextendedproperty @name=N''MS_Description'', @value=@cd,
                @level0type=N''SCHEMA'', @level0name=N''System'', @level1type=N''TABLE'', @level1name=N''SchemaMigrations'', @level2type=N''COLUMN'', @level2name=@cn;';
        EXEC sp_executesql @colsql, N'@cn sysname, @cd nvarchar(400)', @ColName, @ColDescrVal;

        FETCH NEXT FROM curc INTO @ColName, @ColDescrVal;
    END
    CLOSE curc;
    DEALLOCATE curc;

    FETCH NEXT FROM cur INTO @DbName;
END
CLOSE cur;
DEALLOCATE cur;
GO

------------------------------------------------------------
-- 2. msdb.dbo.PlantSafety_MaintenanceLog (Phase 12, script 121).
------------------------------------------------------------
USE [msdb];
GO
IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.extended_properties ep ON ep.major_id = t.object_id AND ep.minor_id = 0 WHERE t.name = N'PlantSafety_MaintenanceLog' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Run log for the cross-database PlantSafety maintenance procedures (usp_PlantSafety_IntegrityCheck / index maintenance / statistics update), one row per task execution per database. Exists independently of SQL Agent job history so runs remain auditable while the Agent service is stopped.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PlantSafety_MaintenanceLog';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Run log for the cross-database PlantSafety maintenance procedures (usp_PlantSafety_IntegrityCheck / index maintenance / statistics update), one row per task execution per database. Exists independently of SQL Agent job history so runs remain auditable while the Agent service is stopped.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PlantSafety_MaintenanceLog';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'dbo.PlantSafety_MaintenanceLog') AND c.name = N'TaskType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Which maintenance task produced this row: CHECKDB, INDEX_MAINT, or UPDATE_STATS.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PlantSafety_MaintenanceLog', @level2type=N'COLUMN', @level2name=N'TaskType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Which maintenance task produced this row: CHECKDB, INDEX_MAINT, or UPDATE_STATS.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PlantSafety_MaintenanceLog', @level2type=N'COLUMN', @level2name=N'TaskType';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'dbo.PlantSafety_MaintenanceLog') AND c.name = N'ObjectDetail' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Optional detail identifying the specific object acted on, e.g. the index/table name for an INDEX_MAINT row; null for whole-database tasks like CHECKDB.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PlantSafety_MaintenanceLog', @level2type=N'COLUMN', @level2name=N'ObjectDetail';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Optional detail identifying the specific object acted on, e.g. the index/table name for an INDEX_MAINT row; null for whole-database tasks like CHECKDB.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'PlantSafety_MaintenanceLog', @level2type=N'COLUMN', @level2name=N'ObjectDetail';
GO

------------------------------------------------------------
-- 3. PlantSafety.AuditDB.dbo.ServerDdlEventLog (Phase 13, script 132).
------------------------------------------------------------
USE [PlantSafety.AuditDB];
GO
IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.extended_properties ep ON ep.major_id = t.object_id AND ep.minor_id = 0 WHERE t.name = N'ServerDdlEventLog' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Structured, T-SQL-queryable log of DDL events captured by the server-scoped trigger trg_Server_DDL_SchemaProtection, restricted to PlantSafety.* databases. Logging-only -- complements (does not replace) the tamper-evident file-based SQL Server Audit from script 130; never blocks or denies the underlying DDL.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ServerDdlEventLog';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Structured, T-SQL-queryable log of DDL events captured by the server-scoped trigger trg_Server_DDL_SchemaProtection, restricted to PlantSafety.* databases. Logging-only -- complements (does not replace) the tamper-evident file-based SQL Server Audit from script 130; never blocks or denies the underlying DDL.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ServerDdlEventLog';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'dbo.ServerDdlEventLog') AND c.name = N'EventXml' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Full raw EVENTDATA() XML payload for the DDL event, retained for forensic detail beyond the individually-extracted columns.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ServerDdlEventLog', @level2type=N'COLUMN', @level2name=N'EventXml';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Full raw EVENTDATA() XML payload for the DDL event, retained for forensic detail beyond the individually-extracted columns.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ServerDdlEventLog', @level2type=N'COLUMN', @level2name=N'EventXml';
GO

------------------------------------------------------------
-- 4. PlantSafety.AuditDB.dbo.ServerLogonLog (Phase 13, script 133).
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.extended_properties ep ON ep.major_id = t.object_id AND ep.minor_id = 0 WHERE t.name = N'ServerLogonLog' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Log of server logon events captured by the server-scoped LOGON trigger trg_Server_Logon_Audit. Log-and-allow only by design -- the trigger body has no denial/rollback logic and swallows logging failures so a logging outage can never lock out a legitimate logon.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ServerLogonLog';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Log of server logon events captured by the server-scoped LOGON trigger trg_Server_Logon_Audit. Log-and-allow only by design -- the trigger body has no denial/rollback logic and swallows logging failures so a logging outage can never lock out a legitimate logon.', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ServerLogonLog';
GO
IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N'dbo.ServerLogonLog') AND c.name = N'ClientNetAddress' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Client-reported network address the logon originated from, as seen by the LOGON trigger''s EVENTDATA().', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ServerLogonLog', @level2type=N'COLUMN', @level2name=N'ClientNetAddress';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Client-reported network address the logon originated from, as seen by the LOGON trigger''s EVENTDATA().', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'ServerLogonLog', @level2type=N'COLUMN', @level2name=N'ClientNetAddress';
GO

------------------------------------------------------------
-- 5. Verification
------------------------------------------------------------
USE [PlantSafety.CoreDB];
GO
SELECT COUNT(*) AS SchemaMigrations_CoreDB_DocCount
FROM sys.extended_properties ep
WHERE ep.major_id = OBJECT_ID(N'System.SchemaMigrations') AND ep.name = N'MS_Description';
GO
USE [msdb];
GO
SELECT COUNT(*) AS MaintenanceLog_DocCount FROM sys.extended_properties ep WHERE ep.major_id = OBJECT_ID(N'dbo.PlantSafety_MaintenanceLog') AND ep.name = N'MS_Description';
GO
USE [PlantSafety.AuditDB];
GO
SELECT COUNT(*) AS DdlLog_DocCount FROM sys.extended_properties ep WHERE ep.major_id = OBJECT_ID(N'dbo.ServerDdlEventLog') AND ep.name = N'MS_Description';
SELECT COUNT(*) AS LogonLog_DocCount FROM sys.extended_properties ep WHERE ep.major_id = OBJECT_ID(N'dbo.ServerLogonLog') AND ep.name = N'MS_Description';
GO
