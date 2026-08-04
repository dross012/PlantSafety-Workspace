-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationRoutes_GetByZone.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves EvacuationRoutes read-model items for the given tenant/zone, backing
--                  ListEvacuationRoutesByZoneQuery. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationRoutes_GetByZone]
    @TenantId UNIQUEIDENTIFIER,
    @ZoneId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [EvacuationRouteId], [TenantId], [SiteId], [Status], [RouteName], [ZoneId], [AssemblyPointId],
    [PathGeometryRef], [TwinModelVersion], [EstimatedEgressTimeSeconds], [ObstructionCount],
    [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EvacuationRoutes]
WHERE [TenantId] = @TenantId AND [ZoneId] = @ZoneId AND [IsDeleted] = 0
ORDER BY [RouteName] ASC;
GO
