-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.InspectionsEquipment
--  Layer:          Infrastructure - SQL Scripts
--  File:           InspectionsEquipment.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the InspectionsEquipment read-model aggregate backing
--                  IEquipmentRepository.GetInspectionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsEquipment' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InspectionsEquipment] (
        [InspectionId]            UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EquipmentCategory]       NVARCHAR(300)       NOT NULL,
        [ScheduledAtUtc]          DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsEquipment_TenantId_ScheduledAtUtc]
        ON [Compliance].[InspectionsEquipment] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
GO