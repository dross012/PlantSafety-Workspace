-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowTransition_BulkUpsert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Performance Optimization Agent, Phase 5 §7.3)
--  SchemaVersion:  2026.1
--  Description:    Set-based replacement for the per-row usp_WorkflowTransition_Upsert loop
--                  previously issued once per transition from WorkflowDefinitionDapperRepository.
--                  AddAsync/UpdateAsync. Accepts every transition for the definition as a single
--                  table-valued parameter and inserts only the ones that do not already exist for
--                  the same (WorkflowDefinitionId, FromStepId, ToStepId) triple -- the set-based
--                  equivalent of usp_WorkflowTransition_Upsert's existing IF NOT EXISTS semantics,
--                  not a behavior change. usp_WorkflowTransition_Upsert.sql is left in place,
--                  unmodified, for any other caller.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF TYPE_ID(N'[Workflow].[WorkflowTransitionTableType]') IS NULL
BEGIN
    EXEC('CREATE TYPE [Workflow].[WorkflowTransitionTableType] AS TABLE
    (
        FromStepId          UNIQUEIDENTIFIER NOT NULL,
        ToStepId             UNIQUEIDENTIFIER NOT NULL,
        GuardField           NVARCHAR(100)    NULL,
        GuardOperator        NVARCHAR(20)     NULL,
        GuardExpectedValue   NVARCHAR(400)    NULL,
        IsDefault            BIT              NOT NULL
    )');
END
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowTransition_BulkUpsert]
    @WorkflowDefinitionId UNIQUEIDENTIFIER,
    @Transitions [Workflow].[WorkflowTransitionTableType] READONLY
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Workflow].[WorkflowTransition]
        (WorkflowDefinitionId, FromStepId, ToStepId, GuardField, GuardOperator, GuardExpectedValue, IsDefault)
        SELECT
            @WorkflowDefinitionId, t.FromStepId, t.ToStepId, t.GuardField, t.GuardOperator, t.GuardExpectedValue, t.IsDefault
        FROM @Transitions t
        WHERE NOT EXISTS (
            SELECT 1 FROM [Workflow].[WorkflowTransition] wt
            WHERE wt.WorkflowDefinitionId = @WorkflowDefinitionId AND wt.FromStepId = t.FromStepId AND wt.ToStepId = t.ToStepId);
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
