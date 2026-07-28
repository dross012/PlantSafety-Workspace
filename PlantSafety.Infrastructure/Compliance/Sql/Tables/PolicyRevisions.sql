-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyRevisions
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicyRevisions.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicyRevisions read-model aggregate backing
--                  IRevisionsRepository.GetRevisionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyRevisions' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyRevisions] (
        [RevisionId]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL,
        [RevisionNumber]          NVARCHAR(50)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [RevisedAtUtc]            DATETIME2(3)        NOT NULL,
        [RevisedBy]               UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyRevisions_TenantId_RevisedAtUtc]
        ON [Compliance].[PolicyRevisions] ([TenantId], [RevisedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PolicyRevisions]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PolicyRevisions] ADD RowVersion ROWVERSION NOT NULL;
END
GO