-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationRoutes_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single EvacuationRoutes row by identity. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationRoutes_GetById]
    @EvacuationRouteId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [EvacuationRouteId], [TenantId], [SiteId], [Status], [RouteName], [ZoneId], [AssemblyPointId],
    [PathGeometryRef], [TwinModelVersion], [EstimatedEgressTimeSeconds], [ObstructionCount],
    [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EvacuationRoutes]
WHERE [EvacuationRouteId] = @EvacuationRouteId AND [IsDeleted] = 0;
GO
