-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EgressSimulationRuns_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single EgressSimulationRuns row by identity. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EgressSimulationRuns_GetById]
    @EgressSimulationRunId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [EgressSimulationRunId], [TenantId], [SiteId], [Status], [ZoneId], [TwinModelVersion], [TriggerReason],
    [SimulatedEvacuationTimeSeconds], [PredictedPeakOccupancy], [LinkedDrillId], [StartedAtUtc], [CompletedAtUtc],
    [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EgressSimulationRuns]
WHERE [EgressSimulationRunId] = @EgressSimulationRunId AND [IsDeleted] = 0;
GO
