-- =============================================
-- AI.RegulatoryInterpretation -- Phase1-RegulatoryInterpretationLog
-- Script:        20-usp-regulatoryinterpretationlog.sql
-- Purpose:       usp_RegulatoryInterpretationLog_Record / GetRecent.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [AIRegulatoryInterpretation].[usp_RegulatoryInterpretationLog_Record]
    @InputHazardTypeCode  nvarchar(30),
    @InputSeverityCode     nvarchar(30),
    @InterpretationsJson   nvarchar(max),
    @GeneratedAtUtc        datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @InputHazardTypeCode IS NULL OR @InputSeverityCode IS NULL
            THROW 50001, N'InputHazardTypeCode and InputSeverityCode are required.', 1;
        IF ISJSON(@InterpretationsJson) = 0
            THROW 50001, N'InterpretationsJson must be valid JSON.', 1;

        INSERT INTO [AIRegulatoryInterpretation].[RegulatoryInterpretationLog] (
            [InputHazardTypeCode], [InputSeverityCode], [InterpretationsJson], [GeneratedAtUtc]
        )
        VALUES (@InputHazardTypeCode, @InputSeverityCode, @InterpretationsJson, @GeneratedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextJ0 nvarchar(200) = CONCAT(N'Params=InputHazardTypeCode=', @InputHazardTypeCode, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsJ0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextJ0, @ErrorDetailsJ0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [AIRegulatoryInterpretation].[usp_RegulatoryInterpretationLog_GetRecent]
    @TopN int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT TOP (@TopN) [RegulatoryInterpretationLogId], [InputHazardTypeCode], [InputSeverityCode],
               [InterpretationsJson], [GeneratedAtUtc]
        FROM [AIRegulatoryInterpretation].[RegulatoryInterpretationLog]
        WHERE ISNULL([IsDeleted], 0) = 0
        ORDER BY [GeneratedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextJ1 nvarchar(200) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @ErrorDetailsJ1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextJ1, @ErrorDetailsJ1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
