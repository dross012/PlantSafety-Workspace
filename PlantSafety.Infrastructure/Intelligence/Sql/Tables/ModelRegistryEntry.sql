-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.ModelRegistry
--  Layer:          Infrastructure - SQL Scripts
--  File:           ModelRegistryEntry.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team (Claude Code file-and-folder-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ModelRegistryEntry aggregate root, backing
--                  IModelRegistryEntryRepository (Domain write-side). Ported (rebuilt) from the
--                  orphaned, dead Intelligence.Domain/ModelRegistry sub-project into this real,
--                  live Intelligence module. Carries the full governance lifecycle state
--                  (Draft/PendingApproval/Approved/Deprecated/Retired) plus a native RowVersion for
--                  optimistic-concurrency ETags. Mirrors
--                  [Intelligence].[DataLakeActivity]'s Phase 4 precedent.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
BEGIN
    EXEC('CREATE SCHEMA [Intelligence]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ModelRegistryEntry' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[ModelRegistryEntry] (
        ModelRegistryEntryId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        ModelName               NVARCHAR(200)    NOT NULL,
        ModelVersion            NVARCHAR(32)     NOT NULL,
        Status                  NVARCHAR(50)     NOT NULL,
        DatasetHash             NVARCHAR(64)     NOT NULL,
        TrainingAccuracyScore   FLOAT            NOT NULL,
        RegisteredBy            UNIQUEIDENTIFIER NOT NULL,
        ApprovedBy              UNIQUEIDENTIFIER NULL,
        ApprovedAtUtc           DATETIME2(3)     NULL,
        DeprecationReason       NVARCHAR(1000)   NULL,
        DeprecatedAtUtc         DATETIME2(3)     NULL,
        RetiredAtUtc            DATETIME2(3)     NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_ModelRegistryEntry_IsDeleted DEFAULT (0),
        CreatedAtUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_ModelRegistryEntry_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy               UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc           DATETIME2(3)     NULL,
        ModifiedBy              UNIQUEIDENTIFIER NULL,
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ModelRegistryEntry_TenantId_SiteId] ON [Intelligence].[ModelRegistryEntry] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_ModelRegistryEntry_ModelName] ON [Intelligence].[ModelRegistryEntry] (TenantId, ModelName);
    CREATE UNIQUE NONCLUSTERED INDEX [UX_ModelRegistryEntry_TenantId_ModelName_Version] ON [Intelligence].[ModelRegistryEntry] (TenantId, ModelName, ModelVersion) WHERE IsDeleted = 0;
END
GO
