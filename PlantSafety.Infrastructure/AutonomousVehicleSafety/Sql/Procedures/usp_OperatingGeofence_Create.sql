-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.OperatingGeofence
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OperatingGeofence_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [AutonomousVehicleSafety].[OperatingGeofence], backing
--                  IOperatingGeofenceRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_OperatingGeofence_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @OperatingGeofenceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @GeofenceName NVARCHAR(200),
    @ZoneType NVARCHAR(100),
    @BoundaryDefinition NVARCHAR(MAX),
    @MaxSpeedKph DECIMAL(9,2) = NULL,
    @ViolationCount INT,
    @LastAuditedAtUtc DATETIME2(3) = NULL,
    @NextAuditDueAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [AutonomousVehicleSafety].[OperatingGeofence]
            ([OperatingGeofenceId], [TenantId], [SiteId], [Status], [GeofenceName], [ZoneType], [BoundaryDefinition], [MaxSpeedKph], [ViolationCount], [LastAuditedAtUtc], [NextAuditDueAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@OperatingGeofenceId, @TenantId, @SiteId, @Status, @GeofenceName, @ZoneType, @BoundaryDefinition, @MaxSpeedKph, @ViolationCount, @LastAuditedAtUtc, @NextAuditDueAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
