-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkloadTrendRecord_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Inserts a new WorkloadTrendRecord child row. Idempotent by design. Backs
--                  RecalculateWorkloadTrendCommandHandler.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_WorkloadTrendRecord_Insert]
    @WorkloadTrendRecordId UNIQUEIDENTIFIER,
    @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @PeriodStartUtc DATETIME2(3),
    @PeriodEndUtc DATETIME2(3),
    @MeanCognitiveLoadScore DECIMAL(5,2),
    @OverloadEventCount INT,
    @ReadingCount INT,
    @RecalculatedAtUtc DATETIME2(3),
    @RecalculatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord] WHERE WorkloadTrendRecordId = @WorkloadTrendRecordId)
        INSERT INTO [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord]
        (WorkloadTrendRecordId, CognitiveLoadLedgerId, TenantId, PeriodStartUtc, PeriodEndUtc, MeanCognitiveLoadScore, OverloadEventCount, ReadingCount, RecalculatedAtUtc, RecalculatedBy)
        VALUES
        (@WorkloadTrendRecordId, @CognitiveLoadLedgerId, @TenantId, @PeriodStartUtc, @PeriodEndUtc, @MeanCognitiveLoadScore, @OverloadEventCount, @ReadingCount, @RecalculatedAtUtc, @RecalculatedBy);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkloadTrendRecordId=', @WorkloadTrendRecordId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
