-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards.Dashboards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OshaDashboard_GetTenantExposure.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Per-module coverage counts across an entire tenant -- same as
--                  usp_OshaDashboard_GetSiteExposure but with the SiteId filter removed
--                  entirely (any AuditFinding for the tenant, any site, counts as coverage).
--                  Backs IOshaAuditGapRepository.GetTenantExposureAsync. Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_OshaDashboard_GetTenantExposure]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        ;WITH Requirements AS (
            SELECT r.OSHARequirementId, r.AppliesToModule AS Module, r.Severity,
                   CASE WHEN EXISTS (
                       SELECT 1
                       FROM [Auditing].[AuditFindingOshaCoverage] c
                       JOIN [Auditing].[AuditFinding] f ON f.AuditFindingId = c.AuditFindingId
                       WHERE c.OSHARequirementId = r.OSHARequirementId
                         AND f.TenantId = @TenantId
                         AND f.IsDeleted = 0
                   ) THEN 1 ELSE 0 END AS IsCovered
            FROM [Compliance].[OSHARequirement] r
            WHERE r.AppliesToModule IS NOT NULL
        )
        SELECT
            Module,
            COUNT(*) AS TotalRequirements,
            SUM(IsCovered) AS CoveredCount,
            SUM(1 - IsCovered) AS GapCount,
            SUM(CASE WHEN IsCovered = 0 AND Severity = 'High' THEN 1 ELSE 0 END) AS HighGapCount,
            SUM(CASE WHEN IsCovered = 0 AND Severity = 'Critical' THEN 1 ELSE 0 END) AS CriticalGapCount
        FROM Requirements
        GROUP BY Module
        ORDER BY Module;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
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
