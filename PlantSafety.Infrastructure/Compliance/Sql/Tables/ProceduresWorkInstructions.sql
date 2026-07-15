-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresWorkInstructions
--  Layer:          Infrastructure - SQL Scripts
--  File:           ProceduresWorkInstructions.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProceduresWorkInstructions read-model aggregate backing
--                  IWorkInstructionsRepository.GetWorkInstructionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresWorkInstructions' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresWorkInstructions] (
        [InstructionId]           UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(300)       NOT NULL,
        [WorkArea]                NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresWorkInstructions_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresWorkInstructions] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO