-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.FeatureEngineering
--  Layer:          Infrastructure - SQL Scripts
--  File:           FeatureSet.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the FeatureSet entity, backing IFeatureSetRepository (Domain
--                  write-side). Ported (rebuilt) from the orphaned, dead
--                  Intelligence.Domain/FeatureEngineering sub-project into this real, live
--                  Intelligence module. FeatureNamesCsv stores the feature-name list as a
--                  comma-delimited string (kept flat/simple; no child feature table) since this
--                  is a small, bounded owned collection per feature set. Native RowVersion for
--                  optimistic-concurrency ETags. Mirrors [Intelligence].[DataLakeActivity]'s
--                  Phase 4 precedent.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
BEGIN
    EXEC('CREATE SCHEMA [Intelligence]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FeatureSet' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[FeatureSet] (
        FeatureSetId            UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        Name                    NVARCHAR(200)    NOT NULL,
        Version                 NVARCHAR(16)     NOT NULL,
        DataSourcePath          NVARCHAR(500)    NOT NULL,
        IsPublished             BIT              NOT NULL CONSTRAINT DF_FeatureSet_IsPublished DEFAULT (0),
        FeatureNamesCsv         NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_FeatureSet_FeatureNamesCsv DEFAULT (N''),
        ComputedAtUtc           DATETIME2(3)     NOT NULL,
        PublishedAtUtc          DATETIME2(3)     NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_FeatureSet_IsDeleted DEFAULT (0),
        CreatedAtUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_FeatureSet_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy               UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc           DATETIME2(3)     NULL,
        ModifiedBy              UNIQUEIDENTIFIER NULL,
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_FeatureSet_TenantId_SiteId] ON [Intelligence].[FeatureSet] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_FeatureSet_Name] ON [Intelligence].[FeatureSet] (TenantId, Name);
    CREATE UNIQUE NONCLUSTERED INDEX [UX_FeatureSet_TenantId_Name_Version] ON [Intelligence].[FeatureSet] (TenantId, Name, Version) WHERE IsDeleted = 0;
END
GO
