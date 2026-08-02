-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Risk_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Updates all mutable fields of a Risk aggregate root row. Backs
--                  IRiskRepository.UpdateAsync. HazardDomainCategory is intentionally not
--                  updated here -- it is an immutable Infrastructure-layer classification tag
--                  assigned once at creation. Additive-only evolution.
--  UPDATED:        2026-07-29 -- Added @ETag optimistic-concurrency guard against the native
--                  RowVersion column (Phase 4 RiskManagement concurrency remediation; mirrors
--                  usp_PermitWorkflow_Update / usp_Incident_Update). @RowsAffected is captured via
--                  @@ROWCOUNT immediately after the UPDATE (before the audit INSERT, which would
--                  otherwise reset it) and returned via SELECT in place of the previous ETag-only
--                  SELECT -- callers MUST invoke via ExecuteScalarAsync (not ExecuteNonQuery), since
--                  SET NOCOUNT ON makes ExecuteNonQuery always return -1, silently disabling the
--                  concurrency check. A stale/wrong @ETag or a missing/deleted row both naturally
--                  produce zero rows affected. The pre-existing IF NOT EXISTS/THROW 50004 existence
--                  check is unchanged and still fires first for a genuinely missing/deleted row.
--  UPDATED:        2026-07-29 -- Added @TenantId (Phase 4 cross-tenant access remediation): the
--                  UPDATE's WHERE clause now also requires TenantId = @TenantId, so a Risk loaded
--                  and mutated in-process cannot be persisted against a different tenant's row even
--                  in principle. The caller (RiskDapperRepository.UpdateAsync) binds this from the
--                  aggregate's own TenantId, which is only ever set via a tenant-checked
--                  GetByIdAsync -- mirrors IIncidentRepository.UpdateAsync's identical approach of
--                  deriving TenantId from the aggregate rather than a separate repository
--                  parameter. The pre-existing IF NOT EXISTS/THROW 50004 check is intentionally
--                  left RiskId/IsDeleted-only (not tenant-scoped) so a genuinely-missing row still
--                  raises the specific "not found" error rather than being folded into the
--                  rows-affected-based concurrency-conflict path.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risk_Update]
    @RiskId              UNIQUEIDENTIFIER,
    @TenantId            UNIQUEIDENTIFIER,
    @Status              NVARCHAR(30),
    @Category            NVARCHAR(30),
    @CurrentSeverity     INT = NULL,
    @CurrentLikelihood   INT = NULL,
    @Priority            NVARCHAR(10) = NULL,
    @DocumentedAtUtc     DATETIME2(3) = NULL,
    @ResidualSeverity    INT = NULL,
    @ResidualLikelihood  INT = NULL,
    @LastValidatedAtUtc  DATETIME2(3) = NULL,
    @ReviewCadence       NVARCHAR(30),
    @LastReviewedAtUtc   DATETIME2(3) = NULL,
    @NextReviewDueUtc    DATETIME2(3) = NULL,
    @ReviewCount         INT,
    @RiskVersion         INT,
    @UpdatedBy           UNIQUEIDENTIFIER = NULL,
    @ETag                NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [RiskMgmt].[Risk] WHERE RiskId = @RiskId AND IsDeleted = 0)
            THROW 50004, N'Risk not found or has been deleted.', 1;

        UPDATE [RiskMgmt].[Risk]
        SET
            Status = @Status,
            Category = @Category,
            CurrentSeverity = @CurrentSeverity,
            CurrentLikelihood = @CurrentLikelihood,
            Priority = @Priority,
            DocumentedAtUtc = @DocumentedAtUtc,
            ResidualSeverity = @ResidualSeverity,
            ResidualLikelihood = @ResidualLikelihood,
            LastValidatedAtUtc = @LastValidatedAtUtc,
            ReviewCadence = @ReviewCadence,
            LastReviewedAtUtc = @LastReviewedAtUtc,
            NextReviewDueUtc = @NextReviewDueUtc,
            ReviewCount = @ReviewCount,
            RiskVersion = @RiskVersion,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE RiskId = @RiskId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Risk', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RiskId=', @RiskId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskId=', @RiskId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
