-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Auditing.CAPATriggers
--  Layer:          Infrastructure – SQL Scripts
--  File:           CAPATriggers.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CAPATriggers read-model, backing
--                  ICAPATriggersRepository.GetTriggersAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CAPATriggers' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CAPATriggers] (
        TriggerId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        AuditingId      UNIQUEIDENTIFIER NOT NULL,
        SourceFindingId UNIQUEIDENTIFIER NULL,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        Reason          NVARCHAR(500)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        TriggeredAtUtc  DATETIME2(3)     NOT NULL,
        TriggeredBy     UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CAPATriggers_TenantId_SiteId] ON [Compliance].[CAPATriggers] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CAPATriggers_TriggeredAtUtc] ON [Compliance].[CAPATriggers] (TriggeredAtUtc);
    CREATE NONCLUSTERED INDEX [IX_CAPATriggers_AuditingId] ON [Compliance].[CAPATriggers] (AuditingId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[CAPATriggers]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[CAPATriggers] ADD RowVersion ROWVERSION NOT NULL;
END
GO
