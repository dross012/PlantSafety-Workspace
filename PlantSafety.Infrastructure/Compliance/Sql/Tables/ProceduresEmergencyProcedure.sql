-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresEmergencyProcedure
--  Layer:          Infrastructure - SQL Scripts
--  File:           ProceduresEmergencyProcedure.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProceduresEmergencyProcedure read-model aggregate backing
--                  IEmergencyProcedureRepository.GetProceduresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresEmergencyProcedure' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresEmergencyProcedure] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ProcedureName]           NVARCHAR(300)       NOT NULL,
        [EmergencyScenario]       NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresEmergencyProcedure_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresEmergencyProcedure] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO