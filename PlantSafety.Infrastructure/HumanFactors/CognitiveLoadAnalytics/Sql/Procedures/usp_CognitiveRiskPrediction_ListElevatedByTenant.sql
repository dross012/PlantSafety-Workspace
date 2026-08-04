-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CognitiveRiskPrediction_ListElevatedByTenant.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Lists all Elevated/Critical, non-dismissed CognitiveRiskPrediction rows for a
--                  tenant/site. Backs ListElevatedCognitiveRiskPredictionsQuery / the Predictive
--                  Cognitive Risk Board dashboard.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveRiskPrediction_ListElevatedByTenant]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            CognitiveRiskPredictionId, WorkerUserId, SourceLedgerId, PredictedRiskLevel, PredictionScore,
            ModelVersion, Status, GeneratedAtUtc, AcknowledgedAtUtc
        FROM [HumanFactorsCognitiveLoadAnalytics].[CognitiveRiskPrediction]
        WHERE TenantId = @TenantId
          AND ((@SiteId IS NULL) OR SiteId = @SiteId)
          AND PredictedRiskLevel IN (N'Elevated', N'Critical')
          AND Status <> N'Dismissed'
          AND IsDeleted = 0
        ORDER BY GeneratedAtUtc DESC;
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
