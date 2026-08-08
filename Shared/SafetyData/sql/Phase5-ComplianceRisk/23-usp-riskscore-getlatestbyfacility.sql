-- =============================================
-- Shared.SafetyData -- Phase5-ComplianceRisk
-- Script:        23-usp-riskscore-getlatestbyfacility.sql
-- Purpose:       Creates ComplianceRisk.usp_RiskScore_GetLatestByFacility
--                against [PlantSafety.CoreDB] -- the most recently computed
--                RiskScore row for a given FacilityId.
--
-- GOVERNANCE NOTE: no viewer-access AuditLog INSERT -- read-only.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [ComplianceRisk].[usp_RiskScore_GetLatestByFacility]
    @FacilityId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        SELECT TOP (1)
            s.[FacilityId], s.[ComputedAtUtc], s.[HazardRiskLevel], s.[ProgramRiskLevel],
            s.[RegulatorRiskLevel], s.[EventRiskLevel], s.[OverallRiskLevel],
            s.[CreatedUtc], s.[CreatedBy], s.[UpdatedUtc], s.[UpdatedBy]
        FROM [ComplianceRisk].[RiskScore] AS s
        WHERE s.[FacilityId] = @FacilityId
          AND ISNULL(s.[IsDeleted], 0) = 0
        ORDER BY s.[ComputedAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext23 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails23 nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (
                ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                SYSUTCDATETIME(), @ErrorContext23, @ErrorDetails23, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
