-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EgressSimulationRuns_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves all EgressSimulationRuns rows for in-memory specification filtering.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EgressSimulationRuns_GetAll]
AS
SET NOCOUNT ON;
SELECT
    [EgressSimulationRunId], [TenantId], [SiteId], [Status], [ZoneId], [TwinModelVersion], [TriggerReason],
    [SimulatedEvacuationTimeSeconds], [PredictedPeakOccupancy], [LinkedDrillId], [StartedAtUtc], [CompletedAtUtc],
    [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EgressSimulationRuns]
WHERE [IsDeleted] = 0
ORDER BY [StartedAtUtc] DESC;
GO
