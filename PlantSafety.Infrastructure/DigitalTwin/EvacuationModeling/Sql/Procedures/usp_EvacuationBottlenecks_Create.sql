-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationBottlenecks_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [DigitalTwin].[EvacuationBottlenecks]. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationBottlenecks_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @EvacuationBottleneckId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @RouteId UNIQUEIDENTIFIER,
    @SimulationRunId UNIQUEIDENTIFIER,
    @BottleneckLocation NVARCHAR(1000),
    @DelaySeconds INT,
    @RecurrenceCount INT,
    @DetectedAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [DigitalTwin].[EvacuationBottlenecks]
            ([EvacuationBottleneckId], [TenantId], [SiteId], [Status], [RouteId], [SimulationRunId], [BottleneckLocation], [DelaySeconds], [RecurrenceCount], [DetectedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@EvacuationBottleneckId, @TenantId, @SiteId, @Status, @RouteId, @SimulationRunId, @BottleneckLocation, @DelaySeconds, @RecurrenceCount, @DetectedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
