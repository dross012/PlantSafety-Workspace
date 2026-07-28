-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.PredictiveAnalytics
--  Layer:          Infrastructure - SQL Scripts
--  File:           PredictiveAnalytics.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PredictiveAnalytics read-model aggregate backing
--                  IPredictiveAnalyticsRepository.GetPredictionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictiveAnalytics' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[PredictiveAnalytics] (
        [PredictionId]      UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ModelName]         NVARCHAR(200)       NOT NULL,
        [Score]             DECIMAL(18,4)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [GeneratedAtUtc]    DATETIME2(3)        NOT NULL,
        [GeneratedBy]       UNIQUEIDENTIFIER    NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PredictiveAnalytics_TenantId_GeneratedAtUtc]
        ON [Analytics].[PredictiveAnalytics] ([TenantId], [GeneratedAtUtc])
        INCLUDE ([SiteId], [Status], [ModelName]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[PredictiveAnalytics]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD RowVersion ROWVERSION NOT NULL;
END
GO
