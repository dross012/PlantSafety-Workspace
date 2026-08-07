-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards.AuditGaps
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FmcsaAuditGaps_GetSummaryForSite.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Aggregate counterpart to usp_FmcsaAuditGaps_GetForSite -- same coverage logic
--                  and same nullable @SiteId/@ModuleName filters, collapsed to per-module counts
--                  for a dashboard tile. Ranked/bucketed by FMCSARequirementModuleMapping's
--                  per-module Severity, not the global, nullable FMCSARequirement.Severity. Backs
--                  IFmcsaAuditGapRepository.GetGapsSummaryForSiteAsync. Mirrors
--                  usp_OshaAuditGaps_GetSummaryForSite.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_FmcsaAuditGaps_GetSummaryForSite]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @ModuleName NVARCHAR(100)    = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        ;WITH Requirements AS (
            SELECT r.RequirementId, m.Severity, m.ModuleName,
                   CASE WHEN EXISTS (
                       SELECT 1
                       FROM [Auditing].[AuditFindingFmcsaCoverage] c
                       JOIN [Auditing].[AuditFinding] f ON f.AuditFindingId = c.AuditFindingId
                       WHERE c.RequirementId = r.RequirementId
                         AND f.TenantId = @TenantId
                         AND f.IsDeleted = 0
                         AND (@SiteId IS NULL OR f.SiteId = @SiteId OR f.SiteId IS NULL)
                   ) THEN 1 ELSE 0 END AS IsCovered
            FROM [Compliance].[FMCSARequirement] r
            JOIN [Compliance].[FMCSARequirementModuleMapping] m ON m.RequirementId = r.RequirementId
            WHERE (@ModuleName IS NULL OR m.ModuleName = @ModuleName)
        )
        SELECT
            ModuleName AS Module,
            COUNT(*) AS TotalRequirements,
            SUM(IsCovered) AS CoveredCount,
            SUM(1 - IsCovered) AS GapCount,
            SUM(CASE WHEN IsCovered = 0 AND Severity = 'High' THEN 1 ELSE 0 END) AS HighGapCount,
            SUM(CASE WHEN IsCovered = 0 AND Severity = 'Critical' THEN 1 ELSE 0 END) AS CriticalGapCount
        FROM Requirements
        GROUP BY ModuleName
        ORDER BY ModuleName;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; ModuleName=', @ModuleName, N'; User=', SYSTEM_USER);
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
