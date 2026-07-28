-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.TrendAnalysis
--  Layer:          Infrastructure - SQL Scripts
--  File:           TrendAnalysis.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the top-level TrendAnalysis read-model aggregate
--                  backing PlantSafety.Application.Analytics.TrendAnalysis.Repositories.ITrendAnalysisRepository.GetTrendPointsAsync.
--                  Distinct from [Analytics].[DashboardTrendAnalysis], which backs
--                  the Dashboard-subdomain interface of the same short name.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TrendAnalysis' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[TrendAnalysis] (
        [TrendPointId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [MetricName]        NVARCHAR(200)       NOT NULL,
        [MetricValue]       DECIMAL(18,4)       NOT NULL,
        [TrendDirection]    NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [CalculatedAtUtc]   DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TrendAnalysis_TenantId_CalculatedAtUtc]
        ON [Analytics].[TrendAnalysis] ([TenantId], [CalculatedAtUtc])
        INCLUDE ([SiteId], [Status], [MetricName]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[TrendAnalysis]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[TrendAnalysis] ADD RowVersion ROWVERSION NOT NULL;
END
GO
