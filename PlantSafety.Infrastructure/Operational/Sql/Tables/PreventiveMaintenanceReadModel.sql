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
