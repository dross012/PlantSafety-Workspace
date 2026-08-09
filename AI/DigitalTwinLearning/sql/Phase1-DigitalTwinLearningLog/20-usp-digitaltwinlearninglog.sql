-- =============================================
-- AI.DigitalTwinLearning -- Phase1-DigitalTwinLearningLog
-- Script:        20-usp-digitaltwinlearninglog.sql
-- Purpose:       usp_DigitalTwinLearningLog_Record / GetRecent.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [AIDigitalTwinLearning].[usp_DigitalTwinLearningLog_Record]
    @PatternsJson          nvarchar(max),
    @RecommendationsJson   nvarchar(max),
    @GeneratedAtUtc        datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF ISJSON(@PatternsJson) = 0 OR ISJSON(@RecommendationsJson) = 0
            THROW 50001, N'PatternsJson and RecommendationsJson must both be valid JSON.', 1;

        INSERT INTO [AIDigitalTwinLearning].[DigitalTwinLearningLog] ([PatternsJson], [RecommendationsJson], [GeneratedAtUtc])
        VALUES (@PatternsJson, @RecommendationsJson, @GeneratedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextM0 nvarchar(200) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @ErrorDetailsM0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextM0, @ErrorDetailsM0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [AIDigitalTwinLearning].[usp_DigitalTwinLearningLog_GetRecent]
    @TopN int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT TOP (@TopN) [DigitalTwinLearningLogId], [PatternsJson], [RecommendationsJson], [GeneratedAtUtc]
        FROM [AIDigitalTwinLearning].[DigitalTwinLearningLog]
        WHERE ISNULL([IsDeleted], 0) = 0
        ORDER BY [GeneratedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextM1 nvarchar(200) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @ErrorDetailsM1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextM1, @ErrorDetailsM1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
