-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CognitiveRiskPrediction_UpdateAcknowledgement.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Updates a CognitiveRiskPrediction row's lifecycle status and acknowledgement
--                  fields. Backs ICognitiveRiskPredictionRepository.UpdateAsync
--                  (Acknowledge/Dismiss transitions).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_UpdateAcknowledgement]
    @CognitiveRiskPredictionId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @AcknowledgedAtUtc DATETIME2(3) = NULL,
    @AcknowledgedBy UNIQUEIDENTIFIER = NULL,
    @AcknowledgementNotes NVARCHAR(1000) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction]
        SET Status = @Status,
            AcknowledgedAtUtc = @AcknowledgedAtUtc,
            AcknowledgedBy = @AcknowledgedBy,
            AcknowledgementNotes = @AcknowledgementNotes
        WHERE CognitiveRiskPredictionId = @CognitiveRiskPredictionId AND TenantId = @TenantId AND IsDeleted = 0;

        SELECT @@ROWCOUNT AS RowsAffected;
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
