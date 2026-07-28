-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresLockoutTagoutProcedure
--  Layer:          Infrastructure - SQL Scripts
--  File:           ProceduresLockoutTagoutProcedure.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProceduresLockoutTagoutProcedure read-model aggregate backing
--                  ILockoutTagoutProcedureRepository.GetProceduresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresLockoutTagoutProcedure' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresLockoutTagoutProcedure] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EquipmentType]           NVARCHAR(300)       NOT NULL,
        [LockoutStep]             NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresLockoutTagoutProcedure_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresLockoutTagoutProcedure] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[ProceduresLockoutTagoutProcedure]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[ProceduresLockoutTagoutProcedure] ADD RowVersion ROWVERSION NOT NULL;
END
GO