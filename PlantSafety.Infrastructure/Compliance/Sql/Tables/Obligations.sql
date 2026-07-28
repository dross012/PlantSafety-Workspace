-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Obligations
--  Layer:          Infrastructure – SQL Scripts
--  File:           Obligations.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Obligations read-model, backing
--                  IObligationsRepository.GetObligationsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Obligations' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[Obligations] (
        ObligationId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        ObligationTitle NVARCHAR(300)    NOT NULL,
        RegulatoryBody  NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        DueAtUtc        DATETIME2(3)     NOT NULL,
        OwnerId         UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Obligations_TenantId_SiteId] ON [Compliance].[Obligations] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_Obligations_DueAtUtc] ON [Compliance].[Obligations] (DueAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[Obligations]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[Obligations] ADD RowVersion ROWVERSION NOT NULL;
END
GO
