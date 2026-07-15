-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture.TrendModeling
--  Layer:          Infrastructure - SQL Scripts
--  File:           TrendModeling.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the TrendModeling read-model aggregate backing
--                  ITrendModelingRepository.GetTrendModelsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TrendModeling' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[TrendModeling] (
        [TrendModelId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [MetricName]        NVARCHAR(200)       NOT NULL,
        [PredictedValue]    DECIMAL(18,4)       NOT NULL,
        [ModelConfidence]   DECIMAL(18,4)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [GeneratedAtUtc]    DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TrendModeling_TenantId_GeneratedAtUtc]
        ON [Analytics].[TrendModeling] ([TenantId], [GeneratedAtUtc])
        INCLUDE ([SiteId], [Status], [MetricName]);
END
GO
