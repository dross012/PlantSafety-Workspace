-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EnvironmentalHealthMonitoring_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [EmployeeSafety].[EnvironmentalHealthMonitoring], backing
--                  IEnvironmentalHealthMonitoringRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_EnvironmentalHealthMonitoring_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [EmployeeSafety].[EnvironmentalHealthMonitoring]
            ([EnvironmentalHealthMonitoringId], [TenantId], [SiteId], [Status], [MonitoringTitle], [Category], [MonitoringLocation], [MonitoredByUserId], [MeasuredAtUtc], [NextMeasurementDueAtUtc], [ExposureLevelScore], [ExceedanceThreshold], [ExceedanceCount], [CorrectiveActionDescription], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@EnvironmentalHealthMonitoringId, @TenantId, @SiteId, @Status, @MonitoringTitle, @Category, @MonitoringLocation, @MonitoredByUserId, @MeasuredAtUtc, @NextMeasurementDueAtUtc, @ExposureLevelScore, @ExceedanceThreshold, @ExceedanceCount, @CorrectiveActionDescription, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
