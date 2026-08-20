-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.EnvironmentalMonitoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EnvironmentalMonitoring_List.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[EnvironmentalMonitoringReadModel],
--                  backing IEnvironmentalMonitoringRepository.GetEnvironmentalMonitoringAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_EnvironmentalMonitoring_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [EnvironmentalMonitoringRecordId],
        [TenantId],
        [SiteId],
        [SensorId],
        [SensorName],
        [ReadingType],
        [ReadingValue],
        [Status],
        [RecordedAtUtc],
        [RecordedBy]
FROM [Operational].[EnvironmentalMonitoringReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RecordedAtUtc] DESC;
GO