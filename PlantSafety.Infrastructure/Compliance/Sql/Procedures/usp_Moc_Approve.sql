-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Moc_Approve.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Write stored procedure backing IMocRepository.ApproveAsync. ETag-guarded
--                  (ROWVERSION) optimistic-concurrency update; ends with an explicit
--                  SELECT @@ROWCOUNT so the caller can detect a stale/incorrect ETag via
--                  DapperCommandExecutor.ExecuteScalarAsync (SET NOCOUNT ON means
--                  ExecuteNonQuery/ExecuteAsync would otherwise always report -1, per the
--                  platform-wide rowcount-concurrency pattern documented on
--                  DapperCommandExecutor.ExecuteScalarAsync).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_Moc_Approve]
    @TenantId       UNIQUEIDENTIFIER,
    @CorrelationId  UNIQUEIDENTIFIER,
    @ActorId        UNIQUEIDENTIFIER,
    @NowUtc         DATETIME2(3),
    @MocId          UNIQUEIDENTIFIER,
    @ApprovalNotes  NVARCHAR(2000) = NULL,
    @ETag           NVARCHAR(64)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        UPDATE [Compliance].[Moc]
        SET
            Status = N'Approved',
            ApprovalNotes = @ApprovalNotes,
            ApprovedByActorId = @ActorId,
            ApprovedAtUtc = @NowUtc,
            LastModifiedAtUtc = @NowUtc
        WHERE MocId = @MocId
          AND TenantId = @TenantId
          AND IsDeleted = 0
          AND CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag;

        DECLARE @RowsAffected INT = @@ROWCOUNT;

    COMMIT TRANSACTION;

    BEGIN TRY
        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
        VALUES (N'Moc', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'MocId=', @MocId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId), 0, OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    SELECT @RowsAffected;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'MocId=', @MocId, N'; TenantId=', @TenantId, N'; CorrelationId=', @CorrelationId, N'; ActorId=', @ActorId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    BEGIN TRY
        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
    END TRY
    BEGIN CATCH
    END CATCH;

    THROW;
END CATCH;
GO
