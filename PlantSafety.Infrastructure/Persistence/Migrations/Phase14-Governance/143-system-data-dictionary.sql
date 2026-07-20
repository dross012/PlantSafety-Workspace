-- =============================================
-- Migration:     143-system-data-dictionary.sql
-- Author:        Documentation Agent
-- Create date:   2026-07-19
-- Description:   Creates and populates System.DataDictionary in each of the
--                8 primary databases: a lightweight, business-value metadata
--                catalog that is NOT a duplicate of sys.columns/
--                sys.extended_properties (SQL Server already provides that).
--                Captures, per table:
--                  Owner            - owning module/team. For
--                                     PlantSafety.CoreDB this is the C#
--                                     PlantSafety.Domain namespace the
--                                     table's schema maps 1:1 onto (verified
--                                     live: every CoreDB schema except
--                                     'System' has an identically-named
--                                     PlantSafety.Domain folder). For the
--                                     other 7 databases, no PlantSafety.Domain
--                                     namespace exists with a matching name
--                                     (verified live), so Owner is the
--                                     database's own schema name, which is
--                                     the real owning platform subsystem per
--                                     its governance manual (e.g. Archival,
--                                     Diagnostics, Integration, Reporting,
--                                     Identity, PredictiveSafety, ExportImport).
--                  SLA              - intentionally left NULL everywhere; no
--                                     confirmed SLA numbers exist for these
--                                     tables (see column-level extended
--                                     property below - "pending SLA
--                                     definition", not fabricated).
--                  SourceOfTruth    - 'Domain-authoritative' for CoreDB
--                                     tables backed by a real PlantSafety.
--                                     Domain aggregate/entity namespace;
--                                     'Database-authoritative' for tables
--                                     with no corresponding Domain-layer
--                                     source (platform/governance tables and
--                                     the 7 non-CoreDB databases), i.e. the
--                                     database schema itself is the
--                                     authoritative definition, not a
--                                     projection from a C# aggregate.
--                  PII_Classification - cross-referenced live against
--                                     sys.sensitivity_classifications (Phase
--                                     7 script 72): tables with 1+ classified
--                                     column(s) are flagged; all others are
--                                     'None classified'.
--
-- Idempotency:   CREATE TABLE guarded by existence check. Row population
--                uses MERGE keyed on (SchemaName, TableName) so re-running
--                updates existing rows in place rather than duplicating.
--                UpdatedUtc is only set on genuine content change.
--                Documentation-only; zero rows of application data touched.
-- =============================================
SET NOCOUNT ON;
GO

------------------------------------------------------------
-- 0. Reusable table-creation script text, run per database.
------------------------------------------------------------
DECLARE @Databases TABLE (DbName sysname, DomainMapped bit);
-- DomainMapped = 1 means: schemas in this DB may map 1:1 onto a real
-- PlantSafety.Domain namespace (CoreDB only, verified live 2026-07-19).
INSERT INTO @Databases VALUES
    (N'PlantSafety.CoreDB', 1),
    (N'PlantSafety.ArchiveDB', 0),
    (N'PlantSafety.CloudIdentityDB', 0),
    (N'PlantSafety.PredictiveDB', 0),
    (N'PlantSafety.ReportingDB', 0),
    (N'PlantSafety.IntegrationDB', 0),
    (N'PlantSafety.DiagnosticsDB', 0),
    (N'PlantSafety.ExportImportDB', 0);

-- Schemas in PlantSafety.CoreDB verified live (2026-07-19) to have an
-- identically-named PlantSafety.Domain/<Name> folder:
DECLARE @DomainNamespaces TABLE (SchemaName sysname);
INSERT INTO @DomainNamespaces VALUES
    (N'AiGovernance'), (N'AiSafetyIntelligence'), (N'Analytics'), (N'Compliance'),
    (N'ContractorVisitor'), (N'DigitalTwin'), (N'EnterpriseFabric'), (N'EnvironmentalHealth'),
    (N'Governance'), (N'HumanFactors'), (N'Operational'), (N'OperationalSafety'),
    (N'ProcessManagement'), (N'SafetyEvents'), (N'Sustainability'), (N'WorkerWellBeing');

DECLARE @DbName sysname, @DomainMapped bit, @sql nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT DbName, DomainMapped FROM @Databases;
OPEN cur;
FETCH NEXT FROM cur INTO @DbName, @DomainMapped;
WHILE @@FETCH_STATUS = 0
BEGIN
    ------------------------------------------------------------
    -- 1. Create System.DataDictionary if missing.
    ------------------------------------------------------------
    SET @sql = N'
    USE ' + QUOTENAME(@DbName) + N';
    IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N''System'')
        EXEC(''CREATE SCHEMA [System]'');';
    EXEC sp_executesql @sql;

    SET @sql = N'
    USE ' + QUOTENAME(@DbName) + N';
    IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N''System'' AND t.name = N''DataDictionary'')
    BEGIN
        CREATE TABLE [System].[DataDictionary] (
            DataDictionaryId   BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_System_DataDictionary PRIMARY KEY CLUSTERED,
            SchemaName         sysname          NOT NULL,
            TableName          sysname          NOT NULL,
            Owner              nvarchar(200)    NOT NULL,
            SLA                nvarchar(200)    NULL,
            SourceOfTruth      nvarchar(400)    NOT NULL,
            PII_Classification nvarchar(400)    NOT NULL,
            Notes              nvarchar(1000)   NULL,
            CreatedUtc         datetime2(3)     NOT NULL CONSTRAINT DF_System_DataDictionary_CreatedUtc DEFAULT (SYSUTCDATETIME()),
            UpdatedUtc         datetime2(3)     NULL,
            CONSTRAINT UQ_System_DataDictionary_Schema_Table UNIQUE (SchemaName, TableName)
        );
    END';
    EXEC sp_executesql @sql;

    ------------------------------------------------------------
    -- 2. Populate/refresh one row per base table in this database
    --    (excluding System.DataDictionary itself).
    ------------------------------------------------------------
    IF @DomainMapped = 1
    BEGIN
        SET @sql = N'
        USE ' + QUOTENAME(@DbName) + N';
        MERGE [System].[DataDictionary] AS tgt
        USING (
            SELECT
                s.name AS SchemaName,
                t.name AS TableName,
                CASE WHEN s.name = N''System'' THEN N''System (platform governance)''
                     ELSE s.name END AS Owner,
                CASE WHEN s.name = N''System'' THEN N''Database-authoritative (platform/governance table; no corresponding PlantSafety.Domain aggregate -- schema defined directly per Enterprise Data & Database Architecture Master Document, not projected from a C# aggregate).''
                     WHEN EXISTS (SELECT 1 FROM @dn dn WHERE dn.SchemaName = s.name) THEN N''Domain-authoritative (table is a direct projection of a PlantSafety.Domain.'' + s.name + N'' aggregate/entity class; the C# Domain model is the source of truth, this table is a persisted read/write shape of it).''
                     ELSE N''Database-authoritative (no matching PlantSafety.Domain namespace found for schema '' + s.name + N'' as of 2026-07-19 -- flagged, not guessed).''
                END AS SourceOfTruth,
                CASE WHEN EXISTS (SELECT 1 FROM sys.sensitivity_classifications sc WHERE sc.major_id = t.object_id)
                     THEN N''Contains classified PII/Health column(s) -- see sys.sensitivity_classifications for the specific column(s) and label (set by Phase 7 script 72-data-classification.sql).''
                     ELSE N''None classified (no rows in sys.sensitivity_classifications for this table as of the last Phase 7 classification pass; not re-verified independently here).''
                END AS PII_Classification
            FROM sys.tables t
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE NOT (s.name = N''System'' AND t.name = N''DataDictionary'')
        ) AS src
        ON tgt.SchemaName = src.SchemaName AND tgt.TableName = src.TableName
        WHEN MATCHED AND (tgt.Owner <> src.Owner OR tgt.SourceOfTruth <> src.SourceOfTruth OR tgt.PII_Classification <> src.PII_Classification) THEN
            UPDATE SET Owner = src.Owner, SourceOfTruth = src.SourceOfTruth, PII_Classification = src.PII_Classification, UpdatedUtc = SYSUTCDATETIME()
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (SchemaName, TableName, Owner, SLA, SourceOfTruth, PII_Classification, Notes)
            VALUES (src.SchemaName, src.TableName, src.Owner, NULL, src.SourceOfTruth, src.PII_Classification, NULL);';
        -- @dn table variable cannot cross sp_executesql scope directly; inline the values instead.
        SET @sql = REPLACE(@sql, N'@dn dn', N'(SELECT SchemaName FROM (VALUES ' +
            (SELECT STRING_AGG(CAST(N'(N''' + SchemaName + N''')' AS nvarchar(max)), N',') FROM @DomainNamespaces) +
            N') AS x(SchemaName)) dn');
        EXEC sp_executesql @sql;
    END
    ELSE
    BEGIN
        SET @sql = N'
        USE ' + QUOTENAME(@DbName) + N';
        MERGE [System].[DataDictionary] AS tgt
        USING (
            SELECT
                s.name AS SchemaName,
                t.name AS TableName,
                CASE WHEN s.name = N''System'' THEN N''System (platform governance)'' ELSE s.name END AS Owner,
                CASE WHEN s.name = N''System'' THEN N''Database-authoritative (platform/governance table; no corresponding PlantSafety.Domain aggregate).''
                     ELSE N''Database-authoritative (no PlantSafety.Domain namespace named '' + s.name + N'' exists as of 2026-07-19 -- this is a platform-subsystem database with its own governance manual, not a projection of a C# Domain aggregate; schema is authoritative in its own right).''
                END AS SourceOfTruth,
                CASE WHEN EXISTS (SELECT 1 FROM sys.sensitivity_classifications sc WHERE sc.major_id = t.object_id)
                     THEN N''Contains classified PII/Health column(s) -- see sys.sensitivity_classifications for detail.''
                     ELSE N''None classified (no rows in sys.sensitivity_classifications for this table).''
                END AS PII_Classification
            FROM sys.tables t
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE NOT (s.name = N''System'' AND t.name = N''DataDictionary'')
        ) AS src
        ON tgt.SchemaName = src.SchemaName AND tgt.TableName = src.TableName
        WHEN MATCHED AND (tgt.Owner <> src.Owner OR tgt.SourceOfTruth <> src.SourceOfTruth OR tgt.PII_Classification <> src.PII_Classification) THEN
            UPDATE SET Owner = src.Owner, SourceOfTruth = src.SourceOfTruth, PII_Classification = src.PII_Classification, UpdatedUtc = SYSUTCDATETIME()
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (SchemaName, TableName, Owner, SLA, SourceOfTruth, PII_Classification, Notes)
            VALUES (src.SchemaName, src.TableName, src.Owner, NULL, src.SourceOfTruth, src.PII_Classification, NULL);';
        EXEC sp_executesql @sql;
    END

    FETCH NEXT FROM cur INTO @DbName, @DomainMapped;
END
CLOSE cur;
DEALLOCATE cur;
GO

------------------------------------------------------------
-- 3. Document the table itself + the SLA column's "pending" status.
------------------------------------------------------------
DECLARE @Databases2 TABLE (DbName sysname);
INSERT INTO @Databases2 VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB');

DECLARE @DbName2 sysname, @sql2 nvarchar(max);
DECLARE cur2 CURSOR LOCAL FAST_FORWARD FOR SELECT DbName FROM @Databases2;
OPEN cur2;
FETCH NEXT FROM cur2 INTO @DbName2;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql2 = N'
    USE ' + QUOTENAME(@DbName2) + N';
    IF EXISTS (SELECT 1 FROM sys.extended_properties ep WHERE ep.major_id = OBJECT_ID(N''System.DataDictionary'') AND ep.minor_id = 0 AND ep.name = N''MS_Description'')
        EXEC sys.sp_updateextendedproperty @name=N''MS_Description'', @value=N''Business-value metadata catalog: one row per table in this database, capturing owning module/team, SLA (where defined), whether this table or its PlantSafety.Domain C# aggregate is the source of truth, and PII/Health sensitivity classification. Deliberately NOT a duplicate of sys.columns/sys.extended_properties -- see 143-system-data-dictionary.sql header for full rationale. Documentation Agent, 2026-07-19.'',
            @level0type=N''SCHEMA'', @level0name=N''System'', @level1type=N''TABLE'', @level1name=N''DataDictionary'';
    ELSE
        EXEC sys.sp_addextendedproperty @name=N''MS_Description'', @value=N''Business-value metadata catalog: one row per table in this database, capturing owning module/team, SLA (where defined), whether this table or its PlantSafety.Domain C# aggregate is the source of truth, and PII/Health sensitivity classification. Deliberately NOT a duplicate of sys.columns/sys.extended_properties -- see 143-system-data-dictionary.sql header for full rationale. Documentation Agent, 2026-07-19.'',
            @level0type=N''SCHEMA'', @level0name=N''System'', @level1type=N''TABLE'', @level1name=N''DataDictionary'';

    IF EXISTS (SELECT 1 FROM sys.extended_properties ep JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id WHERE ep.major_id = OBJECT_ID(N''System.DataDictionary'') AND c.name = N''SLA'' AND ep.name = N''MS_Description'')
        EXEC sys.sp_updateextendedproperty @name=N''MS_Description'', @value=N''Intentionally NULL for every row as of 2026-07-19: no confirmed, defensible SLA numbers exist yet for these tables. Pending SLA definition by the owning module -- do not fabricate a value here; populate only once a real SLA is agreed and documented.'',
            @level0type=N''SCHEMA'', @level0name=N''System'', @level1type=N''TABLE'', @level1name=N''DataDictionary'', @level2type=N''COLUMN'', @level2name=N''SLA'';
    ELSE
        EXEC sys.sp_addextendedproperty @name=N''MS_Description'', @value=N''Intentionally NULL for every row as of 2026-07-19: no confirmed, defensible SLA numbers exist yet for these tables. Pending SLA definition by the owning module -- do not fabricate a value here; populate only once a real SLA is agreed and documented.'',
            @level0type=N''SCHEMA'', @level0name=N''System'', @level1type=N''TABLE'', @level1name=N''DataDictionary'', @level2type=N''COLUMN'', @level2name=N''SLA'';';
    EXEC sp_executesql @sql2;

    FETCH NEXT FROM cur2 INTO @DbName2;
END
CLOSE cur2;
DEALLOCATE cur2;
GO

------------------------------------------------------------
-- 4. Verification: row counts per database + NULL-SLA confirmation.
------------------------------------------------------------
USE [PlantSafety.CoreDB];
GO
SELECT 'PlantSafety.CoreDB' AS DbName, COUNT(*) AS [RowCount], SUM(CASE WHEN SLA IS NULL THEN 1 ELSE 0 END) AS NullSlaCount FROM [System].[DataDictionary];
GO
USE [PlantSafety.ArchiveDB];
GO
SELECT 'PlantSafety.ArchiveDB' AS DbName, COUNT(*) AS [RowCount], SUM(CASE WHEN SLA IS NULL THEN 1 ELSE 0 END) AS NullSlaCount FROM [System].[DataDictionary];
GO
USE [PlantSafety.CloudIdentityDB];
GO
SELECT 'PlantSafety.CloudIdentityDB' AS DbName, COUNT(*) AS [RowCount], SUM(CASE WHEN SLA IS NULL THEN 1 ELSE 0 END) AS NullSlaCount FROM [System].[DataDictionary];
GO
USE [PlantSafety.PredictiveDB];
GO
SELECT 'PlantSafety.PredictiveDB' AS DbName, COUNT(*) AS [RowCount], SUM(CASE WHEN SLA IS NULL THEN 1 ELSE 0 END) AS NullSlaCount FROM [System].[DataDictionary];
GO
USE [PlantSafety.ReportingDB];
GO
SELECT 'PlantSafety.ReportingDB' AS DbName, COUNT(*) AS [RowCount], SUM(CASE WHEN SLA IS NULL THEN 1 ELSE 0 END) AS NullSlaCount FROM [System].[DataDictionary];
GO
USE [PlantSafety.IntegrationDB];
GO
SELECT 'PlantSafety.IntegrationDB' AS DbName, COUNT(*) AS [RowCount], SUM(CASE WHEN SLA IS NULL THEN 1 ELSE 0 END) AS NullSlaCount FROM [System].[DataDictionary];
GO
USE [PlantSafety.DiagnosticsDB];
GO
SELECT 'PlantSafety.DiagnosticsDB' AS DbName, COUNT(*) AS [RowCount], SUM(CASE WHEN SLA IS NULL THEN 1 ELSE 0 END) AS NullSlaCount FROM [System].[DataDictionary];
GO
USE [PlantSafety.ExportImportDB];
GO
SELECT 'PlantSafety.ExportImportDB' AS DbName, COUNT(*) AS [RowCount], SUM(CASE WHEN SLA IS NULL THEN 1 ELSE 0 END) AS NullSlaCount FROM [System].[DataDictionary];
GO
