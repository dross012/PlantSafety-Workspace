-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowStepDefinition_BulkUpsert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Performance Optimization Agent, Phase 5 §7.3)
--  SchemaVersion:  2026.1
--  Description:    Set-based replacement for the per-row usp_WorkflowStepDefinition_Upsert loop
--                  previously issued once per step from WorkflowDefinitionDapperRepository.AddAsync/
--                  UpdateAsync (a genuine N+1 write pattern for workflow definitions with many
--                  steps). Accepts every step for the definition as a single table-valued
--                  parameter and inserts only the ones that do not already exist (steps are
--                  immutable once added, matching usp_WorkflowStepDefinition_Upsert's existing
--                  IF NOT EXISTS semantics -- this is the set-based equivalent, not a behavior
--                  change). Table type + bulk-insert pattern mirrors the established
--                  PlanningProjects.PlanningProjectMilestoneTableType /
--                  usp_PlanningProject_Insert.sql precedent (this codebase's only prior TVP
--                  bulk-write example). usp_WorkflowStepDefinition_Upsert.sql is left in place,
--                  unmodified, for any other caller; this is purely an additive alternative.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF TYPE_ID(N'[Workflow].[WorkflowStepDefinitionTableType]') IS NULL
BEGIN
    EXEC('CREATE TYPE [Workflow].[WorkflowStepDefinitionTableType] AS TABLE
    (
        StepId                  UNIQUEIDENTIFIER NOT NULL,
        [Order]                 INT              NOT NULL,
        Name                    NVARCHAR(200)    NOT NULL,
        StepType                NVARCHAR(30)     NOT NULL,
        AssignedRoleCode        NVARCHAR(100)    NULL,
        AssignedUserId          UNIQUEIDENTIFIER NULL,
        EscalationRoleCode      NVARCHAR(100)    NULL,
        EscalationUserId        UNIQUEIDENTIFIER NULL,
        EntryConditionField     NVARCHAR(100)    NULL,
        EntryConditionOperator  NVARCHAR(20)     NULL,
        EntryConditionValue     NVARCHAR(400)    NULL,
        TimeoutSeconds          INT              NULL,
        TimeoutAction           NVARCHAR(20)     NULL,
        RetryMaxAttempts        INT              NULL,
        RetryBackoffStrategy    NVARCHAR(20)     NULL,
        RetryBaseDelaySeconds   INT              NULL
    )');
END
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowStepDefinition_BulkUpsert]
    @WorkflowDefinitionId UNIQUEIDENTIFIER,
    @Steps [Workflow].[WorkflowStepDefinitionTableType] READONLY
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Workflow].[WorkflowStepDefinition]
        (StepId, WorkflowDefinitionId, [Order], Name, StepType, AssignedRoleCode, AssignedUserId, EscalationRoleCode, EscalationUserId,
         EntryConditionField, EntryConditionOperator, EntryConditionValue, TimeoutSeconds, TimeoutAction, RetryMaxAttempts, RetryBackoffStrategy, RetryBaseDelaySeconds)
        SELECT
            s.StepId, @WorkflowDefinitionId, s.[Order], s.Name, s.StepType, s.AssignedRoleCode, s.AssignedUserId, s.EscalationRoleCode, s.EscalationUserId,
            s.EntryConditionField, s.EntryConditionOperator, s.EntryConditionValue, s.TimeoutSeconds, s.TimeoutAction, s.RetryMaxAttempts, s.RetryBackoffStrategy, s.RetryBaseDelaySeconds
        FROM @Steps s
        WHERE NOT EXISTS (SELECT 1 FROM [Workflow].[WorkflowStepDefinition] wsd WHERE wsd.StepId = s.StepId);
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'WorkflowStepDefinition', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'WorkflowDefinitionId=', @WorkflowDefinitionId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkflowDefinitionId=', @WorkflowDefinitionId, N'; User=', SYSTEM_USER);
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
