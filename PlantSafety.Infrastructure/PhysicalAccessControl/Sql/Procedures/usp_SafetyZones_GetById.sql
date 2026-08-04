-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.SafetyZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyZones_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single SafetyZone row by identity for aggregate rehydration.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_GetById]
    @SafetyZoneId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [SafetyZoneId], [TenantId], [SiteId], [Status], [ZoneCode], [ZoneName],
        [HazardLevel], [MaxOccupancy], [CurrentOccupancy], [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[SafetyZones]
    WHERE [SafetyZoneId] = @SafetyZoneId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
