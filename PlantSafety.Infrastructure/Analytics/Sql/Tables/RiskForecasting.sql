-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Predictive.RiskForecasting
--  Layer:          Infrastructure - SQL Scripts
--  File:           RiskForecasting.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RiskForecasting read-model aggregate backing
--                  IRiskForecastingRepository.GetRiskForecastsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RiskForecasting' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[RiskForecasting] (
        [ForecastId]           UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]             UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]               UNIQUEIDENTIFIER    NULL,
        [RiskCategory]         NVARCHAR(200)       NOT NULL,
        [ForecastValue]        DECIMAL(18,4)       NOT NULL,
        [ForecastHorizonDays]  INT                 NOT NULL,
        [Status]               NVARCHAR(50)        NOT NULL,
        [ForecastedAtUtc]      DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RiskForecasting_TenantId_ForecastedAtUtc]
        ON [Analytics].[RiskForecasting] ([TenantId], [ForecastedAtUtc])
        INCLUDE ([SiteId], [Status], [RiskCategory]);
END
GO
