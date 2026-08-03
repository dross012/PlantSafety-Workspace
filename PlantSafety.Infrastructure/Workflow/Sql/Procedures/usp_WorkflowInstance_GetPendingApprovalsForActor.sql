-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Workflow
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkflowInstance_GetPendingApprovalsForActor.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves all WorkflowInstance rows currently waiting on a HumanApproval step
--                  assigned either directly to @UserId or to one of the roles supplied via
--                  @UserRolesCsv (comma-separated). Backs
--                  IWorkflowInstanceReadRepository.ListPendingApprovalsForActorAsync /
--                  GetPendingApprovalsForActorQuery. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Workflow].[usp_WorkflowInstance_GetPendingApprovalsForActor]
    @TenantId      UNIQUEIDENTIFIER,
    @UserId        UNIQUEIDENTIFIER,
    @UserRolesCsv  NVARCHAR(MAX) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            wi.WorkflowInstanceId, wi.WorkflowDefinitionId, wi.TenantId, wi.SiteId, wi.WorkflowType, wi.State, wi.CurrentStepId, wi.ContextDataJson,
            wi.StartedAtUtc, wi.CompletedAtUtc, wi.InitiatedBy, wi.CorrelationId,
            CONVERT(NVARCHAR(34), CAST(wi.RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Workflow].[WorkflowInstance] wi
        INNER JOIN [Workflow].[WorkflowStepInstance] si
            ON si.WorkflowInstanceId = wi.WorkflowInstanceId AND si.StepId = wi.CurrentStepId AND si.Status = N'Active'
        WHERE wi.TenantId = @TenantId
          AND wi.IsDeleted = 0
          AND wi.State = N'WaitingForApproval'
          AND (
                si.AssignedUserId = @UserId
                OR (
                    si.AssignedRoleCode IS NOT NULL
                    AND @UserRolesCsv IS NOT NULL
                    AND EXISTS (
                        SELECT 1 FROM STRING_SPLIT(@UserRolesCsv, ',') r
                        WHERE LTRIM(RTRIM(r.value)) = si.AssignedRoleCode
                    )
                )
              );
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; UserId=', @UserId, N'; User=', SYSTEM_USER);
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
