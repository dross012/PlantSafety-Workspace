-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresInspection
--  Layer:          Infrastructure - SQL Scripts
--  File:           ProceduresInspection.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProceduresInspection read-model aggregate backing
--                  IInspectionRepository.GetInspectionProceduresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresInspection' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresInspection] (
        [ProcedureId]             UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [InspectionType]          NVARCHAR(300)       NOT NULL,
        [Frequency]               NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresInspection_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresInspection] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[ProceduresInspection]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[ProceduresInspection] ADD RowVersion ROWVERSION NOT NULL;
END
GO