-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresMaintenance
--  Layer:          Infrastructure - SQL Scripts
--  File:           ProceduresMaintenance.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProceduresMaintenance read-model aggregate backing
--                  IMaintenanceRepository.GetMaintenanceProceduresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresMaintenance' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresMaintenance] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [AssetCategory]           NVARCHAR(300)       NOT NULL,
        [MaintenanceTask]         NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresMaintenance_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresMaintenance] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO