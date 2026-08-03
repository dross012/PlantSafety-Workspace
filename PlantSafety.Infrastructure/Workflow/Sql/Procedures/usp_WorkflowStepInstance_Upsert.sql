-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowStepInstance_Upsert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts or updates a WorkflowStepInstance execution-history row. Backs
--                  IWorkflowInstanceRepository.UpdateAsync's step-history reconciliation.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowStepInstance_Upsert]
    @StepInstanceId         UNIQUEIDENTIFIER,
    @WorkflowInstanceId     UNIQUEIDENTIFIER,
    @StepId                 UNIQUEIDENTIFIER,
    @StepType               NVARCHAR(30),
    @Status                 NVARCHAR(30),
    @AssignedRoleCode       NVARCHAR(100) = NULL,
    @AssignedUserId         UNIQUEIDENTIFIER = NULL,
    @ActivatedAtUtc         DATETIME2(3) = NULL,
    @DecidedAtUtc           DATETIME2(3) = NULL,
    @DecidedBy              UNIQUEIDENTIFIER = NULL,
    @DecisionNotes          NVARCHAR(2000) = NULL,
    @AttemptCount           INT,
    @NextAttemptAtUtc       DATETIME2(3) = NULL,
    @RecommendationReference NVARCHAR(400) = NULL,
    @SequenceNo             INT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM [Workflow].[WorkflowStepInstance] WHERE StepInstanceId = @StepInstanceId)
        BEGIN
            UPDATE [Workflow].[WorkflowStepInstance]
            SET Status = @Status,
                AssignedRoleCode = @AssignedRoleCode,
                AssignedUserId = @AssignedUserId,
                ActivatedAtUtc = @ActivatedAtUtc,
                DecidedAtUtc = @DecidedAtUtc,
                DecidedBy = @DecidedBy,
                DecisionNotes = @DecisionNotes,
                AttemptCount = @AttemptCount,
                NextAttemptAtUtc = @NextAttemptAtUtc,
                RecommendationReference = @RecommendationReference
            WHERE StepInstanceId = @StepInstanceId;
        END
        ELSE
        BEGIN
            INSERT INTO [Workflow].[WorkflowStepInstance]
            (StepInstanceId, WorkflowInstanceId, StepId, StepType, Status, AssignedRoleCode, AssignedUserId, ActivatedAtUtc,
             DecidedAtUtc, DecidedBy, DecisionNotes, AttemptCount, NextAttemptAtUtc, RecommendationReference, SequenceNo)
            VALUES
            (@StepInstanceId, @WorkflowInstanceId, @StepId, @StepType, @Status, @AssignedRoleCode, @AssignedUserId, @ActivatedAtUtc,
             @DecidedAtUtc, @DecidedBy, @DecisionNotes, @AttemptCount, @NextAttemptAtUtc, @RecommendationReference, @SequenceNo);
        END
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'StepInstanceId=', @StepInstanceId, N'; User=', SYSTEM_USER);
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
