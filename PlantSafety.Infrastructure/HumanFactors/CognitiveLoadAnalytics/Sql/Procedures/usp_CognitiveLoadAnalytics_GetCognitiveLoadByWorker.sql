-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CognitiveLoadAnalytics_GetCognitiveLoadByWorker.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Computes a single worker's cognitive-load rollup (mean/peak score, overload
--                  event count, reading count) for a period, across all of the worker's ledgers.
--                  Backs GetCognitiveLoadByWorkerQuery. Data governance: this proc returns
--                  single-worker data and must only be invoked behind an authorization check
--                  restricting access to the worker, their supervisor, or an authorized analyst.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadAnalytics_GetCognitiveLoadByWorker]
    @TenantId UNIQUEIDENTIFIER,
    @WorkerUserId UNIQUEIDENTIFIER,
    @PeriodStartUtc DATETIME2(3),
    @PeriodEndUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            @WorkerUserId AS WorkerUserId,
            @PeriodStartUtc AS PeriodStartUtc,
            @PeriodEndUtc AS PeriodEndUtc,
            ISNULL(AVG(r.CognitiveLoadScore), 0) AS MeanCognitiveLoadScore,
            ISNULL(MAX(r.CognitiveLoadScore), 0) AS PeakCognitiveLoadScore,
            SUM(CASE WHEN r.WorkloadLevel = N'Overload' THEN 1 ELSE 0 END) AS OverloadEventCount,
            COUNT(r.CognitiveLoadReadingId) AS ReadingCount
        FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading] AS r
        WHERE r.TenantId = @TenantId
          AND r.WorkerUserId = @WorkerUserId
          AND r.MeasuredAtUtc >= @PeriodStartUtc
          AND r.MeasuredAtUtc <= @PeriodEndUtc
        HAVING COUNT(r.CognitiveLoadReadingId) > 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
