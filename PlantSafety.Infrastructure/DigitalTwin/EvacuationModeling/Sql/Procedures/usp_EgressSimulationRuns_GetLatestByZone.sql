-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EgressSimulationRuns_GetLatestByZone.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves the most recently started EgressSimulationRuns row for a zone,
--                  backing GetLatestEgressSimulationQuery. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EgressSimulationRuns_GetLatestByZone]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @ZoneId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT TOP (1)
    [EgressSimulationRunId], [TenantId], [SiteId], [Status], [ZoneId], [TwinModelVersion], [TriggerReason],
    [SimulatedEvacuationTimeSeconds], [PredictedPeakOccupancy], [LinkedDrillId], [StartedAtUtc], [CompletedAtUtc],
    [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EgressSimulationRuns]
WHERE [TenantId] = @TenantId
  AND [ZoneId] = @ZoneId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND [IsDeleted] = 0
ORDER BY [StartedAtUtc] DESC;
GO
