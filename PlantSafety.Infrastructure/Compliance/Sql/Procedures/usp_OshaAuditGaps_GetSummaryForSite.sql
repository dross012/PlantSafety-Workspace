-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards.AuditGaps
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OshaAuditGaps_GetSummaryForSite.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Aggregate counterpart to usp_OshaAuditGaps_GetForSite -- same coverage logic,
--                  collapsed to counts for a dashboard tile. Backs
--                  IOshaAuditGapRepository.GetGapsSummaryForSiteAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_OshaAuditGaps_GetSummaryForSite]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER,
    @Module   NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        ;WITH Requirements AS (
            SELECT r.OSHARequirementId, r.Severity,
                   CASE WHEN EXISTS (
                       SELECT 1
                       FROM [Auditing].[AuditFindingOshaCoverage] c
                       JOIN [Auditing].[AuditFinding] f ON f.AuditFindingId = c.AuditFindingId
                       WHERE c.OSHARequirementId = r.OSHARequirementId
                         AND f.TenantId = @TenantId
                         AND f.IsDeleted = 0
                         AND (f.SiteId = @SiteId OR f.SiteId IS NULL)
                   ) THEN 1 ELSE 0 END AS IsCovered
            FROM [Compliance].[OSHARequirement] r
            WHERE r.AppliesToModule = @Module
        )
        SELECT
            @Module AS Module,
            COUNT(*) AS TotalRequirements,
            SUM(IsCovered) AS CoveredCount,
            SUM(1 - IsCovered) AS GapCount,
            SUM(CASE WHEN IsCovered = 0 AND Severity = 'High' THEN 1 ELSE 0 END) AS HighGapCount,
            SUM(CASE WHEN IsCovered = 0 AND Severity = 'Critical' THEN 1 ELSE 0 END) AS CriticalGapCount
        FROM Requirements;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; Module=', @Module, N'; User=', SYSTEM_USER);
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
