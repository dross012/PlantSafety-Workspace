-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowInstance_Update.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Updates the mutable fields of a WorkflowInstance aggregate root row, enforcing
--                  optimistic concurrency via @ETag when supplied. Backs
--                  IWorkflowInstanceRepository.UpdateAsync. Must be invoked via ExecuteScalarAsync
--                  -- SET NOCOUNT ON makes ExecuteNonQuery always return -1.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowInstance_Update]
    @WorkflowInstanceId UNIQUEIDENTIFIER,
    @TenantId           UNIQUEIDENTIFIER,
    @State              NVARCHAR(30),
    @CurrentStepId      UNIQUEIDENTIFIER = NULL,
    @ContextDataJson    NVARCHAR(MAX),
    @CompletedAtUtc     DATETIME2(3) = NULL,
    @ETag               NVARCHAR(64) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        UPDATE [Workflow].[WorkflowInstance]
        SET State = @State,
            CurrentStepId = @CurrentStepId,
            ContextDataJson = @ContextDataJson,
            CompletedAtUtc = @CompletedAtUtc
        WHERE WorkflowInstanceId = @WorkflowInstanceId
          AND TenantId = @TenantId
          AND IsDeleted = 0
          AND (@ETag IS NULL OR CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) = @ETag);

        SELECT @@ROWCOUNT;
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
