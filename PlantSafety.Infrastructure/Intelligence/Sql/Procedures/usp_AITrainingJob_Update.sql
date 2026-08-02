-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.AITraining
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_AITrainingJob_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of an AITrainingJob aggregate root row.
--                  Backs IAITrainingJobRepository.UpdateAsync (Domain write-side repository).
--                  @TenantId is part of the WHERE clause from day one (tenant-scoped write) and
--                  @ETag enforces optimistic concurrency against the native RowVersion column from
--                  day one, mirroring usp_RCA_Update.sql's sibling proc. @RowsAffected is captured
--                  via @@ROWCOUNT immediately after the UPDATE (before the audit INSERT, which
--                  would otherwise reset it) and returned via SELECT -- callers MUST invoke this
--                  proc via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT
--                  ON makes ExecuteNonQuery always return -1, which would silently disable the
--                  @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Intelligence].[usp_AITrainingJob_Update]
    @AITrainingJobId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ModelName NVARCHAR(200),
    @ModelVersion NVARCHAR(50),
    @DatasetReference NVARCHAR(400) = NULL,
    @Status NVARCHAR(50),
    @RunStartedBy UNIQUEIDENTIFIER = NULL,
    @RunStartedAtUtc DATETIME2(3) = NULL,
    @ModelArtifactReference NVARCHAR(400) = NULL,
    @CompletedAtUtc DATETIME2(3) = NULL,
    @FailureReason NVARCHAR(MAX) = NULL,
    @FailedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @ModifiedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Intelligence].[AITrainingJob] WHERE AITrainingJobId = @AITrainingJobId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'AITrainingJob not found or has been deleted.', 1;

        UPDATE [Intelligence].[AITrainingJob]
        SET
            ModelName = @ModelName,
            ModelVersion = @ModelVersion,
            DatasetReference = @DatasetReference,
            Status = @Status,
            RunStartedBy = @RunStartedBy,
            RunStartedAtUtc = @RunStartedAtUtc,
            ModelArtifactReference = @ModelArtifactReference,
            CompletedAtUtc = @CompletedAtUtc,
            FailureReason = @FailureReason,
            FailedAtUtc = @FailedAtUtc,
            ArchivedAtUtc = @ArchivedAtUtc,
            ModifiedBy = @ModifiedBy,
            ModifiedAtUtc = SYSUTCDATETIME()
        WHERE AITrainingJobId = @AITrainingJobId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'AITrainingJob', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'AITrainingJobId=', @AITrainingJobId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AITrainingJobId=', @AITrainingJobId, N'; User=', SYSTEM_USER);
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
