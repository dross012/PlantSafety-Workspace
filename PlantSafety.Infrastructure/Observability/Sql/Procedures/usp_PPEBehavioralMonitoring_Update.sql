-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPEBehavioralMonitoring_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PPEBehavioralMonitoring aggregate root row.
--                  Backs IPPEBehavioralMonitoringRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_PPEBehavioralMonitoring_Update]
    @PPEBehavioralMonitoringId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @SensorId NVARCHAR(400),
    @ZoneCode NVARCHAR(400),
    @DetectedEventType NVARCHAR(400),
    @ConfidenceScore DECIMAL(18,4),
    @UpdatedBy UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Observability].[PPEBehavioralMonitoring] WHERE PPEBehavioralMonitoringId = @PPEBehavioralMonitoringId AND IsDeleted = 0)
            THROW 50004, N'PPEBehavioralMonitoring not found or has been deleted.', 1;

        UPDATE [Observability].[PPEBehavioralMonitoring]
        SET
            Status = @Status,
            SensorId = @SensorId,
            ZoneCode = @ZoneCode,
            DetectedEventType = @DetectedEventType,
            ConfidenceScore = @ConfidenceScore,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE PPEBehavioralMonitoringId = @PPEBehavioralMonitoringId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PPEBehavioralMonitoring', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PPEBehavioralMonitoringId=', @PPEBehavioralMonitoringId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PPEBehavioralMonitoringId=', @PPEBehavioralMonitoringId, N'; User=', SYSTEM_USER);
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
