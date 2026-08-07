-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards.Dashboards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_NfpaDashboard_GetSiteExposure.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Per-module coverage counts at one site -- same coverage logic as
--                  usp_NfpaAuditGaps_GetSummaryForSite, generalized across every mapped module.
--                  Backs INfpaAuditGapRepository.GetSiteExposureAsync. Mirrors
--                  usp_OshaDashboard_GetSiteExposure. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_NfpaDashboard_GetSiteExposure]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        ;WITH Mappings AS (
            SELECT mm.MappingId, mm.ModuleName AS Module, mm.HazardId, mm.Severity,
                   CASE WHEN EXISTS (
                       SELECT 1
                       FROM [Auditing].[AuditFindingNfpaCoverage] c
                       JOIN [Auditing].[AuditFinding] f ON f.AuditFindingId = c.AuditFindingId
                       WHERE c.HazardId = mm.HazardId
                         AND f.TenantId = @TenantId
                         AND f.IsDeleted = 0
                         AND (f.SiteId = @SiteId OR f.SiteId IS NULL)
                   ) THEN 1 ELSE 0 END AS IsCovered
            FROM [Compliance].[NFPA_HazardModuleMapping] mm
        )
        SELECT
            Module,
            COUNT(*) AS TotalHazards,
            SUM(IsCovered) AS CoveredCount,
            SUM(1 - IsCovered) AS GapCount,
            SUM(CASE WHEN IsCovered = 0 AND Severity = 'High' THEN 1 ELSE 0 END) AS HighGapCount,
            SUM(CASE WHEN IsCovered = 0 AND Severity = 'Critical' THEN 1 ELSE 0 END) AS CriticalGapCount
        FROM Mappings
        GROUP BY Module
        ORDER BY Module;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
