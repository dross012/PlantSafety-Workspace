-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AdvancedMetrics_GetCarbonFootprintByPeriod.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Computes the Scope 1/2/3 carbon footprint rollup, plus carbon intensity per
--                  unit output, for the ledger covering the given tenant/site/reporting period.
--                  Backs GetCarbonFootprintByPeriodQuery / the Carbon Footprint Trend dashboard.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_AdvancedMetrics_GetCarbonFootprintByPeriod]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @PeriodStartUtc DATETIME2(3),
    @PeriodEndUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        DECLARE @LedgerId UNIQUEIDENTIFIER;

        SELECT TOP (1) @LedgerId = AdvancedMetricsLedgerId
        FROM [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger]
        WHERE TenantId = @TenantId
          AND ((@SiteId IS NULL AND SiteId IS NULL) OR SiteId = @SiteId)
          AND ReportingPeriodStartUtc = @PeriodStartUtc
          AND ReportingPeriodEndUtc = @PeriodEndUtc
          AND IsDeleted = 0;

        SELECT
            @LedgerId AS AdvancedMetricsLedgerId,
            @PeriodStartUtc AS ReportingPeriodStartUtc,
            @PeriodEndUtc AS ReportingPeriodEndUtc,
            ISNULL(SUM(CASE WHEN c.Scope = N'Scope1' THEN c.EmissionsMetricTonsCo2e END), 0) AS Scope1MetricTonsCo2e,
            ISNULL(SUM(CASE WHEN c.Scope = N'Scope2' THEN c.EmissionsMetricTonsCo2e END), 0) AS Scope2MetricTonsCo2e,
            ISNULL(SUM(CASE WHEN c.Scope = N'Scope3' THEN c.EmissionsMetricTonsCo2e END), 0) AS Scope3MetricTonsCo2e,
            ISNULL(SUM(c.EmissionsMetricTonsCo2e), 0) AS TotalMetricTonsCo2e,
            CASE WHEN ISNULL(o.TotalOutputUnits, 0) > 0
                 THEN ROUND(ISNULL(SUM(c.EmissionsMetricTonsCo2e), 0) / o.TotalOutputUnits, 6)
                 ELSE NULL END AS CarbonIntensityPerUnitOutput,
            COUNT(c.CarbonEmissionRecordId) AS RecordCount
        FROM [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger] AS l
        LEFT JOIN [SustainabilityAdvancedMetrics].[CarbonEmissionRecord] AS c ON c.AdvancedMetricsLedgerId = l.AdvancedMetricsLedgerId
        OUTER APPLY (
            SELECT SUM(e.OutputUnits) AS TotalOutputUnits
            FROM [SustainabilityAdvancedMetrics].[EnergyIntensityMetric] AS e
            WHERE e.AdvancedMetricsLedgerId = l.AdvancedMetricsLedgerId
        ) AS o
        WHERE l.AdvancedMetricsLedgerId = @LedgerId
        GROUP BY o.TotalOutputUnits;
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
