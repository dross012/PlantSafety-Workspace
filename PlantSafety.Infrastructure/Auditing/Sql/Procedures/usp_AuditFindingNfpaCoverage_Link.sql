-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding.NfpaCoverage
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AuditFindingNfpaCoverage_Link.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Links one AuditFinding to one Compliance.NFPA_HazardProfile hazard, scoped
--                  to TenantId (no-ops if the finding doesn't belong to the tenant) and
--                  idempotent (no-ops if the pair is already linked). Called once per
--                  (AuditFindingId, HazardId) pair by
--                  IAuditFindingNfpaCoverageRepository.LinkAsync. Mirrors
--                  usp_AuditFindingOshaCoverage_Link -- one hazard per call, no table-valued
--                  parameter. @CreatedBy is NVARCHAR(200) NULL (not UNIQUEIDENTIFIER NOT NULL
--                  like the OSHA counterpart) to match
--                  Auditing.AuditFindingNfpaCoverage.CreatedBy's column type. Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditFindingNfpaCoverage_Link]
    @AuditFindingNfpaCoverageId UNIQUEIDENTIFIER,
    @TenantId                   UNIQUEIDENTIFIER,
    @AuditFindingId              UNIQUEIDENTIFIER,
    @HazardId                    UNIQUEIDENTIFIER,
    @CreatedBy                   NVARCHAR(200) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM [Auditing].[AuditFinding] WHERE AuditFindingId = @AuditFindingId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM [Auditing].[AuditFindingNfpaCoverage]
                WHERE AuditFindingId = @AuditFindingId AND HazardId = @HazardId)
            BEGIN
                INSERT INTO [Auditing].[AuditFindingNfpaCoverage] (AuditFindingNfpaCoverageId, AuditFindingId, HazardId, CreatedBy)
                VALUES (@AuditFindingNfpaCoverageId, @AuditFindingId, @HazardId, @CreatedBy);
            END
        END
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditFindingId=', @AuditFindingId, N'; HazardId=', @HazardId, N'; User=', SYSTEM_USER);
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
