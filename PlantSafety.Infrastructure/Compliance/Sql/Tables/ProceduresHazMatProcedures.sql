-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresHazMatProcedures
--  Layer:          Infrastructure - SQL Scripts
--  File:           ProceduresHazMatProcedures.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProceduresHazMatProcedures read-model aggregate backing
--                  IHazMatProceduresRepository.GetHazMatProceduresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresHazMatProcedures' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresHazMatProcedures] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [HazardousMaterial]       NVARCHAR(300)       NOT NULL,
        [HandlingStep]            NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresHazMatProcedures_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresHazMatProcedures] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO