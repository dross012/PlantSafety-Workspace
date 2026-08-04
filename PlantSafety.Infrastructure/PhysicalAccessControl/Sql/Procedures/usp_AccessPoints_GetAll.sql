-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessPoints
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessPoints_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all non-deleted AccessPoint rows for in-memory specification
--                  filtering by ListAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_GetAll]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AccessPointId], [TenantId], [SiteId], [Status], [AccessPointCode], [Description],
        [DeviceType], [SafetyZoneId], [IsPerimeter], [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[AccessPoints]
    WHERE ISNULL([IsDeleted], 0) = 0;
END
GO
