-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EnergyIntensityMetric_Insert.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Inserts a new EnergyIntensityMetric child row.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_EnergyIntensityMetric_Insert]
    @EnergyIntensityMetricId UNIQUEIDENTIFIER,
    @AdvancedMetricsLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @EnergyConsumedKwh DECIMAL(18,6),
    @OutputUnits DECIMAL(18,6),
    @IntensityValue DECIMAL(18,6),
    @MethodologyVersion NVARCHAR(100),
    @ReadingAtUtc DATETIME2(3),
    @RecordedAtUtc DATETIME2(3),
    @RecordedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        -- Idempotent by design: see usp_CarbonEmissionRecord_Insert remarks.
        IF NOT EXISTS (SELECT 1 FROM [SustainabilityAdvancedMetrics].[EnergyIntensityMetric] WHERE EnergyIntensityMetricId = @EnergyIntensityMetricId)
        INSERT INTO [SustainabilityAdvancedMetrics].[EnergyIntensityMetric]
        (EnergyIntensityMetricId, AdvancedMetricsLedgerId, TenantId, EnergyConsumedKwh, OutputUnits, IntensityValue, MethodologyVersion, ReadingAtUtc, RecordedAtUtc, RecordedBy)
        VALUES
        (@EnergyIntensityMetricId, @AdvancedMetricsLedgerId, @TenantId, @EnergyConsumedKwh, @OutputUnits, @IntensityValue, @MethodologyVersion, @ReadingAtUtc, @RecordedAtUtc, @RecordedBy);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnergyIntensityMetricId=', @EnergyIntensityMetricId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
