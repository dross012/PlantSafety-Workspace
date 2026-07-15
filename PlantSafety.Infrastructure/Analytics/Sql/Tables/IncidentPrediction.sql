-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Predictive.IncidentPrediction
--  Layer:          Infrastructure - SQL Scripts
--  File:           IncidentPrediction.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the IncidentPrediction read-model aggregate backing
--                  IIncidentPredictionRepository.GetIncidentPredictionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IncidentPrediction' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[IncidentPrediction] (
        [PredictionId]         UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]             UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]               UNIQUEIDENTIFIER    NULL,
        [IncidentCategory]     NVARCHAR(200)       NOT NULL,
        [PredictedProbability] DECIMAL(18,4)       NOT NULL,
        [RiskLevel]            NVARCHAR(50)        NOT NULL,
        [Status]               NVARCHAR(50)        NOT NULL,
        [PredictedAtUtc]       DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_IncidentPrediction_TenantId_PredictedAtUtc]
        ON [Analytics].[IncidentPrediction] ([TenantId], [PredictedAtUtc])
        INCLUDE ([SiteId], [Status], [RiskLevel]);
END
GO
