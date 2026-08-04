-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationRoutes_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all EvacuationRoutes rows for in-memory specification filtering.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationRoutes_GetAll]
AS
SET NOCOUNT ON;
SELECT
    [EvacuationRouteId], [TenantId], [SiteId], [Status], [RouteName], [ZoneId], [AssemblyPointId],
    [PathGeometryRef], [TwinModelVersion], [EstimatedEgressTimeSeconds], [ObstructionCount],
    [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EvacuationRoutes]
WHERE [IsDeleted] = 0
ORDER BY [CreatedAtUtc] DESC;
GO
