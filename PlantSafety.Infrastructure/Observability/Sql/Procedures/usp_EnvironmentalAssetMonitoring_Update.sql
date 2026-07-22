-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EnvironmentalAssetMonitoring_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a EnvironmentalAssetMonitoring aggregate root row.
--                  Backs IEnvironmentalAssetMonitoringRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_EnvironmentalAssetMonitoring_Update]
    @EnvironmentalAssetMonitoringId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @SensorId NVARCHAR(400),
    @MeasurementType NVARCHAR(400),
    @Value DECIMAL(18,4),
    @ThresholdExceeded BIT,
    @UpdatedBy UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Observability].[EnvironmentalAssetMonitoring] WHERE EnvironmentalAssetMonitoringId = @EnvironmentalAssetMonitoringId AND IsDeleted = 0)
            THROW 50004, N'EnvironmentalAssetMonitoring not found or has been deleted.', 1;

        UPDATE [Observability].[EnvironmentalAssetMonitoring]
        SET
            Status = @Status,
            SensorId = @SensorId,
            MeasurementType = @MeasurementType,
            Value = @Value,
            ThresholdExceeded = @ThresholdExceeded,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE EnvironmentalAssetMonitoringId = @EnvironmentalAssetMonitoringId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'EnvironmentalAssetMonitoring', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EnvironmentalAssetMonitoringId=', @EnvironmentalAssetMonitoringId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnvironmentalAssetMonitoringId=', @EnvironmentalAssetMonitoringId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
