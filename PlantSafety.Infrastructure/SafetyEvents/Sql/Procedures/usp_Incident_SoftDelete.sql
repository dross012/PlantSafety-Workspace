-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Incident_SoftDelete.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Soft-deletes an [SafetyEvents].[Incident] row (IsDeleted = 1, Status =
--                  'Archived') and records the deletion reason/timestamp. Backs
--                  IIncidentRepository.SoftDeleteAsync / IncidentDapperRepository.
--                  SoftDeleteAsync. Optimistic concurrency is enforced the same way as
--                  usp_Incident_Update: @ETag is matched against CONVERT(NVARCHAR(34),
--                  RowVersion, 1) in the WHERE clause, so a stale ETag, wrong tenant, missing
--                  row, or already-deleted row all produce zero rows affected, which
--                  IncidentDapperRepository.SoftDeleteAsync surfaces as
--                  ConcurrencyConflictException. Row is never physically removed -- downstream
--                  reads must filter IsDeleted = 0, per this codebase's soft-delete convention
--                  (see AccidentManagement.Accident / RiskMgmt.Risk). TRY/CATCH with
--                  AuditDB/ErrorDB logging per the pattern already established by
--                  usp_Accident_Update. Additive-only evolution.
--  UPDATED:        2026-07-27 -- same @@ROWCOUNT-suppressed-by-NOCOUNT fix as usp_Incident_Update
--                  -- see that file's matching remark for the full explanation. @RowsAffected is
--                  captured before the audit INSERT and returned via SELECT;
--                  IncidentDapperRepository.SoftDeleteAsync now reads it via
--                  DapperCommandExecutor.ExecuteScalarAsync instead of ExecuteAsync.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Incident_SoftDelete]
    @TenantId       UNIQUEIDENTIFIER,
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2(3),
    @IncidentId     UNIQUEIDENTIFIER,
    @DeletionReason NVARCHAR(1000),
    @ETag           NVARCHAR(64)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [SafetyEvents].[Incident]
        SET
            IsDeleted = 1,
            Status = N'Archived',
            DeletionReason = @DeletionReason,
            DeletedAtUtc = @NowUtc,
            LastModifiedByActorId = @ActorId,
            LastModifiedAtUtc = @NowUtc
        WHERE IncidentId = @IncidentId
          AND TenantId = @TenantId
          AND IsDeleted = 0
          AND CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag;

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'Incident', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentId=', @IncidentId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
