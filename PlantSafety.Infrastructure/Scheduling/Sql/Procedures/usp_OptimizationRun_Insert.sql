-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling.PredictiveScheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OptimizationRun_Insert.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new OptimizationRun aggregate root row. Backs
--                  IOptimizationRunRepository.AddAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_OptimizationRun_Insert]
    @OptimizationRunId       UNIQUEIDENTIFIER,
    @TenantId                UNIQUEIDENTIFIER,
    @SiteId                  UNIQUEIDENTIFIER = NULL,
    @Objective               NVARCHAR(30),
    @Status                  NVARCHAR(20),
    @SelectedStrategyName    NVARCHAR(100) = NULL,
    @Score                   FLOAT = NULL,
    @FailureReason           NVARCHAR(1000) = NULL,
    @InputTaskCount          INT,
    @InputAvailabilityCount  INT,
    @AssignmentCount         INT,
    @UnscheduledTaskCount    INT,
    @ToleratedViolationCount INT,
    @InputTasksJson          NVARCHAR(MAX),
    @InputAvailabilityJson   NVARCHAR(MAX),
    @ResultAssignmentsJson   NVARCHAR(MAX),
    @UnscheduledTaskIdsJson  NVARCHAR(MAX),
    @ToleratedViolationsJson NVARCHAR(MAX),
    @RequestedAtUtc          DATETIME2(3),
    @CompletedAtUtc          DATETIME2(3) = NULL,
    @RequestedBy             UNIQUEIDENTIFIER = NULL,
    @CorrelationId           UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Scheduling].[OptimizationRun]
        (OptimizationRunId, TenantId, SiteId, Objective, Status, SelectedStrategyName, Score, FailureReason,
         InputTaskCount, InputAvailabilityCount, AssignmentCount, UnscheduledTaskCount, ToleratedViolationCount,
         InputTasksJson, InputAvailabilityJson, ResultAssignmentsJson, UnscheduledTaskIdsJson, ToleratedViolationsJson,
         RequestedAtUtc, CompletedAtUtc, RequestedBy, CorrelationId, IsDeleted)
        VALUES
        (@OptimizationRunId, @TenantId, @SiteId, @Objective, @Status, @SelectedStrategyName, @Score, @FailureReason,
         @InputTaskCount, @InputAvailabilityCount, @AssignmentCount, @UnscheduledTaskCount, @ToleratedViolationCount,
         @InputTasksJson, @InputAvailabilityJson, @ResultAssignmentsJson, @UnscheduledTaskIdsJson, @ToleratedViolationsJson,
         @RequestedAtUtc, @CompletedAtUtc, @RequestedBy, @CorrelationId, 0);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'OptimizationRun', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'OptimizationRunId=', @OptimizationRunId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Scheduling].[OptimizationRun]
        WHERE OptimizationRunId = @OptimizationRunId;
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
