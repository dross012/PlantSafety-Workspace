-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContinuousImprovement.RCA
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RCA_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of an RCA aggregate root row. Backs
--                  IRCARepository.UpdateAsync (Domain write-side repository). @TenantId is part
--                  of the WHERE clause from day one (tenant-scoped write) and @ETag enforces
--                  optimistic concurrency against the native RowVersion column from day one,
--                  mirroring usp_RootCauseAnalysis_Update.sql / usp_CorrectiveAction_GetById.sql's
--                  sibling procs. @RowsAffected is captured via @@ROWCOUNT immediately after the
--                  UPDATE (before the audit INSERT, which would otherwise reset it) and returned
--                  via SELECT -- callers MUST invoke this proc via ExecuteScalarAsync (not
--                  ExecuteNonQuery/ExecuteAsync), since SET NOCOUNT ON makes ExecuteNonQuery
--                  always return -1, which would silently disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_RCA_Update]
    @RcaId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ProblemStatement NVARCHAR(MAX),
    @Methodology NVARCHAR(50),
    @Status NVARCHAR(50),
    @StartedBy UNIQUEIDENTIFIER = NULL,
    @StartedAtUtc DATETIME2(3) = NULL,
    @SubmittedBy UNIQUEIDENTIFIER = NULL,
    @Findings NVARCHAR(MAX) = NULL,
    @SubmittedAtUtc DATETIME2(3) = NULL,
    @ClosedBy UNIQUEIDENTIFIER = NULL,
    @RootCauseSummary NVARCHAR(MAX) = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @ModifiedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [ContinuousImprovement].[RCA] WHERE RcaId = @RcaId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'RCA not found or has been deleted.', 1;

        UPDATE [ContinuousImprovement].[RCA]
        SET
            ProblemStatement = @ProblemStatement,
            Methodology = @Methodology,
            Status = @Status,
            StartedBy = @StartedBy,
            StartedAtUtc = @StartedAtUtc,
            SubmittedBy = @SubmittedBy,
            Findings = @Findings,
            SubmittedAtUtc = @SubmittedAtUtc,
            ClosedBy = @ClosedBy,
            RootCauseSummary = @RootCauseSummary,
            ClosedAtUtc = @ClosedAtUtc,
            ArchivedAtUtc = @ArchivedAtUtc,
            ModifiedBy = @ModifiedBy,
            ModifiedAtUtc = SYSUTCDATETIME()
        WHERE RcaId = @RcaId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'RCA', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'RcaId=', @RcaId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RcaId=', @RcaId, N'; User=', SYSTEM_USER);
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
