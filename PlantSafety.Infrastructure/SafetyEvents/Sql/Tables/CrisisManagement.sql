-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.CrisisManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           CrisisManagement.sql
--  Created:        2026-07-13
--  Updated:        2026-08-02 -- Phase 4 full build-out: this table previously backed only the
--                  read-only list query (ICrisisManagementRepository.GetCrisisEventsAsync). It now
--                  additionally backs the CrisisManagement aggregate root write model
--                  (Domain.SafetyEvents.CrisisManagement.Interfaces.ICrisisManagementRepository).
--                  Extended additively: every pre-existing column (CrisisId, TenantId, SiteId,
--                  Title, Severity, Status, DeclaredAtUtc, DeclaredBy) is untouched, and the
--                  lifecycle-attribute/audit/concurrency columns the aggregate needs
--                  (RespondedBy/RespondedAtUtc, StabilizedAtUtc, ResolvedBy/ResolvedAtUtc,
--                  ClosedBy/ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc/CreatedBy,
--                  UpdatedAtUtc/UpdatedBy, IsDeleted, RowVersion) are added via idempotent
--                  ALTER TABLE ... ADD guarded by sys.columns existence checks, mirroring
--                  [Compliance].[Permits]'s native ROWVERSION concurrency convention. CrisisId
--                  remains the physical primary-key column name (aliased to CrisisManagementId in
--                  usp_CrisisManagement_GetById / write procs' parameter mapping) to avoid
--                  disturbing the pre-existing list proc/read model.
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CrisisManagement read-model, backing
--                  ICrisisManagementRepository.GetCrisisEventsAsync (list) and the write-side
--                  aggregate repository (GetByIdAsync/AddAsync/UpdateAsync).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CrisisManagement' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[CrisisManagement] (
        CrisisId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        Title         NVARCHAR(200)    NOT NULL,
        Severity      NVARCHAR(50)     NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        DeclaredAtUtc DATETIME2(3)     NOT NULL,
        DeclaredBy    UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CrisisManagement_TenantId_SiteId] ON [SafetyEvents].[CrisisManagement] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CrisisManagement_DeclaredAtUtc] ON [SafetyEvents].[CrisisManagement] (DeclaredAtUtc);
END
GO

-- ---- Phase 4 additive extension: aggregate lifecycle attributes + audit/concurrency columns ----
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'RespondedBy')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD RespondedBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'RespondedAtUtc')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD RespondedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'StabilizedAtUtc')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD StabilizedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'ResolvedBy')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD ResolvedBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'ResolvedAtUtc')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD ResolvedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'ClosedBy')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD ClosedBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'ClosedAtUtc')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD ClosedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'ArchivedAtUtc')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD ArchivedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'CreatedAtUtc')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD CreatedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'CreatedBy')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD CreatedBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'UpdatedAtUtc')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD UpdatedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'UpdatedBy')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD UpdatedBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'IsDeleted')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD IsDeleted BIT NOT NULL CONSTRAINT DF_CrisisManagement_IsDeleted DEFAULT (0);
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]') AND name = 'RowVersion')
    ALTER TABLE [SafetyEvents].[CrisisManagement] ADD RowVersion ROWVERSION;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CrisisManagement_CreatedAtUtc' AND object_id = OBJECT_ID('[SafetyEvents].[CrisisManagement]'))
    CREATE NONCLUSTERED INDEX [IX_CrisisManagement_CreatedAtUtc] ON [SafetyEvents].[CrisisManagement] (CreatedAtUtc);
GO
