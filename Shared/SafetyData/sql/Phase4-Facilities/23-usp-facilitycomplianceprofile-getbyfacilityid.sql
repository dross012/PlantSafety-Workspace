-- =============================================
-- Shared.SafetyData -- Phase4-Facilities
-- Script:        23-usp-facilitycomplianceprofile-getbyfacilityid.sql
-- Purpose:       Creates Facilities.usp_FacilityComplianceProfile_GetByFacilityId
--                against [PlantSafety.CoreDB] -- the point-lookup read path
--                for the current cached compliance scorecard for a given
--                FacilityId.
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

CREATE OR ALTER PROCEDURE [Facilities].[usp_FacilityComplianceProfile_GetByFacilityId]
    @FacilityId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        SELECT
            c.[FacilityId], c.[RequiredElementsCount], c.[CompletedElementsCount], c.[MissingElementsCount],
            c.[ComplianceScorePercent], c.[RiskLevel],
            c.[CreatedUtc], c.[CreatedBy], c.[UpdatedUtc], c.[UpdatedBy]
        FROM [Facilities].[FacilityComplianceProfile] AS c
        WHERE c.[FacilityId] = @FacilityId
          AND ISNULL(c.[IsDeleted], 0) = 0;
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
