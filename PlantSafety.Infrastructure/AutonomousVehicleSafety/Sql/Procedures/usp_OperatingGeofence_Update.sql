-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.OperatingGeofence
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OperatingGeofence_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [AutonomousVehicleSafety].[OperatingGeofence],
--                  backing IOperatingGeofenceRepository.UpdateAsync. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_OperatingGeofence_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
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
    @NextAuditDueAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [AutonomousVehicleSafety].[OperatingGeofence]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [GeofenceName] = @GeofenceName,
        [ZoneType] = @ZoneType,
        [BoundaryDefinition] = @BoundaryDefinition,
        [MaxSpeedKph] = @MaxSpeedKph,
        [ViolationCount] = @ViolationCount,
        [LastAuditedAtUtc] = @LastAuditedAtUtc,
        [NextAuditDueAtUtc] = @NextAuditDueAtUtc,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [OperatingGeofenceId] = @OperatingGeofenceId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
