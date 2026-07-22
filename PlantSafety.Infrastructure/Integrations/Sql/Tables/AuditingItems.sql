-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Auditing
--  Layer:          Infrastructure – SQL Scripts
--  File:           AuditingItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Auditing read-model, backing
--                  IAuditingRepository.GetLogsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditingItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[AuditingItems] (
        LogId           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        ExternalSystem  NVARCHAR(200)    NOT NULL,
        Direction       NVARCHAR(20)     NOT NULL,
        Endpoint        NVARCHAR(400)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        OccurredAtUtc   DATETIME2(3)     NOT NULL,
        InitiatedBy     UNIQUEIDENTIFIER NOT NULL,
        IsDeleted       BIT              NOT NULL CONSTRAINT DF_AuditingItems_IsDeleted DEFAULT (0),
        CreatedUtc      DATETIME2(3)     NOT NULL CONSTRAINT DF_AuditingItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy       UNIQUEIDENTIFIER NULL,
        UpdatedUtc      DATETIME2(3)     NULL,
        UpdatedBy       UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AuditingItems_TenantId_SiteId] ON [Integrations].[AuditingItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_AuditingItems_OccurredAtUtc] ON [Integrations].[AuditingItems] (OccurredAtUtc);
END
GO
