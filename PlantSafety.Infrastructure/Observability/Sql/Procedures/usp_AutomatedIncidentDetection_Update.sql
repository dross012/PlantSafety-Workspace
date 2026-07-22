-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AutomatedIncidentDetection_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a AutomatedIncidentDetection aggregate root row.
--                  Backs IAutomatedIncidentDetectionRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Observability].[usp_AutomatedIncidentDetection_Update]
    @AutomatedIncidentDetectionId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @DetectionType NVARCHAR(400),
    @LocationDescription NVARCHAR(400),
    @NotifiedSupervisorUserId UNIQUEIDENTIFIER = NULL,
    @AcknowledgedBy UNIQUEIDENTIFIER = NULL,
    @IncidentReferenceId UNIQUEIDENTIFIER = NULL,
    @DismissalReason NVARCHAR(400) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Observability].[AutomatedIncidentDetection] WHERE AutomatedIncidentDetectionId = @AutomatedIncidentDetectionId AND IsDeleted = 0)
            THROW 50004, N'AutomatedIncidentDetection not found or has been deleted.', 1;

        UPDATE [Observability].[AutomatedIncidentDetection]
        SET
            Status = @Status,
            DetectionType = @DetectionType,
            LocationDescription = @LocationDescription,
            NotifiedSupervisorUserId = @NotifiedSupervisorUserId,
            AcknowledgedBy = @AcknowledgedBy,
            IncidentReferenceId = @IncidentReferenceId,
            DismissalReason = @DismissalReason,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE AutomatedIncidentDetectionId = @AutomatedIncidentDetectionId AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AutomatedIncidentDetection', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AutomatedIncidentDetectionId=', @AutomatedIncidentDetectionId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AutomatedIncidentDetectionId=', @AutomatedIncidentDetectionId, N'; User=', SYSTEM_USER);
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
