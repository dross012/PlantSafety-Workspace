-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Documents
--  Layer:          Infrastructure – SQL Scripts
--  File:           DocumentsItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Documents read-model, backing
--                  IDocumentsRepository.GetDocumentsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DocumentsItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[DocumentsItems] (
        DocumentId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        SourceSystem  NVARCHAR(200)    NOT NULL,
        FileName      NVARCHAR(400)    NOT NULL,
        DocumentType  NVARCHAR(100)    NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        SyncedAtUtc   DATETIME2(3)     NOT NULL,
        SyncedBy      UNIQUEIDENTIFIER NOT NULL,
        IsDeleted     BIT              NOT NULL CONSTRAINT DF_DocumentsItems_IsDeleted DEFAULT (0),
        CreatedUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_DocumentsItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy     UNIQUEIDENTIFIER NULL,
        UpdatedUtc    DATETIME2(3)     NULL,
        UpdatedBy     UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DocumentsItems_TenantId_SiteId] ON [Integrations].[DocumentsItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_DocumentsItems_SyncedAtUtc] ON [Integrations].[DocumentsItems] (SyncedAtUtc);
END
GO
