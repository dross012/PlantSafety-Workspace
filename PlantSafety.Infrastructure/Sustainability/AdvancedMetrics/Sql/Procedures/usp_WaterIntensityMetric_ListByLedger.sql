-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WaterIntensityMetric_ListByLedger.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Lists WaterIntensityMetric rows for a ledger, ordered by reading time. Backs
--                  aggregate child hydration and GetWaterIntensityTrendQuery.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_WaterIntensityMetric_ListByLedger]
    @AdvancedMetricsLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT WaterIntensityMetricId, AdvancedMetricsLedgerId, WaterConsumedCubicMeters, OutputUnits, IntensityValue, MethodologyVersion, ReadingAtUtc, RecordedAtUtc, RecordedBy
        FROM [SustainabilityAdvancedMetrics].[WaterIntensityMetric]
        WHERE AdvancedMetricsLedgerId = @AdvancedMetricsLedgerId AND TenantId = @TenantId
        ORDER BY ReadingAtUtc;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AdvancedMetricsLedgerId=', @AdvancedMetricsLedgerId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
