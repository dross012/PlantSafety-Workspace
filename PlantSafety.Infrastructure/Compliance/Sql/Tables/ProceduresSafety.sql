-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresSafety
--  Layer:          Infrastructure - SQL Scripts
--  File:           ProceduresSafety.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProceduresSafety read-model aggregate backing
--                  ISafetyRepository.GetSafetyProceduresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresSafety' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresSafety] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ProcedureName]           NVARCHAR(300)       NOT NULL,
        [HazardCategory]          NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresSafety_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresSafety] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO