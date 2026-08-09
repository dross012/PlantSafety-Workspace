-- ============================================================================
--  Project:        Enterprise.PredictiveModeling -- Phase1-PredictiveModeling
--  File:           01-table-forecastlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [EnterpriseForecast].[ForecastLog] -- one
--                  shared append-only log for all four Phase D.8 forecast
--                  kinds ('Hazard'/'Compliance'/'Severity'/'Risk'), matching
--                  the union of scalar fields across
--                  EnterpriseHazardForecast/EnterpriseComplianceForecast/
--                  EnterpriseSeverityForecast/EnterpriseRiskForecast
--                  (MovingAverage/TrendSlope/ForecastedValue/IsAnomalousSpike/
--                  IsEscalating/GeneratedAtUtc).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ForecastLog' AND schema_id = SCHEMA_ID('EnterpriseForecast'))
BEGIN
    CREATE TABLE [EnterpriseForecast].[ForecastLog]
    (
        [ForecastLogId]     UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_ForecastLog_Id DEFAULT NEWSEQUENTIALID(),
        [ForecastKind]       NVARCHAR(20)      NOT NULL,
        [MovingAverage]      FLOAT             NOT NULL,
        [TrendSlope]         FLOAT             NOT NULL,
        [ForecastedValue]    FLOAT             NULL,
        [IsAnomalousSpike]   BIT               NULL,
        [IsEscalating]       BIT               NULL,
        [GeneratedAtUtc]     DATETIME2(3)      NOT NULL,

        [CreatedUtc]         DATETIME2(3)      NOT NULL CONSTRAINT DF_ForecastLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]          NVARCHAR(128)     NOT NULL CONSTRAINT DF_ForecastLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]          BIT               NOT NULL CONSTRAINT DF_ForecastLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_ForecastLog PRIMARY KEY CLUSTERED ([ForecastLogId]),
        CONSTRAINT CK_ForecastLog_ForecastKind CHECK ([ForecastKind] IN (N'Hazard', N'Compliance', N'Severity', N'Risk'))
    );
END
GO
