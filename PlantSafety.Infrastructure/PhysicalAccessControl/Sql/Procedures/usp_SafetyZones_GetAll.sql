-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.SafetyZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyZones_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all non-deleted SafetyZone rows for in-memory specification
--                  filtering by ListAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_SafetyZones_GetAll]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [SafetyZoneId], [TenantId], [SiteId], [Status], [ZoneCode], [ZoneName],
        [HazardLevel], [MaxOccupancy], [CurrentOccupancy], [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[SafetyZones]
    WHERE ISNULL([IsDeleted], 0) = 0;
END
GO
