-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards.Dashboards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_NfpaDashboard_GetTopUncovered.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    The @Take highest-severity uncovered hazard mappings across every module at
--                  a site -- same coverage predicate as usp_NfpaAuditGaps_GetForSite, without
--                  the @ModuleName filter, ordered by severity and capped. Backs
--                  INfpaAuditGapRepository.GetTopUncoveredForSiteAsync. Mirrors
--                  usp_OshaDashboard_GetTopUncovered. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_NfpaDashboard_GetTopUncovered]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @Take     INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT TOP (@Take)
               mm.MappingId, mm.ModuleName, mm.Severity, mm.Rationale,
               h.HazardId, h.CodeNumber, h.SectionNumber, h.HazardName
        FROM [Compliance].[NFPA_HazardModuleMapping] mm
        JOIN [Compliance].[NFPA_HazardProfile] h ON h.HazardId = mm.HazardId
        WHERE NOT EXISTS (
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
