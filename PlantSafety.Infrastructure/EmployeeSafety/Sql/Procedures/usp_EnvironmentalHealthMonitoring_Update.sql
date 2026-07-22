-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EnvironmentalHealthMonitoring_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [EmployeeSafety].[EnvironmentalHealthMonitoring], backing
--                  IEnvironmentalHealthMonitoringRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EnvironmentalHealthMonitoring_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @EnvironmentalHealthMonitoringId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @MonitoringTitle NVARCHAR(400),
    @Category NVARCHAR(100),
    @MonitoringLocation NVARCHAR(400),
    @MonitoredByUserId UNIQUEIDENTIFIER,
    @MeasuredAtUtc DATETIME2(3) = NULL,
    @NextMeasurementDueAtUtc DATETIME2(3) = NULL,
    @ExposureLevelScore DECIMAL(9,2) = NULL,
    @ExceedanceThreshold DECIMAL(9,2),
    @ExceedanceCount INT,
    @CorrectiveActionDescription NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EmployeeSafety].[EnvironmentalHealthMonitoring]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [MonitoringTitle] = @MonitoringTitle,
        [Category] = @Category,
        [MonitoringLocation] = @MonitoringLocation,
        [MonitoredByUserId] = @MonitoredByUserId,
        [MeasuredAtUtc] = @MeasuredAtUtc,
        [NextMeasurementDueAtUtc] = @NextMeasurementDueAtUtc,
        [ExposureLevelScore] = @ExposureLevelScore,
        [ExceedanceThreshold] = @ExceedanceThreshold,
        [ExceedanceCount] = @ExceedanceCount,
        [CorrectiveActionDescription] = @CorrectiveActionDescription,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [EnvironmentalHealthMonitoringId] = @EnvironmentalHealthMonitoringId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
