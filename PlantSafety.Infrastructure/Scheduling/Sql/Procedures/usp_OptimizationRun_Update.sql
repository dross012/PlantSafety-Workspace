-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling.PredictiveScheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OptimizationRun_Update.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Updates an existing OptimizationRun aggregate root row (status/result
--                  transitions -- Begin/Complete/Fail). Backs IOptimizationRunRepository.UpdateAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_OptimizationRun_Update]
    @OptimizationRunId       UNIQUEIDENTIFIER,
    @TenantId                UNIQUEIDENTIFIER,
    @Status                  NVARCHAR(20),
    @SelectedStrategyName    NVARCHAR(100) = NULL,
    @Score                   FLOAT = NULL,
    @FailureReason           NVARCHAR(1000) = NULL,
    @AssignmentCount         INT,
    @UnscheduledTaskCount    INT,
    @ToleratedViolationCount INT,
    @ResultAssignmentsJson   NVARCHAR(MAX),
    @UnscheduledTaskIdsJson  NVARCHAR(MAX),
    @ToleratedViolationsJson NVARCHAR(MAX),
    @CompletedAtUtc          DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [Scheduling].[OptimizationRun]
        SET Status                  = @Status,
            SelectedStrategyName    = @SelectedStrategyName,
            Score                   = @Score,
            FailureReason           = @FailureReason,
            AssignmentCount         = @AssignmentCount,
            UnscheduledTaskCount    = @UnscheduledTaskCount,
            ToleratedViolationCount = @ToleratedViolationCount,
            ResultAssignmentsJson   = @ResultAssignmentsJson,
            UnscheduledTaskIdsJson  = @UnscheduledTaskIdsJson,
            ToleratedViolationsJson = @ToleratedViolationsJson,
            CompletedAtUtc          = @CompletedAtUtc
        WHERE OptimizationRunId = @OptimizationRunId
          AND TenantId = @TenantId
          AND IsDeleted = 0;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'OptimizationRun', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'OptimizationRunId=', @OptimizationRunId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'OptimizationRunId=', @OptimizationRunId, N'; User=', SYSTEM_USER);
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
