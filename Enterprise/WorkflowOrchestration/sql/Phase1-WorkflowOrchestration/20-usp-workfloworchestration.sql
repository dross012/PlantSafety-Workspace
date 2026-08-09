-- =============================================
-- Enterprise.WorkflowOrchestration -- Phase1-WorkflowOrchestration
-- Script:        20-usp-workfloworchestration.sql
-- Purpose:       usp_WorkflowInstance_Create, usp_WorkflowStep_ReplaceExecutionPlan
--                (delete+insert, mirrors BuildExecutionPlan's wholesale
--                replace), usp_WorkflowInstance_Transition (updates
--                CurrentState + appends WorkflowTransition, one transaction,
--                mirrors EnterpriseWorkflowOrchestrator.Transition).
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [EnterpriseWorkflow].[usp_WorkflowInstance_Create]
    @WorkflowId     uniqueidentifier,
    @CreatedAtUtc   datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @WorkflowId IS NULL OR @WorkflowId = '00000000-0000-0000-0000-000000000000'
            THROW 50001, N'WorkflowId is required and must not be empty.', 1;

        INSERT INTO [EnterpriseWorkflow].[WorkflowInstance] ([WorkflowId], [CurrentState], [CreatedAtUtc], [UpdatedAtUtc])
        VALUES (@WorkflowId, N'Pending', @CreatedAtUtc, @CreatedAtUtc);
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextC0 nvarchar(200) = CONCAT(N'Params=WorkflowId=', @WorkflowId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsC0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextC0, @ErrorDetailsC0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EnterpriseWorkflow].[usp_WorkflowInstance_Transition]
    @WorkflowId      uniqueidentifier,
    @FromState       nvarchar(20),
    @ToState         nvarchar(20),
    @OccurredAtUtc   datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @WorkflowId IS NULL
            THROW 50001, N'WorkflowId is required.', 1;

        BEGIN TRANSACTION;

        UPDATE [EnterpriseWorkflow].[WorkflowInstance]
        SET [CurrentState] = @ToState, [UpdatedAtUtc] = @OccurredAtUtc,
            [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHERE [WorkflowId] = @WorkflowId;

        INSERT INTO [EnterpriseWorkflow].[WorkflowTransition] ([WorkflowId], [FromState], [ToState], [OccurredAtUtc])
        VALUES (@WorkflowId, @FromState, @ToState, @OccurredAtUtc);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorContextC1 nvarchar(200) = CONCAT(N'Params=WorkflowId=', @WorkflowId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsC1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextC1, @ErrorDetailsC1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [EnterpriseWorkflow].[usp_WorkflowStep_ReplaceExecutionPlan]
    @WorkflowId       uniqueidentifier,
    @StepsJson        nvarchar(max) -- JSON array: [{"SequenceNumber":0,"StepType":"...","Code":"...","Description":"..."}]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @WorkflowId IS NULL
            THROW 50001, N'WorkflowId is required.', 1;
        IF ISJSON(@StepsJson) = 0
            THROW 50001, N'StepsJson must be valid JSON.', 1;

        BEGIN TRANSACTION;

        DELETE FROM [EnterpriseWorkflow].[WorkflowStep] WHERE [WorkflowId] = @WorkflowId;

        INSERT INTO [EnterpriseWorkflow].[WorkflowStep] ([WorkflowId], [SequenceNumber], [StepType], [Code], [Description])
        SELECT @WorkflowId, j.[SequenceNumber], j.[StepType], j.[Code], j.[Description]
        FROM OPENJSON(@StepsJson)
        WITH (
            [SequenceNumber] INT '$.SequenceNumber',
            [StepType]        NVARCHAR(20) '$.StepType',
            [Code]             NVARCHAR(100) '$.Code',
            [Description]      NVARCHAR(500) '$.Description'
        ) AS j;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorContextC2 nvarchar(200) = CONCAT(N'Params=WorkflowId=', @WorkflowId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsC2 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextC2, @ErrorDetailsC2, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
