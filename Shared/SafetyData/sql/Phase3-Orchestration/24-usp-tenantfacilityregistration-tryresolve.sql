-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        24-usp-tenantfacilityregistration-tryresolve.sql
-- Purpose:       Creates Orchestration.usp_TenantFacilityRegistration_TryResolve
--                against [PlantSafety.CoreDB] -- mirrors
--                ITenantFacilityRegistry.TryResolve(string tenantId, out
--                TenantFacilityRegistration? registration): zero or one row
--                back for the given tenant.
--
-- GOVERNANCE NOTE: no viewer-access AuditLog INSERT -- read-only, called for
-- every inbound orchestration event (EventOrchestrationService.cs); matches
-- Phase2-SafetyTaxonomy's 20-usp-get-industries.sql precedent.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Orchestration].[usp_TenantFacilityRegistration_TryResolve]
    @TenantId nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @TenantId IS NULL OR LTRIM(RTRIM(@TenantId)) = N''
            THROW 50001, N'TenantId is required.', 1;

        SELECT
            t.[TenantId], t.[FacilityId], t.[Name], t.[IndustryName], t.[Location],
            t.[CreatedUtc], t.[CreatedBy], t.[UpdatedUtc], t.[UpdatedBy]
        FROM [Orchestration].[TenantFacilityRegistration] AS t
        WHERE t.[TenantId] = @TenantId
          AND ISNULL(t.[IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext24 nvarchar(200) = CONCAT(N'Params=TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails24 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext24, @ErrorDetails24, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
