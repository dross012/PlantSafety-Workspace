-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationRoutes_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Update procedure for [DigitalTwin].[EvacuationRoutes]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationRoutes_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc DATETIME2(3),
    @EvacuationRouteId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @RouteName NVARCHAR(400),
    @ZoneId UNIQUEIDENTIFIER,
    @AssemblyPointId UNIQUEIDENTIFIER,
    @PathGeometryRef NVARCHAR(400),
    @TwinModelVersion NVARCHAR(100),
    @EstimatedEgressTimeSeconds INT,
    @ObstructionCount INT
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [DigitalTwin].[EvacuationRoutes]
        SET [Status] = @Status,
            [RouteName] = @RouteName,
            [ZoneId] = @ZoneId,
            [AssemblyPointId] = @AssemblyPointId,
            [PathGeometryRef] = @PathGeometryRef,
            [TwinModelVersion] = @TwinModelVersion,
            [EstimatedEgressTimeSeconds] = @EstimatedEgressTimeSeconds,
            [ObstructionCount] = @ObstructionCount,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId,
            [ActorId] = @ActorId
        WHERE [EvacuationRouteId] = @EvacuationRouteId AND [TenantId] = @TenantId;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
