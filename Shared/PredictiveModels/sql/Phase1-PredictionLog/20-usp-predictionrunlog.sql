-- =============================================
-- Shared.PredictiveModels -- Phase1-PredictionLog
-- Script:        20-usp-predictionrunlog.sql
-- Purpose:       usp_PredictionRunLog_Record / GetRecentByModelKind.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [PredictiveModels].[usp_PredictionRunLog_Record]
    @ModelKind          nvarchar(30),
    @SubjectId           uniqueidentifier = NULL,
    @InputHistoryJson    nvarchar(max),
    @WindowSize          int,
    @MovingAverage       float,
    @TrendSlope          float = NULL,
    @ZScoreOfLatest      float = NULL,
    @PredictedValue      float = NULL,
    @IsAnomalous         bit = NULL,
    @IsEscalating        bit = NULL,
    @RunAtUtc            datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ModelKind NOT IN (N'Hazard', N'Severity', N'Exposure', N'MachineFailure', N'EmployeeInjury')
            THROW 50001, N'ModelKind must be one of Hazard/Severity/Exposure/MachineFailure/EmployeeInjury.', 1;
        IF ISJSON(@InputHistoryJson) = 0
            THROW 50001, N'InputHistoryJson must be valid JSON.', 1;

        INSERT INTO [PredictiveModels].[PredictionRunLog] (
            [ModelKind], [SubjectId], [InputHistoryJson], [WindowSize], [MovingAverage],
            [TrendSlope], [ZScoreOfLatest], [PredictedValue], [IsAnomalous], [IsEscalating], [RunAtUtc]
        )
        VALUES (
            @ModelKind, @SubjectId, @InputHistoryJson, @WindowSize, @MovingAverage,
            @TrendSlope, @ZScoreOfLatest, @PredictedValue, @IsAnomalous, @IsEscalating, @RunAtUtc
        );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext70 nvarchar(200) = CONCAT(N'Params=ModelKind=', @ModelKind, N'; SubjectId=', @SubjectId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails70 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext70, @ErrorDetails70, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [PredictiveModels].[usp_PredictionRunLog_GetRecentByModelKind]
    @ModelKind  nvarchar(30),
    @SubjectId   uniqueidentifier = NULL,
    @TopN        int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ModelKind IS NULL
            THROW 50001, N'ModelKind is required.', 1;

        SELECT TOP (@TopN) [PredictionRunLogId], [ModelKind], [SubjectId], [InputHistoryJson], [WindowSize],
               [MovingAverage], [TrendSlope], [ZScoreOfLatest], [PredictedValue], [IsAnomalous], [IsEscalating], [RunAtUtc]
        FROM [PredictiveModels].[PredictionRunLog]
        WHERE [ModelKind] = @ModelKind
          AND (@SubjectId IS NULL OR [SubjectId] = @SubjectId)
          AND ISNULL([IsDeleted], 0) = 0
        ORDER BY [RunAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext71 nvarchar(200) = CONCAT(N'Params=ModelKind=', @ModelKind, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails71 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext71, @ErrorDetails71, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
