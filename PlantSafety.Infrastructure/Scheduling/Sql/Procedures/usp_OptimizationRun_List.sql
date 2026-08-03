-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling.PredictiveScheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OptimizationRun_List.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves OptimizationRun rows for a tenant (write-side ListAsync -- filtered
--                  further in-memory via the supplied specification) and, via the same shape, backs
--                  IOptimizationRunReadRepository.ListHistoryAsync's summary projection (optionally
--                  scoped by site/date range/status). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_OptimizationRun_List]
    @TenantId    UNIQUEIDENTIFIER,
    @SiteId      UNIQUEIDENTIFIER = NULL,
    @FromUtc     DATETIME2(3) = NULL,
    @ToUtc       DATETIME2(3) = NULL,
    @StatusCode  NVARCHAR(20) = NULL
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
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@FromUtc IS NULL OR RequestedAtUtc >= @FromUtc)
          AND (@ToUtc IS NULL OR RequestedAtUtc <= @ToUtc)
          AND (@StatusCode IS NULL OR Status = @StatusCode)
        ORDER BY RequestedAtUtc DESC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
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
