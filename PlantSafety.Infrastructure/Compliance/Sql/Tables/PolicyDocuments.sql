-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyDocuments
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicyDocuments.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicyDocuments read-model aggregate backing
--                  IDocumentsRepository.GetDocumentsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyDocuments' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyDocuments] (
        [DocumentId]              UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [Title]                   NVARCHAR(300)       NOT NULL,
        [DocumentType]            NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [PublishedAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyDocuments_TenantId_PublishedAtUtc]
        ON [Compliance].[PolicyDocuments] ([TenantId], [PublishedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PolicyDocuments]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PolicyDocuments] ADD RowVersion ROWVERSION NOT NULL;
END
GO