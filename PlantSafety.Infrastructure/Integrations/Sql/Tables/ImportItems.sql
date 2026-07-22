-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Import
--  Layer:          Infrastructure – SQL Scripts
--  File:           ImportItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Import read-model, backing
--                  IImportRepository.GetImportRunsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ImportItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[ImportItems] (
        ImportRunId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        SourceSystem     NVARCHAR(200)    NOT NULL,
        ImportType       NVARCHAR(100)    NOT NULL,
        RecordsProcessed INT              NOT NULL,
        RecordsFailed    INT              NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        CompletedAtUtc   DATETIME2(3)     NOT NULL,
        InitiatedBy      UNIQUEIDENTIFIER NOT NULL,
        IsDeleted        BIT              NOT NULL CONSTRAINT DF_ImportItems_IsDeleted DEFAULT (0),
        CreatedUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_ImportItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy        UNIQUEIDENTIFIER NULL,
        UpdatedUtc       DATETIME2(3)     NULL,
        UpdatedBy        UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ImportItems_TenantId_SiteId] ON [Integrations].[ImportItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_ImportItems_CompletedAtUtc] ON [Integrations].[ImportItems] (CompletedAtUtc);
END
GO
