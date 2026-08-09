-- =============================================
-- AI.EnterpriseOrchestration -- Phase1-EnterpriseOrchestrationLog
-- Script:        20-usp-enterpriseorchestrationlog.sql
-- Purpose:       usp_EnterpriseOrchestrationLog_Record / GetRecent.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [AIEnterpriseOrchestration].[usp_EnterpriseOrchestrationLog_Record]
    @FacilityCount                            int,
    @FacilitiesRequiringEvacuationJson         nvarchar(max),
    @FacilitiesWithRegulatoryExposureJson      nvarchar(max),
    @FacilitiesWithAnomalousPredictionsJson    nvarchar(max),
    @HighestSeverityCode                       nvarchar(30),
    @AlertsJson                                nvarchar(max),
    @GeneratedAtUtc                            datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF ISJSON(@FacilitiesRequiringEvacuationJson) = 0 OR ISJSON(@FacilitiesWithRegulatoryExposureJson) = 0
           OR ISJSON(@FacilitiesWithAnomalousPredictionsJson) = 0 OR ISJSON(@AlertsJson) = 0
            THROW 50001, N'Every *Json parameter must be valid JSON.', 1;

        INSERT INTO [AIEnterpriseOrchestration].[EnterpriseOrchestrationLog] (
            [FacilityCount], [FacilitiesRequiringEvacuationJson], [FacilitiesWithRegulatoryExposureJson],
            [FacilitiesWithAnomalousPredictionsJson], [HighestSeverityCode], [AlertsJson], [GeneratedAtUtc]
        )
        VALUES (
            @FacilityCount, @FacilitiesRequiringEvacuationJson, @FacilitiesWithRegulatoryExposureJson,
            @FacilitiesWithAnomalousPredictionsJson, @HighestSeverityCode, @AlertsJson, @GeneratedAtUtc
        );
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextO0 nvarchar(200) = CONCAT(N'Params=FacilityCount=', @FacilityCount, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetailsO0 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextO0, @ErrorDetailsO0, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [AIEnterpriseOrchestration].[usp_EnterpriseOrchestrationLog_GetRecent]
    @TopN int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        SELECT TOP (@TopN) [EnterpriseOrchestrationLogId], [FacilityCount], [FacilitiesRequiringEvacuationJson],
               [FacilitiesWithRegulatoryExposureJson], [FacilitiesWithAnomalousPredictionsJson],
               [HighestSeverityCode], [AlertsJson], [GeneratedAtUtc]
        FROM [AIEnterpriseOrchestration].[EnterpriseOrchestrationLog]
        WHERE ISNULL([IsDeleted], 0) = 0
        ORDER BY [GeneratedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContextO1 nvarchar(200) = CONCAT(N'User=', SYSTEM_USER);
        DECLARE @ErrorDetailsO1 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContextO1, @ErrorDetailsO1, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
