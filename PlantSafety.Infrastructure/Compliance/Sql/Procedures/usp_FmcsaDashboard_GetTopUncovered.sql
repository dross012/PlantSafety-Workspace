-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards.Dashboards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FmcsaDashboard_GetTopUncovered.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    The @Take highest-severity uncovered requirements across every module at a
--                  site -- same coverage predicate as usp_FmcsaAuditGaps_GetForSite, without the
--                  @ModuleName filter, ordered by severity and capped. @SiteId NULL means "any
--                  site in the tenant". Backs IFmcsaAuditGapRepository.GetTopUncoveredForSiteAsync.
--                  Mirrors usp_OshaDashboard_GetTopUncovered.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_FmcsaDashboard_GetTopUncovered]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @Take     INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT TOP (@Take)
               r.RequirementId, r.RequirementNumber, r.Severity, r.RequirementText,
               s.PartNumber, s.SectionNumber, s.SectionTitle
        FROM [Compliance].[FMCSARequirement] r
        JOIN [Compliance].[FMCSAStandard] s ON s.StandardId = r.StandardId
        WHERE r.AppliesToModule IS NOT NULL
          AND NOT EXISTS (
              SELECT 1
              FROM [Auditing].[AuditFindingFmcsaCoverage] c
              JOIN [Auditing].[AuditFinding] f ON f.AuditFindingId = c.AuditFindingId
              WHERE c.RequirementId = r.RequirementId
                AND f.TenantId = @TenantId
                AND f.IsDeleted = 0
                AND (@SiteId IS NULL OR f.SiteId = @SiteId OR f.SiteId IS NULL)
          )
        ORDER BY
            CASE r.Severity WHEN 'Critical' THEN 0 WHEN 'High' THEN 1 WHEN 'Medium' THEN 2 WHEN 'Low' THEN 3 ELSE 4 END,
            s.PartNumber, s.SectionNumber;
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
