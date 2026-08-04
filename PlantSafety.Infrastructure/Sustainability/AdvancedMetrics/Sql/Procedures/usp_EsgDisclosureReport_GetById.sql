-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Sustainability.AdvancedMetrics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EsgDisclosureReport_GetById.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single EsgDisclosureReport row by identity, tenant-scoped. Backs
--                  both IEsgDisclosureReportRepository.GetByIdAsync (write-side aggregate
--                  hydration) and GetEsgDisclosureReportByIdQuery (read-side).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [SustainabilityAdvancedMetrics].[usp_EsgDisclosureReport_GetById]
    @EsgDisclosureReportId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            EsgDisclosureReportId, TenantId, SiteId, SourceLedgerId, Frameworks,
            ReportingPeriodStartUtc, ReportingPeriodEndUtc, Version, SupersedesReportId,
            MethodologyVersion, Status, GeneratedAtUtc, GeneratedBy, PublishedAtUtc
        FROM [SustainabilityAdvancedMetrics].[EsgDisclosureReport]
        WHERE EsgDisclosureReportId = @EsgDisclosureReportId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EsgDisclosureReportId=', @EsgDisclosureReportId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
