-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationZones_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all EvacuationZones rows for in-memory specification filtering.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationZones_GetAll]
AS
SET NOCOUNT ON;
SELECT
    [EvacuationZoneId], [TenantId], [SiteId], [Status], [ZoneName], [TwinModelVersion], [ZoneCapacity],
    [OccupancyThreshold], [CurrentOccupancyCount], [LastOccupancyReadingAtUtc], [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EvacuationZones]
WHERE [IsDeleted] = 0
ORDER BY [CreatedAtUtc] DESC;
GO
