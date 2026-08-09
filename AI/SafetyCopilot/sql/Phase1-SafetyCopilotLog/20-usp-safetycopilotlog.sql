-- =============================================
-- AI.SafetyCopilot -- Phase1-SafetyCopilotLog
-- Script:        20-usp-safetycopilotlog.sql
-- Purpose:       usp_SafetyCopilotLog_Record / GetRecentByAudience.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [AISafetyCopilot].[usp_SafetyCopilotLog_Record]
    @Audience             nvarchar(20),
    @ActionsJson          nvarchar(max),
    @RecommendationsJson  nvarchar(max),
    @GeneratedAtUtc       datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @Audience NOT IN (N'Employee', N'Supervisor')
            THROW 50001, N'Audience must be Employee or Supervisor.', 1;
        IF ISJSON(@ActionsJson) = 0 OR ISJSON(@RecommendationsJson) = 0
            THROW 50001, N'ActionsJson and RecommendationsJson must both be valid JSON.', 1;

        INSERT INTO [AISafetyCopilot].[SafetyCopilotLog] ([Audience], [ActionsJson], [RecommendationsJson], [GeneratedAtUtc])
        VALUES (@Audience, @ActionsJson, @RecommendationsJson, @GeneratedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextN0 nvarchar(200) = CONCAT(N'Params=Audience=', @Audience, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsN0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextN0, @ErrorDetailsN0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [AISafetyCopilot].[usp_SafetyCopilotLog_GetRecentByAudience]
    @Audience nvarchar(20),
    @TopN     int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @Audience IS NULL
            THROW 50001, N'Audience is required.', 1;

        SELECT TOP (@TopN) [SafetyCopilotLogId], [Audience], [ActionsJson], [RecommendationsJson], [GeneratedAtUtc]
        FROM [AISafetyCopilot].[SafetyCopilotLog]
        WHERE [Audience] = @Audience AND ISNULL([IsDeleted], 0) = 0
        ORDER BY [GeneratedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextN1 nvarchar(200) = CONCAT(N'Params=Audience=', @Audience, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsN1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextN1, @ErrorDetailsN1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
