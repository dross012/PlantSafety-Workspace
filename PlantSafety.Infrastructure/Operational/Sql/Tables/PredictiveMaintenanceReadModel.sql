-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PredictiveMaintenance
--  Layer:          Infrastructure - SQL Scripts
--  File:           PredictiveMaintenanceReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PredictiveMaintenanceReadModel read-model aggregate backing
--                  IPredictiveMaintenanceRepository.GetPredictiveMaintenanceAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictiveMaintenanceReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[PredictiveMaintenanceReadModel] (
        [PredictiveMaintenanceAlertId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EquipmentId]             UNIQUEIDENTIFIER    NOT NULL,
        [PredictedFailureType]    NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [PredictedAtUtc]  DATETIME2(3)        NOT NULL,
        [ReviewedBy]      UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PredictiveMaintenanceReadModel_TenantId_PredictedAtUtc]
        ON [Operational].[PredictiveMaintenanceReadModel] ([TenantId], [PredictedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO

-- ============================================================================
-- Phase 4 full build-out (2026-07-30): additive columns backing the newly built
-- PredictiveMaintenanceAlert aggregate's full lifecycle (Review/ScheduleAction/Resolve/
-- Dismiss/Close) plus the standard audit-column set. This table pre-existed as a read-only
-- projection (no CreatedAtUtc/CreatedBy/IsDeleted at all) -- every column below is added via
-- an idempotent guard so re-running this script against an already-migrated database is a
-- no-op. DEFAULT constraints on the ADD only backfill any pre-existing rows; new rows are
-- always populated explicitly by usp_PredictiveMaintenanceAlert_Insert going forward.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'ReviewedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD ReviewedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'ActionScheduledBy')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD ActionScheduledBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'WorkOrderReference')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD WorkOrderReference NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'ActionScheduledAtUtc')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD ActionScheduledAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'ResolvedBy')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD ResolvedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'ResolutionNotes')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD ResolutionNotes NVARCHAR(2000) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'ResolvedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD ResolvedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'DismissedBy')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD DismissedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'DismissedReason')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD DismissedReason NVARCHAR(2000) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'DismissedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD DismissedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'ClosedBy')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD ClosedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'ClosedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD ClosedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'CreatedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_PredictiveMaintenanceReadModel_CreatedAtUtc DEFAULT (SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PredictiveMaintenanceReadModel_CreatedBy DEFAULT ('00000000-0000-0000-0000-000000000000');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'UpdatedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD UpdatedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD UpdatedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PredictiveMaintenanceReadModel]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD IsDeleted BIT NOT NULL CONSTRAINT DF_PredictiveMaintenanceReadModel_IsDeleted DEFAULT (0);
END
GO

-- NOTE: the longest PredictiveMaintenanceAlertStatus enum member is 'ActionScheduled' (15
-- chars); the pre-existing Status NVARCHAR(50) column already comfortably accommodates every
-- lifecycle status name, so no widening ALTER is required here.
