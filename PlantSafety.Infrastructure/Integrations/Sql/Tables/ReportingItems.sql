-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Reporting
--  Layer:          Infrastructure – SQL Scripts
--  File:           ReportingItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Reporting read-model, backing
--                  IReportingRepository.GetReportDeliveriesAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReportingItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[ReportingItems] (
        DeliveryId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        ReportName       NVARCHAR(300)    NOT NULL,
        RecipientSystem  NVARCHAR(200)    NOT NULL,
        DeliveryChannel  NVARCHAR(50)     NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        DeliveredAtUtc   DATETIME2(3)     NOT NULL,
        InitiatedBy      UNIQUEIDENTIFIER NOT NULL,
        IsDeleted        BIT              NOT NULL CONSTRAINT DF_ReportingItems_IsDeleted DEFAULT (0),
        CreatedUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_ReportingItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy        UNIQUEIDENTIFIER NULL,
        UpdatedUtc       DATETIME2(3)     NULL,
        UpdatedBy        UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ReportingItems_TenantId_SiteId] ON [Integrations].[ReportingItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_ReportingItems_DeliveredAtUtc] ON [Integrations].[ReportingItems] (DeliveredAtUtc);
END
GO
