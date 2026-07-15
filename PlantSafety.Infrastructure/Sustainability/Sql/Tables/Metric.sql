-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Sustainability
--  Layer:          Infrastructure - SQL Scripts
--  File:           Metric.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for sustainability metric observations backing ISustainabilityRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Metric' AND schema_id = SCHEMA_ID('Sustainability'))
BEGIN
    CREATE TABLE [Sustainability].[Metric] (
        [MetricId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]      UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]        UNIQUEIDENTIFIER    NULL,
        [MetricName]    NVARCHAR(200)       NOT NULL,
        [Category]      NVARCHAR(50)        NOT NULL,
        [Value]         DECIMAL(18,4)       NOT NULL,
        [Unit]          NVARCHAR(20)        NOT NULL,
        [Target]        DECIMAL(18,4)       NULL,
        [PeriodEndUtc]  DATETIME2(3)        NOT NULL,
        [Status]        NVARCHAR(50)        NOT NULL CONSTRAINT DF_Metric_Status DEFAULT ('OnTrack'),
        [CreatedAtUtc]  DATETIME2(3)        NOT NULL,
        [CorrelationId] UNIQUEIDENTIFIER    NOT NULL,
        [ActorId]       UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Metric_TenantId_PeriodEndUtc]
        ON [Sustainability].[Metric] ([TenantId], [PeriodEndUtc])
        INCLUDE ([SiteId], [Category], [Value], [Status]);
END
GO
