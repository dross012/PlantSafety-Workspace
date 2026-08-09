-- =============================================
-- AI.HazardUnderstanding -- Phase1-HazardUnderstandingLog
-- Script:        20-usp-hazardunderstandinglog.sql
-- Purpose:       usp_HazardUnderstandingLog_Record / GetRecentByHazardType.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [AIHazardUnderstanding].[usp_HazardUnderstandingLog_Record]
    @InputHazardTypeCode        nvarchar(30),
    @InputEmployeeInjuryCount    int,
    @InputMachineFailureCount    int,
    @InputEventChainLength       int,
    @ModelJson                   nvarchar(max),
    @GeneratedAtUtc               datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @InputHazardTypeCode IS NULL
            THROW 50001, N'InputHazardTypeCode is required.', 1;
        IF ISJSON(@ModelJson) = 0
            THROW 50001, N'ModelJson must be valid JSON.', 1;

        INSERT INTO [AIHazardUnderstanding].[HazardUnderstandingLog] (
            [InputHazardTypeCode], [InputEmployeeInjuryCount], [InputMachineFailureCount],
            [InputEventChainLength], [ModelJson], [GeneratedAtUtc]
        )
        VALUES (
            @InputHazardTypeCode, @InputEmployeeInjuryCount, @InputMachineFailureCount,
            @InputEventChainLength, @ModelJson, @GeneratedAtUtc
        );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextH0 nvarchar(200) = CONCAT(N'Params=InputHazardTypeCode=', @InputHazardTypeCode, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsH0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextH0, @ErrorDetailsH0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [AIHazardUnderstanding].[usp_HazardUnderstandingLog_GetRecentByHazardType]
    @InputHazardTypeCode nvarchar(30),
    @TopN                 int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @InputHazardTypeCode IS NULL
            THROW 50001, N'InputHazardTypeCode is required.', 1;

        SELECT TOP (@TopN) [HazardUnderstandingLogId], [InputHazardTypeCode], [InputEmployeeInjuryCount],
               [InputMachineFailureCount], [InputEventChainLength], [ModelJson], [GeneratedAtUtc]
        FROM [AIHazardUnderstanding].[HazardUnderstandingLog]
        WHERE [InputHazardTypeCode] = @InputHazardTypeCode AND ISNULL([IsDeleted], 0) = 0
        ORDER BY [GeneratedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextH1 nvarchar(200) = CONCAT(N'Params=InputHazardTypeCode=', @InputHazardTypeCode, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsH1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextH1, @ErrorDetailsH1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
