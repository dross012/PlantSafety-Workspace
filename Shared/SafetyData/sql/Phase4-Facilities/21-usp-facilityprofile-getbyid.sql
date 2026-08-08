-- =============================================
-- Shared.SafetyData -- Phase4-Facilities
-- Script:        21-usp-facilityprofile-getbyid.sql
-- Purpose:       Creates Facilities.usp_FacilityProfile_GetById against
--                [PlantSafety.CoreDB] -- the point-lookup read path for the
--                current cached FacilityProfile snapshot for a given
--                FacilityId.
--
-- GOVERNANCE NOTE: no viewer-access AuditLog INSERT -- read-only, matches
-- Phase2/Phase3 precedent for simple point-lookup read procedures.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Facilities].[usp_FacilityProfile_GetById]
    @FacilityId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        SELECT
            f.[FacilityId], f.[Name], f.[IndustryName], f.[Location], f.[Flags],
            f.[CreatedUtc], f.[CreatedBy], f.[UpdatedUtc], f.[UpdatedBy]
        FROM [Facilities].[FacilityProfile] AS f
        WHERE f.[FacilityId] = @FacilityId
          AND ISNULL(f.[IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext21 nvarchar(200) = CONCAT(N'Params=FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails21 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext21, @ErrorDetails21, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
