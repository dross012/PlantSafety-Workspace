-- =============================================
-- AI.SeverityAssessment -- Phase1-SeverityAssessmentLog
-- Script:        20-usp-severityassessmentlog.sql
-- Purpose:       usp_SeverityAssessmentLog_Record / GetRecent.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [AISeverityAssessment].[usp_SeverityAssessmentLog_Record]
    @InputHazardTypeCode             nvarchar(30),
    @InputEmployeeInjuryCount         int,
    @InputMachineFailureCount         int,
    @InputEventChainLength            int,
    @BaseSeverityCode                 nvarchar(30),
    @DigitalTwinSimulatedSeverityCode nvarchar(30),
    @FinalAssessedSeverityCode        nvarchar(30),
    @NotesJson                        nvarchar(max),
    @AssessedAtUtc                    datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @InputHazardTypeCode IS NULL
            THROW 50001, N'InputHazardTypeCode is required.', 1;
        IF ISJSON(@NotesJson) = 0
            THROW 50001, N'NotesJson must be valid JSON.', 1;

        INSERT INTO [AISeverityAssessment].[SeverityAssessmentLog] (
            [InputHazardTypeCode], [InputEmployeeInjuryCount], [InputMachineFailureCount], [InputEventChainLength],
            [BaseSeverityCode], [DigitalTwinSimulatedSeverityCode], [FinalAssessedSeverityCode], [NotesJson], [AssessedAtUtc]
        )
        VALUES (
            @InputHazardTypeCode, @InputEmployeeInjuryCount, @InputMachineFailureCount, @InputEventChainLength,
            @BaseSeverityCode, @DigitalTwinSimulatedSeverityCode, @FinalAssessedSeverityCode, @NotesJson, @AssessedAtUtc
        );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextK0 nvarchar(200) = CONCAT(N'Params=InputHazardTypeCode=', @InputHazardTypeCode, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsK0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextK0, @ErrorDetailsK0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [AISeverityAssessment].[usp_SeverityAssessmentLog_GetRecent]
    @TopN int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT TOP (@TopN) [SeverityAssessmentLogId], [InputHazardTypeCode], [BaseSeverityCode],
               [DigitalTwinSimulatedSeverityCode], [FinalAssessedSeverityCode], [NotesJson], [AssessedAtUtc]
        FROM [AISeverityAssessment].[SeverityAssessmentLog]
        WHERE ISNULL([IsDeleted], 0) = 0
        ORDER BY [AssessedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextK1 nvarchar(200) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @ErrorDetailsK1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextK1, @ErrorDetailsK1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
