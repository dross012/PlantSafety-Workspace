-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowDefinition_Update.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Updates the mutable fields of a WorkflowDefinition aggregate root row, enforcing
--                  optimistic concurrency via @ETag when supplied. Backs
--                  IWorkflowDefinitionRepository.UpdateAsync. Must be invoked via
--                  ExecuteScalarAsync -- SET NOCOUNT ON makes ExecuteNonQuery always return -1.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowDefinition_Update]
    @WorkflowDefinitionId UNIQUEIDENTIFIER,
    @TenantId             UNIQUEIDENTIFIER,
    @Version              INT,
    @IsActive             BIT,
    @ETag                 NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [Workflow].[WorkflowDefinition]
        SET Version = @Version,
            IsActive = @IsActive
        WHERE WorkflowDefinitionId = @WorkflowDefinitionId
          AND TenantId = @TenantId
          AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        DECLARE @__RowsAffected INT = @@ROWCOUNT;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'WorkflowDefinition', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'WorkflowDefinitionId=', @WorkflowDefinitionId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        SELECT @__RowsAffected;
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
