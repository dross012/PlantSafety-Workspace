-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationZones_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Update procedure for [DigitalTwin].[EvacuationZones]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationZones_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc DATETIME2(3),
    @EvacuationZoneId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(100),
    @ZoneCapacity INT,
    @OccupancyThreshold INT,
    @CurrentOccupancyCount INT,
    @LastOccupancyReadingAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [DigitalTwin].[EvacuationZones]
        SET [Status] = @Status,
            [ZoneCapacity] = @ZoneCapacity,
            [OccupancyThreshold] = @OccupancyThreshold,
            [CurrentOccupancyCount] = @CurrentOccupancyCount,
            [LastOccupancyReadingAtUtc] = @LastOccupancyReadingAtUtc,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId,
            [ActorId] = @ActorId
        WHERE [EvacuationZoneId] = @EvacuationZoneId AND [TenantId] = @TenantId;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
