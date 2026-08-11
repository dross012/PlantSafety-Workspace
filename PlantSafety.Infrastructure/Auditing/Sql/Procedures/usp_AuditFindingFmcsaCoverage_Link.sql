-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding.FmcsaCoverage
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AuditFindingFmcsaCoverage_Link.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Links one AuditFinding to one FMCSARequirement, scoped to TenantId (no-ops if
--                  the finding doesn't belong to the tenant) and idempotent (no-ops if the pair
--                  is already linked). Called once per (AuditFindingId, RequirementId) pair by
--                  IAuditFindingFmcsaCoverageRepository.LinkAsync. Mirrors
--                  usp_AuditFindingOshaCoverage_Link.sql. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Auditing].[usp_AuditFindingFmcsaCoverage_Link]
    @AuditFindingFmcsaCoverageId UNIQUEIDENTIFIER,
    @TenantId                    UNIQUEIDENTIFIER,
    @AuditFindingId               UNIQUEIDENTIFIER,
    @RequirementId                UNIQUEIDENTIFIER,
    @CreatedBy                    UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM [Auditing].[AuditFinding] WHERE AuditFindingId = @AuditFindingId AND TenantId = @TenantId AND IsDeleted = 0)
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM [Auditing].[AuditFindingFmcsaCoverage]
                WHERE AuditFindingId = @AuditFindingId AND RequirementId = @RequirementId)
            BEGIN
                INSERT INTO [Auditing].[AuditFindingFmcsaCoverage] (AuditFindingFmcsaCoverageId, AuditFindingId, RequirementId, CreatedBy)
                VALUES (@AuditFindingFmcsaCoverageId, @AuditFindingId, @RequirementId, @CreatedBy);
            END
        END
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AuditFindingFmcsaCoverage', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AuditFindingFmcsaCoverageId=', @AuditFindingFmcsaCoverageId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditFindingId=', @AuditFindingId, N'; RequirementId=', @RequirementId, N'; User=', SYSTEM_USER);
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
