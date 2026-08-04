-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AdvancedMetricsLedger_ListByTenant.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Lists all non-deleted AdvancedMetricsLedger header rows for a tenant. Backs
--                  IAdvancedMetricsLedgerRepository.ListAsync -- specification filtering is
--                  applied in-memory by the repository over this candidate set, matching the
--                  Specifications pattern's ToPredicate() usage elsewhere in the codebase.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_AdvancedMetricsLedger_ListByTenant]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            AdvancedMetricsLedgerId, TenantId, SiteId, ReportingPeriodStartUtc, ReportingPeriodEndUtc,
            Status, CreatedAtUtc, CreatedBy
        FROM [SustainabilityAdvancedMetrics].[AdvancedMetricsLedger]
        WHERE TenantId = @TenantId AND IsDeleted = 0
        ORDER BY ReportingPeriodStartUtc DESC;
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
