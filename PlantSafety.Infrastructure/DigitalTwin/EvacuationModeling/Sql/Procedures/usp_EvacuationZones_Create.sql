-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationZones_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [DigitalTwin].[EvacuationZones]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationZones_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @EvacuationZoneId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ZoneName NVARCHAR(400),
    @TwinModelVersion NVARCHAR(100),
    @ZoneCapacity INT,
    @OccupancyThreshold INT,
    @CurrentOccupancyCount INT,
    @LastOccupancyReadingAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [DigitalTwin].[EvacuationZones]
            ([EvacuationZoneId], [TenantId], [SiteId], [Status], [ZoneName], [TwinModelVersion], [ZoneCapacity], [OccupancyThreshold], [CurrentOccupancyCount], [LastOccupancyReadingAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@EvacuationZoneId, @TenantId, @SiteId, @Status, @ZoneName, @TwinModelVersion, @ZoneCapacity, @OccupancyThreshold, @CurrentOccupancyCount, @LastOccupancyReadingAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
