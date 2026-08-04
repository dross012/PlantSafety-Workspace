-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessPoints
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessPoints_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single AccessPoint row by identity for aggregate rehydration.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_GetById]
    @AccessPointId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AccessPointId], [TenantId], [SiteId], [Status], [AccessPointCode], [Description],
        [DeviceType], [SafetyZoneId], [IsPerimeter], [CreatedAtUtc], [CreatedBy]
    FROM [PhysicalAccessControl].[AccessPoints]
    WHERE [AccessPointId] = @AccessPointId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
