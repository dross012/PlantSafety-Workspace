-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Analytics
--  Layer:          Infrastructure – SQL Scripts
--  File:           AnalyticsItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Analytics read-model, backing
--                  IAnalyticsRepository.GetRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AnalyticsItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[AnalyticsItems] (
        RecordId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        SourceSystem  NVARCHAR(200)    NOT NULL,
        MetricName    NVARCHAR(200)    NOT NULL,
        MetricValue   DECIMAL(18,4)    NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        IngestedAtUtc DATETIME2(3)     NOT NULL,
        IngestedBy    UNIQUEIDENTIFIER NOT NULL,
        IsDeleted     BIT              NOT NULL CONSTRAINT DF_AnalyticsItems_IsDeleted DEFAULT (0),
        CreatedUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_AnalyticsItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy     UNIQUEIDENTIFIER NULL,
        UpdatedUtc    DATETIME2(3)     NULL,
        UpdatedBy     UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AnalyticsItems_TenantId_SiteId] ON [Integrations].[AnalyticsItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_AnalyticsItems_IngestedAtUtc] ON [Integrations].[AnalyticsItems] (IngestedAtUtc);
END
GO
