-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessPoints
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_AccessPoints_GetByIdItem.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IAccessPointReadRepository.GetByIdAsync
--                  (Application read-model), column-aliased to AccessPointDto.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [PhysicalAccessControl].[usp_AccessPoints_GetByIdItem]
    @TenantId      UNIQUEIDENTIFIER,
    @AccessPointId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [AccessPointId],
        [TenantId],
        [SiteId],
        [AccessPointCode],
        [Description],
        [DeviceType],
        [SafetyZoneId],
        [IsPerimeter],
        [Status],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [PhysicalAccessControl].[AccessPoints]
    WHERE [TenantId] = @TenantId
      AND [AccessPointId] = @AccessPointId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
