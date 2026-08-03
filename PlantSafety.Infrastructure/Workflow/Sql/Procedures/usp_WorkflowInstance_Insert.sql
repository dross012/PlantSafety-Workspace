-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowInstance_Insert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a new WorkflowInstance aggregate root row. Backs
--                  IWorkflowInstanceRepository.AddAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowInstance_Insert]
    @WorkflowInstanceId   UNIQUEIDENTIFIER,
    @WorkflowDefinitionId UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER,
    @SiteId               UNIQUEIDENTIFIER = NULL,
    @WorkflowType         NVARCHAR(30),
    @State                NVARCHAR(30),
    @CurrentStepId        UNIQUEIDENTIFIER = NULL,
    @ContextDataJson      NVARCHAR(MAX),
    @StartedAtUtc         DATETIME2(3),
    @CompletedAtUtc       DATETIME2(3) = NULL,
    @InitiatedBy          UNIQUEIDENTIFIER,
    @CorrelationId        UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [Workflow].[WorkflowInstance]
        (WorkflowInstanceId, WorkflowDefinitionId, TenantId, SiteId, WorkflowType, State, CurrentStepId, ContextDataJson, StartedAtUtc, CompletedAtUtc, InitiatedBy, CorrelationId, IsDeleted)
        VALUES
        (@WorkflowInstanceId, @WorkflowDefinitionId, @TenantId, @SiteId, @WorkflowType, @State, @CurrentStepId, @ContextDataJson, @StartedAtUtc, @CompletedAtUtc, @InitiatedBy, @CorrelationId, 0);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'WorkflowInstance', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'WorkflowInstanceId=', @WorkflowInstanceId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Workflow].[WorkflowInstance]
        WHERE WorkflowInstanceId = @WorkflowInstanceId;
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
