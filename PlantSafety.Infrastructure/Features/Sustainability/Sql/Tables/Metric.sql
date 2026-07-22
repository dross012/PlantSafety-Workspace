-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Sustainability
--  Layer:          Infrastructure – SQL Scripts
--  File:           Metric.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for sustainability metric observations, backing
--                  ISustainabilityRepository (usp_Metric_Create/GetById/List).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Sustainability')
BEGIN
    EXEC('CREATE SCHEMA [Sustainability]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Metric' AND schema_id = SCHEMA_ID('Sustainability'))
BEGIN
    CREATE TABLE [Sustainability].[Metric] (
        MetricId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId       UNIQUEIDENTIFIER NOT NULL,
        SiteId         UNIQUEIDENTIFIER NULL,
        MetricName     NVARCHAR(200)    NOT NULL,
        Category       NVARCHAR(50)     NOT NULL,
        Value          FLOAT            NOT NULL,
        Unit           NVARCHAR(20)     NOT NULL,
        Target         FLOAT            NULL,
        PeriodEndUtc   DATETIME2(3)     NOT NULL,
        Status         NVARCHAR(50)     NOT NULL,
        IsDeleted      BIT              NOT NULL CONSTRAINT DF_Sustainability_Metric_IsDeleted DEFAULT (0),
        CorrelationId  UNIQUEIDENTIFIER NULL,
        CreatedAtUtc   DATETIME2(3)     NOT NULL CONSTRAINT DF_Sustainability_Metric_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy      UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc  DATETIME2(3)     NULL,
        ModifiedBy     UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Sustainability_Metric_TenantId_SiteId] ON [Sustainability].[Metric] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_Sustainability_Metric_PeriodEndUtc] ON [Sustainability].[Metric] (PeriodEndUtc);
END
GO
