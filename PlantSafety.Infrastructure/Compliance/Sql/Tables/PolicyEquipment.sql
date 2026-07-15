-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyEquipment
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicyEquipment.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicyEquipment read-model aggregate backing
--                  IEquipmentRepository.GetEquipmentPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyEquipment' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyEquipment] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EquipmentCategory]       NVARCHAR(300)       NOT NULL,
        [MaintenanceRequirement]  NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyEquipment_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyEquipment] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO