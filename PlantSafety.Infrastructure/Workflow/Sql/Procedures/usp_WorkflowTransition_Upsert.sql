-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowTransition_Upsert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a WorkflowTransition edge row if an equivalent one does not already
--                  exist for the same (WorkflowDefinitionId, FromStepId, ToStepId) triple.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowTransition_Upsert]
    @WorkflowDefinitionId UNIQUEIDENTIFIER,
    @FromStepId            UNIQUEIDENTIFIER,
    @ToStepId              UNIQUEIDENTIFIER,
    @GuardField             NVARCHAR(100) = NULL,
    @GuardOperator          NVARCHAR(20) = NULL,
    @GuardExpectedValue     NVARCHAR(400) = NULL,
    @IsDefault             BIT
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF NOT EXISTS (
            SELECT 1 FROM [Workflow].[WorkflowTransition]
            WHERE WorkflowDefinitionId = @WorkflowDefinitionId AND FromStepId = @FromStepId AND ToStepId = @ToStepId)
        BEGIN
            INSERT INTO [Workflow].[WorkflowTransition]
            (WorkflowDefinitionId, FromStepId, ToStepId, GuardField, GuardOperator, GuardExpectedValue, IsDefault)
            VALUES
            (@WorkflowDefinitionId, @FromStepId, @ToStepId, @GuardField, @GuardOperator, @GuardExpectedValue, @IsDefault);
        END
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'WorkflowTransition', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'WorkflowDefinitionId=', @WorkflowDefinitionId), 0, OBJECT_NAME(@@PROCID));
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
