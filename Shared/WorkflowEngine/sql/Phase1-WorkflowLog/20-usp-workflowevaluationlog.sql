-- =============================================
-- Shared.WorkflowEngine -- Phase1-WorkflowLog
-- Script:        20-usp-workflowevaluationlog.sql
-- Purpose:       usp_WorkflowEvaluationLog_Record / GetRecentByFacility.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [WorkflowEngine].[usp_WorkflowEvaluationLog_Record]
    @FacilityId                uniqueidentifier = NULL,
    @HazardTypeCode             nvarchar(30),
    @SeverityCode               nvarchar(30),
    @AffectedScope              nvarchar(20),
    @EmployeeExposureCount      int,
    @MachineFailureCount        int,
    @EventChainLength           int,
    @RequiredActionsJson        nvarchar(max),
    @SopTriggersJson            nvarchar(max),
    @PermitTriggersJson         nvarchar(max),
    @EvacuationTriggersJson     nvarchar(max),
    @NotificationRoutingsJson   nvarchar(max),
    @EvaluatedAtUtc             datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HazardTypeCode IS NULL OR @SeverityCode IS NULL
            THROW 50001, N'HazardTypeCode and SeverityCode are required.', 1;
        IF ISJSON(@RequiredActionsJson) = 0 OR ISJSON(@SopTriggersJson) = 0 OR ISJSON(@PermitTriggersJson) = 0
           OR ISJSON(@EvacuationTriggersJson) = 0 OR ISJSON(@NotificationRoutingsJson) = 0
            THROW 50001, N'Every *Json parameter must be valid JSON.', 1;

        INSERT INTO [WorkflowEngine].[WorkflowEvaluationLog] (
            [FacilityId], [HazardTypeCode], [SeverityCode], [AffectedScope],
            [EmployeeExposureCount], [MachineFailureCount], [EventChainLength],
            [RequiredActionsJson], [SopTriggersJson], [PermitTriggersJson],
            [EvacuationTriggersJson], [NotificationRoutingsJson], [EvaluatedAtUtc]
        )
        VALUES (
            @FacilityId, @HazardTypeCode, @SeverityCode, @AffectedScope,
            @EmployeeExposureCount, @MachineFailureCount, @EventChainLength,
            @RequiredActionsJson, @SopTriggersJson, @PermitTriggersJson,
            @EvacuationTriggersJson, @NotificationRoutingsJson, @EvaluatedAtUtc
        );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext60 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails60 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext60, @ErrorDetails60, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [WorkflowEngine].[usp_WorkflowEvaluationLog_GetRecentByFacility]
    @FacilityId  uniqueidentifier,
    @TopN        int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        SELECT TOP (@TopN) [WorkflowEvaluationLogId], [FacilityId], [HazardTypeCode], [SeverityCode],
               [AffectedScope], [EmployeeExposureCount], [MachineFailureCount], [EventChainLength],
               [RequiredActionsJson], [SopTriggersJson], [PermitTriggersJson],
               [EvacuationTriggersJson], [NotificationRoutingsJson], [EvaluatedAtUtc]
        FROM [WorkflowEngine].[WorkflowEvaluationLog]
        WHERE [FacilityId] = @FacilityId AND ISNULL([IsDeleted], 0) = 0
        ORDER BY [EvaluatedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext61 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails61 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext61, @ErrorDetails61, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
