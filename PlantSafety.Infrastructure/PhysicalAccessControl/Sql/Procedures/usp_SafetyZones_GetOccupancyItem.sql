-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.SafetyZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyZones_GetOccupancyItem.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing ISafetyZoneReadRepository.GetOccupancyAsync
--                  (Application read-model, backs the Live Access & Occupancy Map dashboard).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_GetOccupancyItem]
    @TenantId     UNIQUEIDENTIFIER,
    @SafetyZoneId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [SafetyZoneId],
        [TenantId],
        [ZoneCode],
        [ZoneName],
        [MaxOccupancy],
        [CurrentOccupancy],
        [Status],
        ISNULL([ModifiedAtUtc], [CreatedAtUtc]) AS [AsOfUtc]
    FROM [PhysicalAccessControl].[SafetyZones]
    WHERE [TenantId] = @TenantId
      AND [SafetyZoneId] = @SafetyZoneId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
