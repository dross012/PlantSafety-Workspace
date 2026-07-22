-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scorecards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_KpiMeasurement_GetByScorecardId.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all KpiMeasurement child rows for a given ScorecardId.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scorecards].[usp_KpiMeasurement_GetByScorecardId]
    @ScorecardId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT KpiMeasurementId, ScorecardId, Metric, Numerator, Denominator, ComputedValue, DataSourceModule, RecordedAtUtc
    FROM [Scorecards].[KpiMeasurement]
    WHERE ScorecardId = @ScorecardId AND IsDeleted = 0
    ORDER BY RecordedAtUtc ASC;
END
GO
