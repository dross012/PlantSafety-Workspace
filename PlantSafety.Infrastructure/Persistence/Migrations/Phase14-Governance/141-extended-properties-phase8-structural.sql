-- =============================================
-- Migration:     141-extended-properties-phase8-structural.sql
-- Author:        Documentation Agent
-- Create date:   2026-07-19
-- Description:   Adds MS_Description extended properties documenting the
--                purpose of the Phase 8 structural objects (see
--                Phase8-Structural\80..83), none of which had any
--                documentation: the 3 common alias UDTs (StrongId/Utc/
--                CorrelationId) replicated per-database, the 2 bulk-insert
--                Table-Valued Parameter (TVP) UDTTs, the 3 human-readable-
--                number sequences, and the 2 cross-database synonyms
--                (AuditLog/ErrorLog) replicated per-database.
--
-- Idempotency:   sp_updateextendedproperty when the property already
--                exists, sp_addextendedproperty otherwise. Documentation-
--                only; zero rows of application data touched.
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- 1. Common alias UDTs (dbo.StrongId / dbo.Utc / dbo.CorrelationId),
--    one set per primary database (Phase 8, script 80).
------------------------------------------------------------
DECLARE @Databases TABLE (DbName sysname);
INSERT INTO @Databases VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB');

DECLARE @TypeDocs TABLE (TypeName sysname, Descr nvarchar(400));
INSERT INTO @TypeDocs VALUES
    (N'StrongId', N'Standardized alias for uniqueidentifier used as the underlying storage type for all StrongId-pattern primary/foreign keys across this database, per the platform StrongId convention (see Enterprise Arch Handbook Part II).'),
    (N'Utc', N'Standardized alias for datetime2(3) used for all UTC timestamp columns across this database, ensuring consistent precision/semantics for CreatedUtc/UpdatedUtc-style audit columns.'),
    (N'CorrelationId', N'Standardized alias for uniqueidentifier used to carry cross-service correlation lineage identifiers (CorrelationId/CausationId) consistently across this database.');

DECLARE @DbName sysname, @TypeName sysname, @Descr nvarchar(400), @sql nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT d.DbName, t.TypeName, t.Descr FROM @Databases d CROSS JOIN @TypeDocs t;
OPEN cur;
FETCH NEXT FROM cur INTO @DbName, @TypeName, @Descr;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'
    USE ' + QUOTENAME(@DbName) + N';
    IF EXISTS (SELECT 1 FROM sys.types ty JOIN sys.extended_properties ep
               ON ep.major_id = ty.user_type_id AND ep.class = 6
               WHERE ty.name = @TypeName AND ep.name = N''MS_Description'')
        EXEC sys.sp_updateextendedproperty @name=N''MS_Description'', @value=@Descr,
            @level0type=N''SCHEMA'', @level0name=N''dbo'', @level1type=N''TYPE'', @level1name=@TypeName;
    ELSE IF EXISTS (SELECT 1 FROM sys.types WHERE name = @TypeName AND schema_id = SCHEMA_ID(N''dbo''))
        EXEC sys.sp_addextendedproperty @name=N''MS_Description'', @value=@Descr,
            @level0type=N''SCHEMA'', @level0name=N''dbo'', @level1type=N''TYPE'', @level1name=@TypeName;';
    EXEC sp_executesql @sql, N'@TypeName sysname, @Descr nvarchar(400)', @TypeName, @Descr;

    FETCH NEXT FROM cur INTO @DbName, @TypeName, @Descr;
END
CLOSE cur;
DEALLOCATE cur;
GO

------------------------------------------------------------
-- 2. Bulk-insert Table-Valued Parameter (TVP) UDTTs (Phase 8, script 81).
------------------------------------------------------------
USE [PlantSafety.DiagnosticsDB];
GO
IF EXISTS (SELECT 1 FROM sys.table_types tt JOIN sys.extended_properties ep ON ep.major_id = tt.user_type_id AND ep.class = 6 AND ep.minor_id = 0 WHERE tt.name = N'DiagnosticEventTableType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Table-Valued Parameter shape used by bulk-insert stored procedures to accept a set of Diagnostics.DiagnosticEvent rows in a single call, avoiding row-by-row round trips for high-volume diagnostic event ingestion.', @level0type=N'SCHEMA', @level0name=N'Diagnostics', @level1type=N'TYPE', @level1name=N'DiagnosticEventTableType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Table-Valued Parameter shape used by bulk-insert stored procedures to accept a set of Diagnostics.DiagnosticEvent rows in a single call, avoiding row-by-row round trips for high-volume diagnostic event ingestion.', @level0type=N'SCHEMA', @level0name=N'Diagnostics', @level1type=N'TYPE', @level1name=N'DiagnosticEventTableType';
GO

USE [PlantSafety.IntegrationDB];
GO
IF EXISTS (SELECT 1 FROM sys.table_types tt JOIN sys.extended_properties ep ON ep.major_id = tt.user_type_id AND ep.class = 6 AND ep.minor_id = 0 WHERE tt.name = N'IntegrationLogEntryTableType' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Table-Valued Parameter shape used by bulk-insert stored procedures to accept a set of Integration.IntegrationLogEntry rows in a single call, for high-volume integration log ingestion without per-row round trips.', @level0type=N'SCHEMA', @level0name=N'Integration', @level1type=N'TYPE', @level1name=N'IntegrationLogEntryTableType';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Table-Valued Parameter shape used by bulk-insert stored procedures to accept a set of Integration.IntegrationLogEntry rows in a single call, for high-volume integration log ingestion without per-row round trips.', @level0type=N'SCHEMA', @level0name=N'Integration', @level1type=N'TYPE', @level1name=N'IntegrationLogEntryTableType';
GO

------------------------------------------------------------
-- 3. Human-readable-number sequences (Phase 8, script 82), CoreDB.
------------------------------------------------------------
USE [PlantSafety.CoreDB];
GO
IF EXISTS (SELECT 1 FROM sys.sequences sq JOIN sys.extended_properties ep ON ep.major_id = sq.object_id AND ep.minor_id = 0 WHERE sq.name = N'Seq_PermitNumber' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing sequence used to generate the human-readable permit number displayed to users (e.g. on Compliance.Permits records); not the surrogate StrongId primary key.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'SEQUENCE', @level1name=N'Seq_PermitNumber';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing sequence used to generate the human-readable permit number displayed to users (e.g. on Compliance.Permits records); not the surrogate StrongId primary key.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'SEQUENCE', @level1name=N'Seq_PermitNumber';
GO
IF EXISTS (SELECT 1 FROM sys.sequences sq JOIN sys.extended_properties ep ON ep.major_id = sq.object_id AND ep.minor_id = 0 WHERE sq.name = N'Seq_CertificationNumber' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing sequence used to generate the human-readable certification number displayed to users (e.g. on Compliance.Certification records); not the surrogate StrongId primary key.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'SEQUENCE', @level1name=N'Seq_CertificationNumber';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing sequence used to generate the human-readable certification number displayed to users (e.g. on Compliance.Certification records); not the surrogate StrongId primary key.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'SEQUENCE', @level1name=N'Seq_CertificationNumber';
GO
IF EXISTS (SELECT 1 FROM sys.sequences sq JOIN sys.extended_properties ep ON ep.major_id = sq.object_id AND ep.minor_id = 0 WHERE sq.name = N'Seq_ProcedureNumber' AND ep.name = N'MS_Description')
    EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing sequence used to generate the human-readable procedure/SOP document number displayed to users (e.g. on Compliance.Procedures records); not the surrogate StrongId primary key.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'SEQUENCE', @level1name=N'Seq_ProcedureNumber';
ELSE
    EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Monotonically increasing sequence used to generate the human-readable procedure/SOP document number displayed to users (e.g. on Compliance.Procedures records); not the surrogate StrongId primary key.', @level0type=N'SCHEMA', @level0name=N'Compliance', @level1type=N'SEQUENCE', @level1name=N'Seq_ProcedureNumber';
GO

------------------------------------------------------------
-- 4. Cross-database synonyms (dbo.AuditLog / dbo.ErrorLog), Phase 8 script 83,
--    one pair per primary database.
------------------------------------------------------------
DECLARE @Databases2 TABLE (DbName sysname);
INSERT INTO @Databases2 VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB');

DECLARE @SynDocs TABLE (SynName sysname, Descr nvarchar(400));
INSERT INTO @SynDocs VALUES
    (N'AuditLog', N'Local synonym for the shared PlantSafety.AuditDB.dbo.AuditLog table, allowing this database''s procedures/triggers to write centralized change-audit rows without a fully-qualified cross-database reference.'),
    (N'ErrorLog', N'Local synonym for the shared PlantSafety.ErrorDB.dbo.ErrorLog table, allowing this database''s procedures/triggers to write centralized error-log rows (via the standard TRY/CATCH error envelope) without a fully-qualified cross-database reference.');

DECLARE @DbName2 sysname, @SynName sysname, @Descr2 nvarchar(400), @sql2 nvarchar(max);
DECLARE cur2 CURSOR LOCAL FAST_FORWARD FOR
    SELECT d.DbName, s.SynName, s.Descr FROM @Databases2 d CROSS JOIN @SynDocs s;
OPEN cur2;
FETCH NEXT FROM cur2 INTO @DbName2, @SynName, @Descr2;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql2 = N'
    USE ' + QUOTENAME(@DbName2) + N';
    IF EXISTS (SELECT 1 FROM sys.synonyms sn JOIN sys.extended_properties ep
               ON ep.major_id = sn.object_id AND ep.class = 1 AND ep.minor_id = 0
               WHERE sn.name = @SynName AND ep.name = N''MS_Description'')
        EXEC sys.sp_updateextendedproperty @name=N''MS_Description'', @value=@Descr,
            @level0type=N''SCHEMA'', @level0name=N''dbo'', @level1type=N''SYNONYM'', @level1name=@SynName;
    ELSE IF EXISTS (SELECT 1 FROM sys.synonyms WHERE name = @SynName AND schema_id = SCHEMA_ID(N''dbo''))
        EXEC sys.sp_addextendedproperty @name=N''MS_Description'', @value=@Descr,
            @level0type=N''SCHEMA'', @level0name=N''dbo'', @level1type=N''SYNONYM'', @level1name=@SynName;';
    EXEC sp_executesql @sql2, N'@SynName sysname, @Descr nvarchar(400)', @SynName, @Descr2;

    FETCH NEXT FROM cur2 INTO @DbName2, @SynName, @Descr2;
END
CLOSE cur2;
DEALLOCATE cur2;
GO

------------------------------------------------------------
-- 5. Verification
------------------------------------------------------------
SELECT 'Types' AS ObjectKind, COUNT(*) AS DocumentedCount
FROM sys.types ty JOIN sys.extended_properties ep ON ep.major_id = ty.user_type_id AND ep.class = 6
WHERE ep.name = N'MS_Description' AND ty.name IN (N'StrongId', N'Utc', N'CorrelationId')
UNION ALL
SELECT 'Synonyms', COUNT(*)
FROM sys.synonyms sn JOIN sys.extended_properties ep ON ep.major_id = sn.object_id AND ep.class = 1 AND ep.minor_id = 0
WHERE ep.name = N'MS_Description' AND sn.name IN (N'AuditLog', N'ErrorLog');
GO
