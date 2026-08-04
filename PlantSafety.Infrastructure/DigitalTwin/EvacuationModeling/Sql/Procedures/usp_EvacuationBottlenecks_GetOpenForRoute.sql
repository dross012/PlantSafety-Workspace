-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationBottlenecks_GetOpenForRoute.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves the single most recent unmitigated bottleneck for a route, if any,
--                  used by RunEgressSimulationCommandHandler to decide between recording a new
--                  bottleneck or a recurrence. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationBottlenecks_GetOpenForRoute]
    @RouteId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT TOP (1)
    [EvacuationBottleneckId], [TenantId], [SiteId], [Status], [RouteId], [SimulationRunId],
    [BottleneckLocation], [DelaySeconds], [RecurrenceCount], [DetectedAtUtc], [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EvacuationBottlenecks]
WHERE [RouteId] = @RouteId AND [Status] <> 'Mitigated' AND [IsDeleted] = 0
ORDER BY [DetectedAtUtc] DESC;
GO
