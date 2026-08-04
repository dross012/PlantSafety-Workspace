-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AdvancedMetrics_GetWasteDiversionRate.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Computes the waste diversion rate rollup for a ledger. Backs
--                  GetWasteDiversionRateQuery / the Waste Diversion Rate dashboard.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_AdvancedMetrics_GetWasteDiversionRate]
    @AdvancedMetricsLedgerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            @AdvancedMetricsLedgerId AS AdvancedMetricsLedgerId,
            ISNULL(SUM(TotalWasteKg), 0) AS TotalWasteKg,
            ISNULL(SUM(DivertedWasteKg), 0) AS DivertedWasteKg,
            CASE WHEN ISNULL(SUM(TotalWasteKg), 0) = 0 THEN 0
                 ELSE ROUND(SUM(DivertedWasteKg) / SUM(TotalWasteKg) * 100.0, 2) END AS DiversionRatePercent,
            COUNT(*) AS RecordCount
        FROM [SustainabilityAdvancedMetrics].[WasteDiversionRecord]
        WHERE AdvancedMetricsLedgerId = @AdvancedMetricsLedgerId AND TenantId = @TenantId;
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
