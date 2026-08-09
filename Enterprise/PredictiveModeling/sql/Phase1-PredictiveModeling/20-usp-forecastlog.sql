-- =============================================
-- Enterprise.PredictiveModeling -- Phase1-PredictiveModeling
-- Script:        20-usp-forecastlog.sql
-- Purpose:       usp_ForecastLog_Record / GetRecentByKind.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [EnterpriseForecast].[usp_ForecastLog_Record]
    @ForecastKind      nvarchar(20),
    @MovingAverage      float,
    @TrendSlope         float,
    @ForecastedValue    float = NULL,
    @IsAnomalousSpike   bit = NULL,
    @IsEscalating       bit = NULL,
    @GeneratedAtUtc     datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ForecastKind NOT IN (N'Hazard', N'Compliance', N'Severity', N'Risk')
            THROW 50001, N'ForecastKind must be one of Hazard/Compliance/Severity/Risk.', 1;

        INSERT INTO [EnterpriseForecast].[ForecastLog] (
            [ForecastKind], [MovingAverage], [TrendSlope], [ForecastedValue], [IsAnomalousSpike], [IsEscalating], [GeneratedAtUtc]
        )
        VALUES (
            @ForecastKind, @MovingAverage, @TrendSlope, @ForecastedValue, @IsAnomalousSpike, @IsEscalating, @GeneratedAtUtc
        );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextE0 nvarchar(200) = CONCAT(N'Params=ForecastKind=', @ForecastKind, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsE0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextE0, @ErrorDetailsE0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EnterpriseForecast].[usp_ForecastLog_GetRecentByKind]
    @ForecastKind nvarchar(20),
    @TopN         int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ForecastKind IS NULL
            THROW 50001, N'ForecastKind is required.', 1;

        SELECT TOP (@TopN) [ForecastLogId], [ForecastKind], [MovingAverage], [TrendSlope],
               [ForecastedValue], [IsAnomalousSpike], [IsEscalating], [GeneratedAtUtc]
        FROM [EnterpriseForecast].[ForecastLog]
        WHERE [ForecastKind] = @ForecastKind AND ISNULL([IsDeleted], 0) = 0
        ORDER BY [GeneratedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextE1 nvarchar(200) = CONCAT(N'Params=ForecastKind=', @ForecastKind, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsE1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextE1, @ErrorDetailsE1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
