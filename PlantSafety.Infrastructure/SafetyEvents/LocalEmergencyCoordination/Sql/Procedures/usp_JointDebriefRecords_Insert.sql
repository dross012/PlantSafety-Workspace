-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.LocalEmergencyCoordination
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_JointDebriefRecords_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Idempotently inserts a JointDebriefRecord child entity row (INSERT WHERE NOT
--                  EXISTS -- mirrors usp_MutualAidAgreements_Insert.sql).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_JointDebriefRecords_Insert]
    @JointDebriefRecordId UNIQUEIDENTIFIER,
    @LocalEmergencyCoordinationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER = NULL,
    @AgenciesInvolved NVARCHAR(500),
    @Summary NVARCHAR(4000),
    @RecordedAtUtc DATETIME2(3),
    @RecordedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[JointDebriefRecords] WHERE JointDebriefRecordId = @JointDebriefRecordId)
        BEGIN
            INSERT INTO [SafetyEvents].[JointDebriefRecords]
            (JointDebriefRecordId, LocalEmergencyCoordinationId, TenantId, IncidentId, AgenciesInvolved, Summary, RecordedAtUtc, RecordedBy)
            VALUES
            (@JointDebriefRecordId, @LocalEmergencyCoordinationId, @TenantId, @IncidentId, @AgenciesInvolved, @Summary, @RecordedAtUtc, @RecordedBy);
        END
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'JointDebriefRecordId=', @JointDebriefRecordId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
