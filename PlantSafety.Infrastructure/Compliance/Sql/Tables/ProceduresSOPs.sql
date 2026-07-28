-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ProceduresSOPs
--  Layer:          Infrastructure - SQL Scripts
--  File:           ProceduresSOPs.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ProceduresSOPs read-model aggregate backing
--                  ISOPsRepository.GetSOPsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresSOPs' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresSOPs] (
        [SOPId]                   UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(300)       NOT NULL,
        [ProcessArea]             NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ProceduresSOPs_TenantId_EffectiveAtUtc]
        ON [Compliance].[ProceduresSOPs] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[ProceduresSOPs]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[ProceduresSOPs] ADD RowVersion ROWVERSION NOT NULL;
END
GO