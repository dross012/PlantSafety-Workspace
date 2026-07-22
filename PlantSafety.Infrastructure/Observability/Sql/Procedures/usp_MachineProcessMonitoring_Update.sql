-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_MachineProcessMonitoring_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a MachineProcessMonitoring aggregate root row.
--                  Backs IMachineProcessMonitoringRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_MachineProcessMonitoring_Update]
    @MachineProcessMonitoringId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @MachineId UNIQUEIDENTIFIER,
    @SensorType NVARCHAR(400),
    @Reading DECIMAL(18,4),
    @ThresholdBreached BIT,
    @AcknowledgedBy UNIQUEIDENTIFIER = NULL,
    @AnomalyDescription NVARCHAR(400) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Observability].[MachineProcessMonitoring] WHERE MachineProcessMonitoringId = @MachineProcessMonitoringId AND IsDeleted = 0)
            THROW 50004, N'MachineProcessMonitoring not found or has been deleted.', 1;

        UPDATE [Observability].[MachineProcessMonitoring]
        SET
            Status = @Status,
            MachineId = @MachineId,
            SensorType = @SensorType,
            Reading = @Reading,
            ThresholdBreached = @ThresholdBreached,
            AcknowledgedBy = @AcknowledgedBy,
            AnomalyDescription = @AnomalyDescription,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE MachineProcessMonitoringId = @MachineProcessMonitoringId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'MachineProcessMonitoring', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'MachineProcessMonitoringId=', @MachineProcessMonitoringId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'MachineProcessMonitoringId=', @MachineProcessMonitoringId, N'; User=', SYSTEM_USER);
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
