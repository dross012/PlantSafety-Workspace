-- =============================================
-- AI.WorkflowAutomation -- Phase1-WorkflowAutomationLog
-- Script:        20-usp-workflowautomationlog.sql
-- Purpose:       usp_WorkflowAutomationLog_Record / GetRecent.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [AIWorkflowAutomation].[usp_WorkflowAutomationLog_Record]
    @RequiredActionsJson          nvarchar(max),
    @SopStepsJson                 nvarchar(max),
    @PermitRequirementsJson       nvarchar(max),
    @EvacuationStepsJson          nvarchar(max),
    @ShutdownRestartSequenceJson  nvarchar(max),
    @ExecutionPlanJson            nvarchar(max),
    @GeneratedAtUtc               datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF ISJSON(@RequiredActionsJson) = 0 OR ISJSON(@SopStepsJson) = 0 OR ISJSON(@PermitRequirementsJson) = 0
           OR ISJSON(@EvacuationStepsJson) = 0 OR ISJSON(@ShutdownRestartSequenceJson) = 0 OR ISJSON(@ExecutionPlanJson) = 0
            THROW 50001, N'Every *Json parameter must be valid JSON.', 1;

        INSERT INTO [AIWorkflowAutomation].[WorkflowAutomationLog] (
            [RequiredActionsJson], [SopStepsJson], [PermitRequirementsJson],
            [EvacuationStepsJson], [ShutdownRestartSequenceJson], [ExecutionPlanJson], [GeneratedAtUtc]
        )
        VALUES (
            @RequiredActionsJson, @SopStepsJson, @PermitRequirementsJson,
            @EvacuationStepsJson, @ShutdownRestartSequenceJson, @ExecutionPlanJson, @GeneratedAtUtc
        );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextI0 nvarchar(200) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @ErrorDetailsI0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextI0, @ErrorDetailsI0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [AIWorkflowAutomation].[usp_WorkflowAutomationLog_GetRecent]
    @TopN int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT TOP (@TopN) [WorkflowAutomationLogId], [RequiredActionsJson], [SopStepsJson],
               [PermitRequirementsJson], [EvacuationStepsJson], [ShutdownRestartSequenceJson],
               [ExecutionPlanJson], [GeneratedAtUtc]
        FROM [AIWorkflowAutomation].[WorkflowAutomationLog]
        WHERE ISNULL([IsDeleted], 0) = 0
        ORDER BY [GeneratedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextI1 nvarchar(200) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @ErrorDetailsI1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextI1, @ErrorDetailsI1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
