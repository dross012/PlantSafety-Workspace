-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationBottlenecks_GetHistory.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves EvacuationBottlenecks history, optionally scoped to a route, backing
--                  GetBottleneckHistoryQuery. Feeds the bottleneck-recurrence-rate KPI.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationBottlenecks_GetHistory]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @RouteId UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
SELECT
    [EvacuationBottleneckId], [TenantId], [SiteId], [Status], [RouteId], [SimulationRunId],
    [BottleneckLocation], [DelaySeconds], [RecurrenceCount], [DetectedAtUtc], [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EvacuationBottlenecks]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND (@RouteId IS NULL OR [RouteId] = @RouteId)
  AND [IsDeleted] = 0
ORDER BY [DetectedAtUtc] DESC;
GO
