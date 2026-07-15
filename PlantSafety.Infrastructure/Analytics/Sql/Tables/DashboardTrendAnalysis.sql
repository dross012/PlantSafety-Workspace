-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard.TrendAnalysis
--  Layer:          Infrastructure - SQL Scripts
--  File:           DashboardTrendAnalysis.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Dashboard.TrendAnalysis read-model aggregate backing
--                  PlantSafety.Application.Analytics.Dashboard.TrendAnalysis.Repositories.ITrendAnalysisRepository.GetTrendPointsAsync.
--                  Named DashboardTrendAnalysis (rather than TrendAnalysis) to disambiguate
--                  from the top-level Analytics.TrendAnalysis.ITrendAnalysisRepository
--                  aggregate, which has an identical interface/method/DTO shape in a
--                  different namespace.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DashboardTrendAnalysis' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[DashboardTrendAnalysis] (
        [TrendPointId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [MetricName]        NVARCHAR(200)       NOT NULL,
        [MetricValue]       DECIMAL(18,4)       NOT NULL,
        [TrendDirection]    NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [CalculatedAtUtc]   DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DashboardTrendAnalysis_TenantId_CalculatedAtUtc]
        ON [Analytics].[DashboardTrendAnalysis] ([TenantId], [CalculatedAtUtc])
        INCLUDE ([SiteId], [Status], [MetricName]);
END
GO
