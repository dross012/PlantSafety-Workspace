-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Analyzers
--  Layer:          Infrastructure – SQL Scripts
--  File:           AnalyzersItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Analyzers read-model, backing
--                  IAnalyzersRepository.GetRunsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AnalyzersItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[AnalyzersItems] (
        RunId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId       UNIQUEIDENTIFIER NOT NULL,
        SiteId         UNIQUEIDENTIFIER NULL,
        AnalyzerName   NVARCHAR(200)    NOT NULL,
        TargetSystem   NVARCHAR(200)    NOT NULL,
        FindingsCount  INT              NOT NULL,
        Status         NVARCHAR(50)     NOT NULL,
        RunAtUtc       DATETIME2(3)     NOT NULL,
        TriggeredBy    UNIQUEIDENTIFIER NOT NULL,
        IsDeleted      BIT              NOT NULL CONSTRAINT DF_AnalyzersItems_IsDeleted DEFAULT (0),
        CreatedUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_AnalyzersItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy      UNIQUEIDENTIFIER NULL,
        UpdatedUtc     DATETIME2(3)     NULL,
        UpdatedBy      UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AnalyzersItems_TenantId_SiteId] ON [Integrations].[AnalyzersItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_AnalyzersItems_RunAtUtc] ON [Integrations].[AnalyzersItems] (RunAtUtc);
END
GO
