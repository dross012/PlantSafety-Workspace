-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         WorkerWellBeing
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkerWellBeingAggregate_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a WorkerWellBeing aggregate root row.
--                  Backs IWorkerWellBeingRepository.UpdateAsync. @TenantId is part of the WHERE
--                  clause and @ETag enforces optimistic concurrency against the native RowVersion
--                  column from day one, mirroring usp_AuditingAggregate_Update.sql.
--                  @RowsAffected is captured via @@ROWCOUNT immediately after the UPDATE (before
--                  the audit INSERT) and returned via SELECT -- callers MUST invoke this proc via
--                  ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON
--                  makes ExecuteNonQuery always return -1, which would silently disable the
--                  @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [WorkerWellBeing].[usp_WorkerWellBeingAggregate_Update]
    @WorkerWellBeingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @WellBeingRiskLevel NVARCHAR(50),
    @CheckInType NVARCHAR(100),
    @WellBeingScore INT,
    @FollowUpRequired BIT,
    @Status NVARCHAR(50),
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [WorkerWellBeing].[WorkerWellBeingAggregate] WHERE WorkerWellBeingId = @WorkerWellBeingId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'WorkerWellBeing record not found or has been deleted.', 1;

        UPDATE [WorkerWellBeing].[WorkerWellBeingAggregate]
        SET
            WellBeingRiskLevel = @WellBeingRiskLevel,
            CheckInType = @CheckInType,
            WellBeingScore = @WellBeingScore,
            FollowUpRequired = @FollowUpRequired,
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE WorkerWellBeingId = @WorkerWellBeingId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'WorkerWellBeingAggregate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId, N'; User=', SYSTEM_USER);
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
