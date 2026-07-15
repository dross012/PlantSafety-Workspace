-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EnvironmentalHealth_GetById.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row procedure for
--                  [EnvironmentalHealth].[EnvironmentalHealthReadings], backing
--                  IEnvironmentalHealthReadingRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EnvironmentalHealth_GetById]
    @TenantId               UNIQUEIDENTIFIER,
    @EnvironmentalHealthId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
        [EnvironmentalHealthId],
        [TenantId],
        [SiteId],
        [MetricType],
        [MeasurementValue],
        [Unit],
        [Threshold],
        [Status],
        [RecordedAtUtc]
FROM [EnvironmentalHealth].[EnvironmentalHealthReadings]
WHERE [TenantId] = @TenantId
  AND [EnvironmentalHealthId] = @EnvironmentalHealthId;
GO
