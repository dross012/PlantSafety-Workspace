-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OccupancyReadings_GetByZone.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves occupancy readings for a zone within a UTC date range, backing
--                  GetOccupancyByZoneQuery. Feeds the occupancy-reading-freshness KPI.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_OccupancyReadings_GetByZone]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @EvacuationZoneId UNIQUEIDENTIFIER,
    @FromUtc DATETIME2(3),
    @ToUtc DATETIME2(3)
AS
SET NOCOUNT ON;
SELECT
    [OccupancyReadingId], [EvacuationZoneId], [HeadcountEstimate], [Source], [RecordedAtUtc], [ExceededThreshold]
FROM [DigitalTwin].[OccupancyReadings]
WHERE [TenantId] = @TenantId
  AND [EvacuationZoneId] = @EvacuationZoneId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [RecordedAtUtc] DESC;
GO
