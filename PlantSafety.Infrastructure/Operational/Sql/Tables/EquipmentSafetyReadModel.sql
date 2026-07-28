-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.EquipmentSafety
--  Layer:          Infrastructure - SQL Scripts
--  File:           EquipmentSafetyReadModel.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EquipmentSafetyReadModel read-model aggregate backing
--                  IEquipmentSafetyRepository.GetEquipmentSafetyAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EquipmentSafetyReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[EquipmentSafetyReadModel] (
        [EquipmentSafetyRecordId] UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER    NULL,
        [EquipmentId]             UNIQUEIDENTIFIER    NOT NULL,
        [EquipmentName]           NVARCHAR(400)       NOT NULL,
        [Status]                NVARCHAR(50)        NOT NULL,
        [InspectedAtUtc]  DATETIME2(3)        NOT NULL,
        [InspectedBy]     UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EquipmentSafetyReadModel_TenantId_InspectedAtUtc]
        ON [Operational].[EquipmentSafetyReadModel] ([TenantId], [InspectedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[EquipmentSafetyReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[EquipmentSafetyReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO
