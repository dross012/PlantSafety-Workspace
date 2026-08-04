-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EgressSimulationRuns_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Update procedure for [DigitalTwin].[EgressSimulationRuns]. Backs Complete,
--                  Fail, and LinkToDrill aggregate transitions. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EgressSimulationRuns_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc DATETIME2(3),
    @EgressSimulationRunId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(100),
    @SimulatedEvacuationTimeSeconds INT = NULL,
    @PredictedPeakOccupancy INT = NULL,
    @LinkedDrillId UNIQUEIDENTIFIER = NULL,
    @CompletedAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [DigitalTwin].[EgressSimulationRuns]
        SET [Status] = @Status,
            [SimulatedEvacuationTimeSeconds] = @SimulatedEvacuationTimeSeconds,
            [PredictedPeakOccupancy] = @PredictedPeakOccupancy,
            [LinkedDrillId] = @LinkedDrillId,
            [CompletedAtUtc] = @CompletedAtUtc,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId,
            [ActorId] = @ActorId
        WHERE [EgressSimulationRunId] = @EgressSimulationRunId AND [TenantId] = @TenantId;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
