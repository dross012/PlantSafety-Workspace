-- =============================================
-- Phase 7 Governance
-- Script:        70-schema-migrations-tracking.sql
-- Purpose:       Governance finding #1 remediation.
--                1) Deploy the finished [System].[SchemaMigrations] table
--                   (see Persistence\Scripts\Tables\SchemaMigrations.sql,
--                   same canonical DDL, duplicated here for a standalone,
--                   idempotent, rerunnable migration artifact) to all 8 live
--                   databases on (local)\SQLEXPRESS.
--                2) Backfill one row per migration script that was actually
--                   applied this session across Phase1..Phase6-StoredProcedures,
--                   scanned from PlantSafety.Infrastructure\Persistence\Migrations.
--                   AppliedAtUtc is approximated from each script file's
--                   filesystem last-write-time (exact apply timestamps were
--                   not separately recorded at the time these phases ran,
--                   before this table existed) -- flagged via the Notes column.
--
-- Idempotency:   CREATE TABLE guarded by IF NOT EXISTS; each backfill INSERT
--                guarded by IF NOT EXISTS on ScriptName (belt-and-braces with
--                the UQ_System_SchemaMigrations_ScriptName constraint). Safe
--                to rerun with zero side effects.
--
-- Databases touched: PlantSafety.CoreDB, PlantSafety.ArchiveDB,
--   PlantSafety.CloudIdentityDB, PlantSafety.PredictiveDB,
--   PlantSafety.ReportingDB, PlantSafety.IntegrationDB,
--   PlantSafety.DiagnosticsDB, PlantSafety.ExportImportDB
-- No other object types touched. No application data touched.
-- =============================================

-- ############################################################################
-- PlantSafety.CoreDB
-- ############################################################################
USE [PlantSafety.CoreDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
    EXEC('CREATE SCHEMA [System]');
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'SchemaMigrations')
BEGIN
    CREATE TABLE [System].[SchemaMigrations] (
        MigrationId     UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_MigrationId DEFAULT NEWSEQUENTIALID(),
        ScriptName      NVARCHAR(260)    NOT NULL,
        Phase           NVARCHAR(100)    NULL,
        AppliedAtUtc    DATETIME2(7)     NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        AppliedBy       NVARCHAR(128)    NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedBy DEFAULT SYSTEM_USER,
        Checksum        VARBINARY(32)    NULL,
        Success         BIT              NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_Success DEFAULT (1),
        Notes           NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_System_SchemaMigrations PRIMARY KEY CLUSTERED (MigrationId),
        CONSTRAINT UQ_System_SchemaMigrations_ScriptName UNIQUE (ScriptName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'10-coredb-flesh-out-stubs-v2.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'10-coredb-flesh-out-stubs-v2.sql', N'Phase1', '2026-07-18T21:23:15.3488898', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime (System.SchemaMigrations did not exist when this script ran).');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'14-coredb-stub-indexes.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'14-coredb-stub-indexes.sql', N'Phase1', '2026-07-18T21:37:59.4046477', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'15-coredb-history-snapshot-nullability-fix.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'15-coredb-history-snapshot-nullability-fix.sql', N'Phase1', '2026-07-18T21:53:38.9566067', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'20-CoreDB-rowversion.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'20-CoreDB-rowversion.sql', N'Phase2', '2026-07-18T16:27:02.1442871', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'32-workerwellbeing-index-fix.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'32-workerwellbeing-index-fix.sql', N'Phase3', '2026-07-18T20:31:25.5202385', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'40-coredb-triggers-functions.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'40-coredb-triggers-functions.sql', N'Phase4-TriggersFunctions', '2026-07-19T02:49:06.3189721', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'50-coredb-views.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'50-coredb-views.sql', N'Phase5-Views', '2026-07-19T03:02:22.1675277', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'60-coredb-procedures.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'60-coredb-procedures.sql', N'Phase6-StoredProcedures', '2026-07-19T07:16:44.9045192', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');
GO

-- ############################################################################
-- PlantSafety.ArchiveDB
-- ############################################################################
USE [PlantSafety.ArchiveDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
    EXEC('CREATE SCHEMA [System]');
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'SchemaMigrations')
BEGIN
    CREATE TABLE [System].[SchemaMigrations] (
        MigrationId     UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_MigrationId DEFAULT NEWSEQUENTIALID(),
        ScriptName      NVARCHAR(260)    NOT NULL,
        Phase           NVARCHAR(100)    NULL,
        AppliedAtUtc    DATETIME2(7)     NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        AppliedBy       NVARCHAR(128)    NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedBy DEFAULT SYSTEM_USER,
        Checksum        VARBINARY(32)    NULL,
        Success         BIT              NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_Success DEFAULT (1),
        Notes           NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_System_SchemaMigrations PRIMARY KEY CLUSTERED (MigrationId),
        CONSTRAINT UQ_System_SchemaMigrations_ScriptName UNIQUE (ScriptName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'20-ArchiveDB-rowversion.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'20-ArchiveDB-rowversion.sql', N'Phase2', '2026-07-18T16:27:03.2013756', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'41-archivedb-triggers-functions.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'41-archivedb-triggers-functions.sql', N'Phase4-TriggersFunctions', '2026-07-19T02:51:25.3541736', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'51-archivedb-views.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'51-archivedb-views.sql', N'Phase5-Views', '2026-07-19T03:03:52.2224893', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'61-archivedb-procedures.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'61-archivedb-procedures.sql', N'Phase6-StoredProcedures', '2026-07-19T07:16:45.4163020', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');
GO

-- ############################################################################
-- PlantSafety.CloudIdentityDB
-- ############################################################################
USE [PlantSafety.CloudIdentityDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
    EXEC('CREATE SCHEMA [System]');
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'SchemaMigrations')
BEGIN
    CREATE TABLE [System].[SchemaMigrations] (
        MigrationId     UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_MigrationId DEFAULT NEWSEQUENTIALID(),
        ScriptName      NVARCHAR(260)    NOT NULL,
        Phase           NVARCHAR(100)    NULL,
        AppliedAtUtc    DATETIME2(7)     NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        AppliedBy       NVARCHAR(128)    NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedBy DEFAULT SYSTEM_USER,
        Checksum        VARBINARY(32)    NULL,
        Success         BIT              NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_Success DEFAULT (1),
        Notes           NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_System_SchemaMigrations PRIMARY KEY CLUSTERED (MigrationId),
        CONSTRAINT UQ_System_SchemaMigrations_ScriptName UNIQUE (ScriptName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'20-CloudIdentityDB-rowversion.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'20-CloudIdentityDB-rowversion.sql', N'Phase2', '2026-07-18T16:27:03.6197431', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'42-cloudidentitydb-triggers-functions.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'42-cloudidentitydb-triggers-functions.sql', N'Phase4-TriggersFunctions', '2026-07-19T02:51:25.8513204', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'52-cloudidentitydb-views.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'52-cloudidentitydb-views.sql', N'Phase5-Views', '2026-07-19T03:07:09.5265629', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'62-cloudidentitydb-procedures.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'62-cloudidentitydb-procedures.sql', N'Phase6-StoredProcedures', '2026-07-19T07:16:44.9865885', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');
GO

-- ############################################################################
-- PlantSafety.PredictiveDB
-- ############################################################################
USE [PlantSafety.PredictiveDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
    EXEC('CREATE SCHEMA [System]');
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'SchemaMigrations')
BEGIN
    CREATE TABLE [System].[SchemaMigrations] (
        MigrationId     UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_MigrationId DEFAULT NEWSEQUENTIALID(),
        ScriptName      NVARCHAR(260)    NOT NULL,
        Phase           NVARCHAR(100)    NULL,
        AppliedAtUtc    DATETIME2(7)     NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        AppliedBy       NVARCHAR(128)    NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedBy DEFAULT SYSTEM_USER,
        Checksum        VARBINARY(32)    NULL,
        Success         BIT              NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_Success DEFAULT (1),
        Notes           NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_System_SchemaMigrations PRIMARY KEY CLUSTERED (MigrationId),
        CONSTRAINT UQ_System_SchemaMigrations_ScriptName UNIQUE (ScriptName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'20-PredictiveDB-rowversion.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'20-PredictiveDB-rowversion.sql', N'Phase2', '2026-07-18T16:27:02.9827305', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'21-float-to-decimal.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'21-float-to-decimal.sql', N'Phase2', '2026-07-18T16:28:03.6862851', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'30-partitioning-templates.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'30-partitioning-templates.sql', N'Phase3', '2026-07-18T20:29:08.5117270', SYSTEM_USER, 1, N'Backfilled retroactively; multi-database script (also touches DiagnosticsDB and IntegrationDB, one row per touched DB); AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'43-predictivedb-triggers-functions.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'43-predictivedb-triggers-functions.sql', N'Phase4-TriggersFunctions', '2026-07-19T02:51:26.3546243', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'53-predictivedb-views.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'53-predictivedb-views.sql', N'Phase5-Views', '2026-07-19T03:04:18.5595540', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'63-predictivedb-procedures.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'63-predictivedb-procedures.sql', N'Phase6-StoredProcedures', '2026-07-19T07:16:45.0533383', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');
GO

-- ############################################################################
-- PlantSafety.ReportingDB
-- ############################################################################
USE [PlantSafety.ReportingDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
    EXEC('CREATE SCHEMA [System]');
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'SchemaMigrations')
BEGIN
    CREATE TABLE [System].[SchemaMigrations] (
        MigrationId     UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_MigrationId DEFAULT NEWSEQUENTIALID(),
        ScriptName      NVARCHAR(260)    NOT NULL,
        Phase           NVARCHAR(100)    NULL,
        AppliedAtUtc    DATETIME2(7)     NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        AppliedBy       NVARCHAR(128)    NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedBy DEFAULT SYSTEM_USER,
        Checksum        VARBINARY(32)    NULL,
        Success         BIT              NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_Success DEFAULT (1),
        Notes           NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_System_SchemaMigrations PRIMARY KEY CLUSTERED (MigrationId),
        CONSTRAINT UQ_System_SchemaMigrations_ScriptName UNIQUE (ScriptName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'20-ReportingDB-rowversion.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'20-ReportingDB-rowversion.sql', N'Phase2', '2026-07-18T16:27:02.5534632', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'31-reportsection-json-check.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'31-reportsection-json-check.sql', N'Phase3', '2026-07-18T20:30:36.5532938', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'44-reportingdb-triggers-functions.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'44-reportingdb-triggers-functions.sql', N'Phase4-TriggersFunctions', '2026-07-19T02:51:26.8675611', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'54-reportingdb-views.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'54-reportingdb-views.sql', N'Phase5-Views', '2026-07-19T03:04:40.7838550', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'64-reportingdb-procedures.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'64-reportingdb-procedures.sql', N'Phase6-StoredProcedures', '2026-07-19T07:16:45.0320625', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');
GO

-- ############################################################################
-- PlantSafety.IntegrationDB
-- ############################################################################
USE [PlantSafety.IntegrationDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
    EXEC('CREATE SCHEMA [System]');
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'SchemaMigrations')
BEGIN
    CREATE TABLE [System].[SchemaMigrations] (
        MigrationId     UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_MigrationId DEFAULT NEWSEQUENTIALID(),
        ScriptName      NVARCHAR(260)    NOT NULL,
        Phase           NVARCHAR(100)    NULL,
        AppliedAtUtc    DATETIME2(7)     NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        AppliedBy       NVARCHAR(128)    NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedBy DEFAULT SYSTEM_USER,
        Checksum        VARBINARY(32)    NULL,
        Success         BIT              NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_Success DEFAULT (1),
        Notes           NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_System_SchemaMigrations PRIMARY KEY CLUSTERED (MigrationId),
        CONSTRAINT UQ_System_SchemaMigrations_ScriptName UNIQUE (ScriptName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'20-IntegrationDB-rowversion.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'20-IntegrationDB-rowversion.sql', N'Phase2', '2026-07-18T16:27:02.7737142', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'30-partitioning-templates.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'30-partitioning-templates.sql', N'Phase3', '2026-07-18T20:29:08.5117270', SYSTEM_USER, 1, N'Backfilled retroactively; multi-database script (also touches DiagnosticsDB and PredictiveDB, one row per touched DB); AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'45-integrationdb-triggers-functions.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'45-integrationdb-triggers-functions.sql', N'Phase4-TriggersFunctions', '2026-07-19T02:51:27.3409391', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'55-integrationdb-views.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'55-integrationdb-views.sql', N'Phase5-Views', '2026-07-19T03:05:38.0112683', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'65-integrationdb-procedures.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'65-integrationdb-procedures.sql', N'Phase6-StoredProcedures', '2026-07-19T07:16:45.1705389', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');
GO

-- ############################################################################
-- PlantSafety.DiagnosticsDB
-- ############################################################################
USE [PlantSafety.DiagnosticsDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
    EXEC('CREATE SCHEMA [System]');
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'SchemaMigrations')
BEGIN
    CREATE TABLE [System].[SchemaMigrations] (
        MigrationId     UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_MigrationId DEFAULT NEWSEQUENTIALID(),
        ScriptName      NVARCHAR(260)    NOT NULL,
        Phase           NVARCHAR(100)    NULL,
        AppliedAtUtc    DATETIME2(7)     NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        AppliedBy       NVARCHAR(128)    NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedBy DEFAULT SYSTEM_USER,
        Checksum        VARBINARY(32)    NULL,
        Success         BIT              NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_Success DEFAULT (1),
        Notes           NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_System_SchemaMigrations PRIMARY KEY CLUSTERED (MigrationId),
        CONSTRAINT UQ_System_SchemaMigrations_ScriptName UNIQUE (ScriptName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'20-DiagnosticsDB-rowversion.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'20-DiagnosticsDB-rowversion.sql', N'Phase2', '2026-07-18T16:27:02.3491685', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'30-partitioning-templates.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'30-partitioning-templates.sql', N'Phase3', '2026-07-18T20:29:08.5117270', SYSTEM_USER, 1, N'Backfilled retroactively; multi-database script (also touches PredictiveDB and IntegrationDB, one row per touched DB); AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'46-diagnosticsdb-triggers-functions.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'46-diagnosticsdb-triggers-functions.sql', N'Phase4-TriggersFunctions', '2026-07-19T02:51:28.1215744', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'56-diagnosticsdb-views.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'56-diagnosticsdb-views.sql', N'Phase5-Views', '2026-07-19T03:03:34.2581899', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'66-diagnosticsdb-procedures.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'66-diagnosticsdb-procedures.sql', N'Phase6-StoredProcedures', '2026-07-19T07:16:45.3684257', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');
GO

-- ############################################################################
-- PlantSafety.ExportImportDB
-- ############################################################################
USE [PlantSafety.ExportImportDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
    EXEC('CREATE SCHEMA [System]');
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'SchemaMigrations')
BEGIN
    CREATE TABLE [System].[SchemaMigrations] (
        MigrationId     UNIQUEIDENTIFIER NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_MigrationId DEFAULT NEWSEQUENTIALID(),
        ScriptName      NVARCHAR(260)    NOT NULL,
        Phase           NVARCHAR(100)    NULL,
        AppliedAtUtc    DATETIME2(7)     NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedAtUtc DEFAULT SYSUTCDATETIME(),
        AppliedBy       NVARCHAR(128)    NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_AppliedBy DEFAULT SYSTEM_USER,
        Checksum        VARBINARY(32)    NULL,
        Success         BIT              NOT NULL
                             CONSTRAINT DF_System_SchemaMigrations_Success DEFAULT (1),
        Notes           NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_System_SchemaMigrations PRIMARY KEY CLUSTERED (MigrationId),
        CONSTRAINT UQ_System_SchemaMigrations_ScriptName UNIQUE (ScriptName)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'20-ExportImportDB-rowversion.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'20-ExportImportDB-rowversion.sql', N'Phase2', '2026-07-18T16:27:03.4058422', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'47-exportimportdb-triggers-functions.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'47-exportimportdb-triggers-functions.sql', N'Phase4-TriggersFunctions', '2026-07-19T02:51:28.6181276', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'57-exportimportdb-views.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'57-exportimportdb-views.sql', N'Phase5-Views', '2026-07-19T03:06:28.8690585', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');

IF NOT EXISTS (SELECT 1 FROM [System].[SchemaMigrations] WHERE ScriptName = N'67-exportimportdb-procedures.sql')
    INSERT INTO [System].[SchemaMigrations] (ScriptName, Phase, AppliedAtUtc, AppliedBy, Success, Notes)
    VALUES (N'67-exportimportdb-procedures.sql', N'Phase6-StoredProcedures', '2026-07-19T07:16:45.2539298', SYSTEM_USER, 1, N'Backfilled retroactively; AppliedAtUtc approximated from file mtime.');
GO
