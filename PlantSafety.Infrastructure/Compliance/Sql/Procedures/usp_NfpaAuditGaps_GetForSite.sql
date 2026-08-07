-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards.AuditGaps
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_NfpaAuditGaps_GetForSite.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Lists Compliance.NFPA_HazardModuleMapping rows (optionally filtered to
--                  @ModuleName) with no covering row in Auditing.AuditFindingNfpaCoverage for
--                  any non-deleted AuditFinding at @SiteId. A finding with NULL SiteId is
--                  treated as tenant-wide and covers every site; a NULL @SiteId means "any
--                  site" (no site restriction). Coverage is tracked per HazardId (not per
--                  module mapping) because Auditing.AuditFindingNfpaCoverage has no Module
--                  column. Backs INfpaAuditGapRepository.GetGapsForSiteAsync. Cross-schema
--                  join (Compliance <-> Auditing) is intentional -- both live in the same
--                  PlantSafety.CoreDB database. Mirrors usp_OshaAuditGaps_GetForSite.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_NfpaAuditGaps_GetForSite]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @ModuleName NVARCHAR(100)    = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT mm.MappingId, mm.ModuleName, mm.MappingRuleCode, mm.Severity, mm.Rationale,
               h.HazardId, h.CodeNumber, h.SectionNumber, h.HazardName
        FROM [Compliance].[NFPA_HazardModuleMapping] mm
        JOIN [Compliance].[NFPA_HazardProfile] h ON h.HazardId = mm.HazardId
        WHERE (@ModuleName IS NULL OR mm.ModuleName = @ModuleName)
          AND NOT EXISTS (
              SELECT 1
              FROM [Auditing].[AuditFindingNfpaCoverage] c
              JOIN [Auditing].[AuditFinding] f ON f.AuditFindingId = c.AuditFindingId
              WHERE c.HazardId = mm.HazardId
                AND f.TenantId = @TenantId
                AND f.IsDeleted = 0
                AND (@SiteId IS NULL OR f.SiteId = @SiteId OR f.SiteId IS NULL)
          )
        ORDER BY
            CASE mm.Severity WHEN 'Critical' THEN 0 WHEN 'High' THEN 1 WHEN 'Medium' THEN 2 WHEN 'Low' THEN 3 ELSE 4 END,
            h.CodeNumber, h.SectionNumber;
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
