-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.NearMisses
--  Layer:          Infrastructure – SQL Scripts
--  File:           NearMisses.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the NearMisses read-model, backing
--                  INearMissesRepository.GetNearMissesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NearMisses' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[NearMisses] (
        NearMissId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        Category         NVARCHAR(100)    NOT NULL,
        PotentialSeverity NVARCHAR(50)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        ReportedAtUtc    DATETIME2(3)     NOT NULL,
        ReportedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_NearMisses_TenantId_SiteId] ON [SafetyEvents].[NearMisses] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_NearMisses_ReportedAtUtc] ON [SafetyEvents].[NearMisses] (ReportedAtUtc);
END
GO

-- ============================================================================
-- Phase 4 full build-out (2026-08-02): additive columns backing the newly built NearMisses
-- aggregate's full lifecycle (StartReview/Complete/Close/Archive) plus the standard audit-column
-- set. This table pre-existed as a read-only projection (no CreatedAtUtc/CreatedBy/IsDeleted at
-- all) -- every column below is added via an idempotent guard so re-running this script against
-- an already-migrated database is a no-op. The primary key column remains NearMissId (matches the
-- pre-existing read model / NearMissesItemDto.NearMissId); the aggregate's own NearMissesId
-- property maps onto this same column.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'ReviewStartedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD ReviewStartedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'ReviewStartedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD ReviewStartedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'CompletedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD CompletedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'Findings')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD Findings NVARCHAR(2000) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'CompletedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD CompletedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'ClosedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD ClosedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'ClosedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD ClosedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'ArchivedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD ArchivedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'CreatedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_NearMisses_CreatedAtUtc DEFAULT (SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NearMisses_CreatedBy DEFAULT ('00000000-0000-0000-0000-000000000000');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'UpdatedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD UpdatedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD UpdatedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD IsDeleted BIT NOT NULL CONSTRAINT DF_NearMisses_IsDeleted DEFAULT (0);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[NearMisses]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [SafetyEvents].[NearMisses] ADD RowVersion ROWVERSION NOT NULL;
END
GO

-- NOTE: the longest NearMissesStatus enum member is 'UnderReview' (11 chars); the pre-existing
-- Status NVARCHAR(50) column already comfortably accommodates every lifecycle status name, so no
-- widening ALTER is required here.
