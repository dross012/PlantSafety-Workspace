-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RouteObstructions_GetOpenForRoute.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves the single most recent open (uncleared) obstruction for a specific
--                  route, if any. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_RouteObstructions_GetOpenForRoute]
    @EvacuationRouteId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT TOP (1)
    [RouteObstructionId], [EvacuationRouteId], [TenantId], [Description], [Severity],
    [RecordedAtUtc], [ClearedAtUtc]
FROM [DigitalTwin].[RouteObstructions]
WHERE [EvacuationRouteId] = @EvacuationRouteId AND [ClearedAtUtc] IS NULL
ORDER BY [RecordedAtUtc] DESC;
GO
