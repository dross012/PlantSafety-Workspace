-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationZones_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single EvacuationZones row by identity. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationZones_GetById]
    @EvacuationZoneId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [EvacuationZoneId], [TenantId], [SiteId], [Status], [ZoneName], [TwinModelVersion], [ZoneCapacity],
    [OccupancyThreshold], [CurrentOccupancyCount], [LastOccupancyReadingAtUtc], [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EvacuationZones]
WHERE [EvacuationZoneId] = @EvacuationZoneId AND [IsDeleted] = 0;
GO
