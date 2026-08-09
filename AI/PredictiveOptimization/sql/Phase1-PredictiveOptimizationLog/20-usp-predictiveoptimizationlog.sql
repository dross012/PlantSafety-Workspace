-- =============================================
-- AI.PredictiveOptimization -- Phase1-PredictiveOptimizationLog
-- Script:        20-usp-predictiveoptimizationlog.sql
-- Purpose:       usp_PredictiveOptimizationLog_Record / GetRecent.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [AIPredictiveOptimization].[usp_PredictiveOptimizationLog_Record]
    @ModelsJson         nvarchar(max),
    @RecommendedFocus   nvarchar(50),
    @CompositeScore     float,
    @GeneratedAtUtc     datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF ISJSON(@ModelsJson) = 0
            THROW 50001, N'ModelsJson must be valid JSON.', 1;

        INSERT INTO [AIPredictiveOptimization].[PredictiveOptimizationLog] (
            [ModelsJson], [RecommendedFocus], [CompositeScore], [GeneratedAtUtc]
        )
        VALUES (@ModelsJson, @RecommendedFocus, @CompositeScore, @GeneratedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextL0 nvarchar(200) = CONCAT(N'Params=RecommendedFocus=', @RecommendedFocus, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsL0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextL0, @ErrorDetailsL0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [AIPredictiveOptimization].[usp_PredictiveOptimizationLog_GetRecent]
    @TopN int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT TOP (@TopN) [PredictiveOptimizationLogId], [ModelsJson], [RecommendedFocus], [CompositeScore], [GeneratedAtUtc]
        FROM [AIPredictiveOptimization].[PredictiveOptimizationLog]
        WHERE ISNULL([IsDeleted], 0) = 0
        ORDER BY [GeneratedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextL1 nvarchar(200) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @ErrorDetailsL1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextL1, @ErrorDetailsL1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
