-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CognitiveLoadReading_ListByLedger.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Lists all CognitiveLoadReading child rows for a ledger, ordered by measurement
--                  time. Backs aggregate rehydration.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadReading_ListByLedger]
    @CognitiveLoadLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT CognitiveLoadReadingId, WorkerUserId, TaskComplexityProfileId, CognitiveLoadScore, WorkloadLevel, MeasurementMethod, MeasuredAtUtc, RecordedAtUtc, RecordedBy
        FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadReading]
        WHERE CognitiveLoadLedgerId = @CognitiveLoadLedgerId AND TenantId = @TenantId
        ORDER BY MeasuredAtUtc ASC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CognitiveLoadLedgerId=', @CognitiveLoadLedgerId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
