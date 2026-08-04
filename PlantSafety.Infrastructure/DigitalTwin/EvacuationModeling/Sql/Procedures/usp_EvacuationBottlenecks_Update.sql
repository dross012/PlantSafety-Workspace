-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationBottlenecks_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Update procedure for [DigitalTwin].[EvacuationBottlenecks]. Backs
--                  RecordRecurrence and MarkMitigated aggregate transitions. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationBottlenecks_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc DATETIME2(3),
    @EvacuationBottleneckId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(100),
    @SimulationRunId UNIQUEIDENTIFIER,
    @DelaySeconds INT,
    @RecurrenceCount INT
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [DigitalTwin].[EvacuationBottlenecks]
        SET [Status] = @Status,
            [SimulationRunId] = @SimulationRunId,
            [DelaySeconds] = @DelaySeconds,
            [RecurrenceCount] = @RecurrenceCount,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId,
            [ActorId] = @ActorId
        WHERE [EvacuationBottleneckId] = @EvacuationBottleneckId AND [TenantId] = @TenantId;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
