-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards.AuditGaps
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OshaAuditGaps_GetForSite.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Lists Compliance.OSHARequirement rows mapped to @Module with no covering
--                  row in Auditing.AuditFindingOshaCoverage for any non-deleted AuditFinding at
--                  @SiteId. A finding with NULL SiteId is treated as tenant-wide and covers
--                  every site. Backs IOshaAuditGapRepository.GetGapsForSiteAsync. Cross-schema
--                  join (Compliance <-> Auditing) is intentional -- both live in the same
--                  PlantSafety.CoreDB database. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Compliance].[usp_OshaAuditGaps_GetForSite]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER,
    @Module   NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT r.OSHARequirementId, r.RequirementType, r.Severity, r.Notes,
               s.PartNumber, s.Section, s.SectionTitle
        FROM [Compliance].[OSHARequirement] r
        JOIN [Compliance].[OSHAStandard] s ON s.OSHAStandardId = r.OSHAStandardId
        WHERE r.AppliesToModule = @Module
          AND NOT EXISTS (
              SELECT 1
              FROM [Auditing].[AuditFindingOshaCoverage] c
              JOIN [Auditing].[AuditFinding] f ON f.AuditFindingId = c.AuditFindingId
              WHERE c.OSHARequirementId = r.OSHARequirementId
                AND f.TenantId = @TenantId
                AND f.IsDeleted = 0
                AND (f.SiteId = @SiteId OR f.SiteId IS NULL)
          )
        ORDER BY
            CASE r.Severity WHEN 'Critical' THEN 0 WHEN 'High' THEN 1 WHEN 'Medium' THEN 2 WHEN 'Low' THEN 3 ELSE 4 END,
            s.PartNumber, s.Section;
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
