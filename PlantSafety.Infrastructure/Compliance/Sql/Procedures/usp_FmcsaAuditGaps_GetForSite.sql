-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.FMCSAStandards.AuditGaps
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_FmcsaAuditGaps_GetForSite.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Lists Compliance.FMCSARequirement rows mapped (via
--                  FMCSARequirementModuleMapping, whose per-module Severity -- not the global,
--                  nullable FMCSARequirement.Severity -- drives ranking) to @ModuleName with no
--                  covering row in
--                  Auditing.AuditFindingFmcsaCoverage for any non-deleted AuditFinding at
--                  @SiteId. A finding with NULL SiteId is treated as tenant-wide and covers every
--                  site. @SiteId NULL means "any site in the tenant". @ModuleName NULL means
--                  "every mapped module". Backs IFmcsaAuditGapRepository.GetGapsForSiteAsync.
--                  Cross-schema join (Compliance <-> Auditing) is intentional -- both live in the
--                  same PlantSafety.CoreDB database. Mirrors usp_OshaAuditGaps_GetForSite.sql.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_FmcsaAuditGaps_GetForSite]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @ModuleName NVARCHAR(100)    = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT r.RequirementId, r.RequirementNumber, m.Severity, r.RequirementText,
               m.ModuleName,
               s.PartNumber, s.SectionNumber, s.SectionTitle
        FROM [Compliance].[FMCSARequirement] r
        JOIN [Compliance].[FMCSAStandard] s ON s.StandardId = r.StandardId
        JOIN [Compliance].[FMCSARequirementModuleMapping] m ON m.RequirementId = r.RequirementId
        WHERE (@ModuleName IS NULL OR m.ModuleName = @ModuleName)
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
            CASE m.Severity WHEN 'Critical' THEN 0 WHEN 'High' THEN 1 WHEN 'Medium' THEN 2 WHEN 'Low' THEN 3 ELSE 4 END,
            s.PartNumber, s.SectionNumber;
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
