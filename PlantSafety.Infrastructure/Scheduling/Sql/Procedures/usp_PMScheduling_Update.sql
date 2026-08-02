-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PMScheduling_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a PMScheduling aggregate root row.
--                  Backs IPMSchedulingRepository.UpdateAsync. Additive-only evolution.
--  UPDATED:        2026-07-30 (Phase 4 Scheduling.PMScheduling remediation) -- added @TenantId
--                  (cross-tenant access remediation): the UPDATE's WHERE clause now also requires
--                  TenantId = @TenantId, so a PMScheduling record loaded and mutated in-process
--                  cannot be persisted against a different tenant's row even in principle. The
--                  caller (PMSchedulingDapperRepository.UpdateAsync) binds this from the
--                  aggregate's own TenantId, which is only ever set via a tenant-checked
--                  GetByIdAsync. Also added @ETag optimistic-concurrency guard against the native
--                  RowVersion column. @RowsAffected is captured via @@ROWCOUNT immediately after
--                  the UPDATE (before the audit INSERT, which would otherwise reset it) and
--                  returned via SELECT -- callers MUST invoke via ExecuteScalarAsync (not
--                  ExecuteNonQuery), since SET NOCOUNT ON makes ExecuteNonQuery always return -1,
--                  silently disabling the concurrency check. A stale/wrong @ETag or a
--                  missing/deleted/cross-tenant row both naturally produce zero rows affected. The
--                  pre-existing IF NOT EXISTS/THROW 50004 existence check is intentionally left
--                  PMSchedulingId/IsDeleted-only (not tenant-scoped) so a genuinely-missing row
--                  still raises the specific "not found" error rather than being folded into the
--                  rows-affected-based concurrency-conflict path.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_PMScheduling_Update]
    @PMSchedulingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @EquipmentId UNIQUEIDENTIFIER,
    @EquipmentDescription NVARCHAR(400),
    @PmTaskType NVARCHAR(400),
    @DueAtUtc DATETIME2(3) = NULL,
    @IsOverdue BIT,
    @EscalationPriorityCode NVARCHAR(400) = NULL,
    @CompletionNotes NVARCHAR(400) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Scheduling].[PMScheduling] WHERE PMSchedulingId = @PMSchedulingId AND IsDeleted = 0)
            THROW 50004, N'PMScheduling not found or has been deleted.', 1;

        UPDATE [Scheduling].[PMScheduling]
        SET
            Status = @Status,
            EquipmentId = @EquipmentId,
            EquipmentDescription = @EquipmentDescription,
            PmTaskType = @PmTaskType,
            DueAtUtc = @DueAtUtc,
            IsOverdue = @IsOverdue,
            EscalationPriorityCode = @EscalationPriorityCode,
            CompletionNotes = @CompletionNotes,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME(),
            ConcurrencyToken = NEWID()
        WHERE PMSchedulingId = @PMSchedulingId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PMScheduling', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'PMSchedulingId=', @PMSchedulingId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PMSchedulingId=', @PMSchedulingId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
