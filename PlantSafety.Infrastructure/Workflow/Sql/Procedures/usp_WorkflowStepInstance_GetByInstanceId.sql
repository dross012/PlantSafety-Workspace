-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowStepInstance_GetByInstanceId.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves the full step execution history for a WorkflowInstance, in activation
--                  (sequence) order. Backs IWorkflowInstanceRepository aggregate hydration and
--                  IWorkflowInstanceReadRepository.ListHistoryAsync / GetWorkflowHistoryQuery.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowStepInstance_GetByInstanceId]
    @WorkflowInstanceId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            StepInstanceId, WorkflowInstanceId, StepId, StepType, Status, AssignedRoleCode, AssignedUserId, ActivatedAtUtc,
            DecidedAtUtc, DecidedBy, DecisionNotes, AttemptCount, NextAttemptAtUtc, RecommendationReference, SequenceNo
        FROM [Workflow].[WorkflowStepInstance]
        WHERE WorkflowInstanceId = @WorkflowInstanceId
        ORDER BY SequenceNo;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkflowInstanceId=', @WorkflowInstanceId, N'; User=', SYSTEM_USER);
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
