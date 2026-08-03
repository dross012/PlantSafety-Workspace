-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling.PredictiveScheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OptimizationRun_GetById.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single OptimizationRun row by identity, scoped to tenant. Backs
--                  IOptimizationRunRepository.GetByIdAsync and IOptimizationRunReadRepository
--                  .GetByIdAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_OptimizationRun_GetById]
    @OptimizationRunId UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            OptimizationRunId, TenantId, SiteId, Objective, Status, SelectedStrategyName, Score, FailureReason,
            InputTaskCount, InputAvailabilityCount, AssignmentCount, UnscheduledTaskCount, ToleratedViolationCount,
            InputTasksJson, InputAvailabilityJson, ResultAssignmentsJson, UnscheduledTaskIdsJson, ToleratedViolationsJson,
            RequestedAtUtc, CompletedAtUtc, RequestedBy, CorrelationId,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Scheduling].[OptimizationRun]
        WHERE OptimizationRunId = @OptimizationRunId
          AND TenantId = @TenantId
          AND IsDeleted = 0;
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
