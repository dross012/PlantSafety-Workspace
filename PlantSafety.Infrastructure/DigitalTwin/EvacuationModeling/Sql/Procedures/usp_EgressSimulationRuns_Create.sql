-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EgressSimulationRuns_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [DigitalTwin].[EgressSimulationRuns]. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EgressSimulationRuns_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @EgressSimulationRunId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ZoneId UNIQUEIDENTIFIER,
    @TwinModelVersion NVARCHAR(100),
    @TriggerReason NVARCHAR(100),
    @SimulatedEvacuationTimeSeconds INT = NULL,
    @PredictedPeakOccupancy INT = NULL,
    @LinkedDrillId UNIQUEIDENTIFIER = NULL,
    @StartedAtUtc DATETIME2(3),
    @CompletedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [DigitalTwin].[EgressSimulationRuns]
            ([EgressSimulationRunId], [TenantId], [SiteId], [Status], [ZoneId], [TwinModelVersion], [TriggerReason], [SimulatedEvacuationTimeSeconds], [PredictedPeakOccupancy], [LinkedDrillId], [StartedAtUtc], [CompletedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@EgressSimulationRunId, @TenantId, @SiteId, @Status, @ZoneId, @TwinModelVersion, @TriggerReason, @SimulatedEvacuationTimeSeconds, @PredictedPeakOccupancy, @LinkedDrillId, @StartedAtUtc, @CompletedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
