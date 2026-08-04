-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationRoutes_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [DigitalTwin].[EvacuationRoutes]. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationRoutes_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
    @ObstructionCount INT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [DigitalTwin].[EvacuationRoutes]
            ([EvacuationRouteId], [TenantId], [SiteId], [Status], [RouteName], [ZoneId], [AssemblyPointId], [PathGeometryRef], [TwinModelVersion], [EstimatedEgressTimeSeconds], [ObstructionCount], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@EvacuationRouteId, @TenantId, @SiteId, @Status, @RouteName, @ZoneId, @AssemblyPointId, @PathGeometryRef, @TwinModelVersion, @EstimatedEgressTimeSeconds, @ObstructionCount, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
