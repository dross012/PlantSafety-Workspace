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
