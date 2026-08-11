-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowStepDefinition_Upsert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a WorkflowStepDefinition child row if it does not already exist (step
--                  definitions are immutable once added -- no update branch is needed). Backs
--                  IWorkflowDefinitionRepository.UpdateAsync (reconciling AddStep calls).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowStepDefinition_Upsert]
    @StepId                 UNIQUEIDENTIFIER,
    @WorkflowDefinitionId    UNIQUEIDENTIFIER,
    @Order                   INT,
    @Name                    NVARCHAR(200),
    @StepType                NVARCHAR(30),
    @AssignedRoleCode        NVARCHAR(100) = NULL,
    @AssignedUserId          UNIQUEIDENTIFIER = NULL,
    @EscalationRoleCode      NVARCHAR(100) = NULL,
    @EscalationUserId        UNIQUEIDENTIFIER = NULL,
    @EntryConditionField     NVARCHAR(100) = NULL,
    @EntryConditionOperator  NVARCHAR(20) = NULL,
    @EntryConditionValue     NVARCHAR(400) = NULL,
    @TimeoutSeconds          INT = NULL,
    @TimeoutAction           NVARCHAR(20) = NULL,
    @RetryMaxAttempts        INT = NULL,
    @RetryBackoffStrategy    NVARCHAR(20) = NULL,
    @RetryBaseDelaySeconds   INT = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Workflow].[WorkflowStepDefinition] WHERE StepId = @StepId)
        BEGIN
            INSERT INTO [Workflow].[WorkflowStepDefinition]
            (StepId, WorkflowDefinitionId, [Order], Name, StepType, AssignedRoleCode, AssignedUserId, EscalationRoleCode, EscalationUserId,
             EntryConditionField, EntryConditionOperator, EntryConditionValue, TimeoutSeconds, TimeoutAction, RetryMaxAttempts, RetryBackoffStrategy, RetryBaseDelaySeconds)
            VALUES
            (@StepId, @WorkflowDefinitionId, @Order, @Name, @StepType, @AssignedRoleCode, @AssignedUserId, @EscalationRoleCode, @EscalationUserId,
             @EntryConditionField, @EntryConditionOperator, @EntryConditionValue, @TimeoutSeconds, @TimeoutAction, @RetryMaxAttempts, @RetryBackoffStrategy, @RetryBaseDelaySeconds);
        END
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'WorkflowStepDefinition', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'StepId=', @StepId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'StepId=', @StepId, N'; User=', SYSTEM_USER);
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
