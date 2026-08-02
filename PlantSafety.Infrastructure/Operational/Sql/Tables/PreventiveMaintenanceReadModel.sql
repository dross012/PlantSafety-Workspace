-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PreventiveMaintenance
--  Layer:          Infrastructure - SQL Scripts
--  File:           PreventiveMaintenanceReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PreventiveMaintenanceReadModel read-model aggregate backing
--                  IPreventiveMaintenanceRepository.GetPreventiveMaintenanceAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PreventiveMaintenanceReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[PreventiveMaintenanceReadModel] (
        [PreventiveMaintenanceTaskId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EquipmentId]             UNIQUEIDENTIFIER    NOT NULL,
        [TaskDescription]         NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [ScheduledAtUtc]  DATETIME2(3)        NOT NULL,
        [CompletedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PreventiveMaintenanceReadModel_TenantId_ScheduledAtUtc]
        ON [Operational].[PreventiveMaintenanceReadModel] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO

-- ============================================================================
--  Phase 4 full build-out (2026-07-30): PreventiveMaintenanceTask aggregate.
--  This table previously backed only a read-only date-range list query
--  (usp_PreventiveMaintenance_List / IPreventiveMaintenanceRepository). It is now ALSO the
--  persistence for the full PreventiveMaintenanceTask aggregate root's write path
--  (IPreventiveMaintenanceTaskRepository / usp_PreventiveMaintenanceTask_Insert/Update/GetById).
--  All additions below are additive, idempotent ALTER TABLE guards -- no existing column is
--  dropped or renamed, and the pre-existing usp_PreventiveMaintenance_List/
--  PreventiveMaintenanceDapperRepository/GetPreventiveMaintenanceQuery read path keeps working
--  unmodified throughout.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'StartedBy')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [StartedBy] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'StartedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [StartedAtUtc] DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'CompletionNotes')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [CompletionNotes] NVARCHAR(2000) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'CompletedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [CompletedAtUtc] DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'VerifiedBy')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [VerifiedBy] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'VerifiedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [VerifiedAtUtc] DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'CancelledBy')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [CancelledBy] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'CancelledReason')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [CancelledReason] NVARCHAR(2000) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'CancelledAtUtc')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [CancelledAtUtc] DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'OverdueAtUtc')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [OverdueAtUtc] DATETIME2(3) NULL;
END
GO

-- CreatedAtUtc/CreatedBy: NOT NULL with a DEFAULT so any pre-existing rows (there should be none
-- in a dev/test environment, but be defensive) get backfilled at ALTER time; the DEFAULT is not
-- relied upon going forward since usp_PreventiveMaintenanceTask_Insert always supplies both
-- explicitly.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'CreatedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [CreatedAtUtc] DATETIME2(3) NOT NULL CONSTRAINT DF_PreventiveMaintenanceReadModel_CreatedAtUtc DEFAULT (SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [CreatedBy] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PreventiveMaintenanceReadModel_CreatedBy DEFAULT ('00000000-0000-0000-0000-000000000000');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'UpdatedAtUtc')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [UpdatedAtUtc] DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [UpdatedBy] UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD [IsDeleted] BIT NOT NULL CONSTRAINT DF_PreventiveMaintenanceReadModel_IsDeleted DEFAULT (0);
END
GO

-- Status was originally NVARCHAR(50); the new lifecycle status strings (Scheduled, InProgress,
-- Completed, Verified, Cancelled, Overdue) all comfortably fit within 50 characters, so no widen
-- is required here (checked against the longest value, "InProgress" = 10 chars). No-op guard
-- kept for documentation/traceability only -- intentionally does not ALTER COLUMN.
IF EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]')
      AND c.name = 'Status' AND t.max_length < 100
)
BEGIN
    -- Status column already NVARCHAR(50) or wider; every lifecycle status string fits. No action.
    SELECT 1;
END
GO

-- CompletedBy was originally NOT NULL. A freshly Scheduled/InProgress/Cancelled/Overdue task has
-- no CompletedBy yet, so it must become nullable to match the real aggregate lifecycle. Guarded
-- on IS_NULLABLE so this ALTER COLUMN runs at most once.
IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID('[Operational].[PreventiveMaintenanceReadModel]')
      AND c.name = 'CompletedBy' AND c.is_nullable = 0
)
BEGIN
    ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ALTER COLUMN [CompletedBy] UNIQUEIDENTIFIER NULL;
END
GO
