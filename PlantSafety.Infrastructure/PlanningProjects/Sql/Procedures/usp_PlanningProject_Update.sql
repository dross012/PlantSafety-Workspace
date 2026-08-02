-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PlanningProjects
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PlanningProject_Update.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Overwrites the full mutable state of a Project aggregate root row, and
--                  re-syncs its milestone child collection (delete-then-reinsert, simplest correct
--                  strategy for a small bounded child collection). Backs
--                  IProjectRepository.UpdateAsync (Domain write-side repository). @TenantId is part
--                  of the WHERE clause from day one (tenant-scoped write) and @ETag enforces
--                  optimistic concurrency against the native RowVersion column from day one,
--                  mirroring usp_RootCauseAnalysis_Update.sql. @RowsAffected is captured via
--                  @@ROWCOUNT immediately after the UPDATE and returned via SELECT -- callers MUST
--                  invoke this proc via ExecuteScalarAsync (not ExecuteNonQuery/ExecuteAsync),
--                  since SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would
--                  silently disable the @ETag concurrency check.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PlanningProjects].[usp_PlanningProject_Update]
    @ProjectId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @ActivatedBy UNIQUEIDENTIFIER = NULL,
    @ActivatedAtUtc DATETIME2(3) = NULL,
    @PutOnHoldBy UNIQUEIDENTIFIER = NULL,
    @PutOnHoldAtUtc DATETIME2(3) = NULL,
    @OnHoldReason NVARCHAR(1000) = NULL,
    @CompletedBy UNIQUEIDENTIFIER = NULL,
    @CompletedAtUtc DATETIME2(3) = NULL,
    @CancelledBy UNIQUEIDENTIFIER = NULL,
    @CancelledAtUtc DATETIME2(3) = NULL,
    @CancellationReason NVARCHAR(1000) = NULL,
    @ArchivedAtUtc DATETIME2(3) = NULL,
    @UpdatedBy UNIQUEIDENTIFIER = NULL,
    @ETag NVARCHAR(64) = NULL,
    @Milestones [PlanningProjects].[PlanningProjectMilestoneTableType] READONLY
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [PlanningProjects].[PlanningProject] WHERE ProjectId = @ProjectId AND TenantId = @TenantId AND IsDeleted = 0)
            THROW 50004, N'PlanningProject not found or has been deleted.', 1;

        UPDATE [PlanningProjects].[PlanningProject]
        SET
            Status = @Status,
            ActivatedBy = @ActivatedBy,
            ActivatedAtUtc = @ActivatedAtUtc,
            PutOnHoldBy = @PutOnHoldBy,
            PutOnHoldAtUtc = @PutOnHoldAtUtc,
            OnHoldReason = @OnHoldReason,
            CompletedBy = @CompletedBy,
            CompletedAtUtc = @CompletedAtUtc,
            CancelledBy = @CancelledBy,
            CancelledAtUtc = @CancelledAtUtc,
            CancellationReason = @CancellationReason,
            ArchivedAtUtc = @ArchivedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE ProjectId = @ProjectId AND TenantId = @TenantId AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @RowsAffected INT = @@ROWCOUNT;

        IF @RowsAffected > 0
        BEGIN
            DELETE FROM [PlanningProjects].[PlanningProjectMilestone] WHERE ProjectId = @ProjectId;

            INSERT INTO [PlanningProjects].[PlanningProjectMilestone] (MilestoneId, ProjectId, Name, TargetDateUtc, IsAchieved)
            SELECT MilestoneId, @ProjectId, Name, TargetDateUtc, IsAchieved FROM @Milestones;
        END

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'PlanningProject', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ProjectId=', @ProjectId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProjectId=', @ProjectId, N'; User=', SYSTEM_USER);
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
