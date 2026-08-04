-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CognitiveRiskPrediction_GetById.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single CognitiveRiskPrediction row by identity, scoped to tenant.
--                  Backs ICognitiveRiskPredictionRepository.GetByIdAsync.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_GetById]
    @CognitiveRiskPredictionId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            CognitiveRiskPredictionId, TenantId, SiteId, WorkerUserId, SourceLedgerId, PredictedRiskLevel, PredictionScore,
            ModelVersion, PredictionBasis, Status, GeneratedAtUtc, GeneratedBy, AcknowledgedAtUtc, AcknowledgedBy, AcknowledgementNotes
        FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction]
        WHERE CognitiveRiskPredictionId = @CognitiveRiskPredictionId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CognitiveRiskPredictionId=', @CognitiveRiskPredictionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
