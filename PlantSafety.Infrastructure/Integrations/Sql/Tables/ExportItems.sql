-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Export
--  Layer:          Infrastructure – SQL Scripts
--  File:           ExportItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Export read-model, backing
--                  IExportRepository.GetExportRunsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ExportItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[ExportItems] (
        ExportRunId    UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId       UNIQUEIDENTIFIER NOT NULL,
        SiteId         UNIQUEIDENTIFIER NULL,
        TargetSystem   NVARCHAR(200)    NOT NULL,
        ExportType     NVARCHAR(100)    NOT NULL,
        RecordCount    INT              NOT NULL,
        Status         NVARCHAR(50)     NOT NULL,
        CompletedAtUtc DATETIME2(3)     NOT NULL,
        InitiatedBy    UNIQUEIDENTIFIER NOT NULL,
        IsDeleted      BIT              NOT NULL CONSTRAINT DF_ExportItems_IsDeleted DEFAULT (0),
        CreatedUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_ExportItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy      UNIQUEIDENTIFIER NULL,
        UpdatedUtc     DATETIME2(3)     NULL,
        UpdatedBy      UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ExportItems_TenantId_SiteId] ON [Integrations].[ExportItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_ExportItems_CompletedAtUtc] ON [Integrations].[ExportItems] (CompletedAtUtc);
END
GO
