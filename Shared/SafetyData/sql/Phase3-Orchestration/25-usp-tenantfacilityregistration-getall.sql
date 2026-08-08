-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        25-usp-tenantfacilityregistration-getall.sql
-- Purpose:       Creates Orchestration.usp_TenantFacilityRegistration_GetAll
--                against [PlantSafety.CoreDB] -- mirrors
--                ITenantFacilityRegistry.GetAll(): every currently
--                registered tenant/facility binding, in no particular order
--                (no ORDER BY, matching the interface's own "in no
--                particular order" contract and validate-view/proc
--                determinism -- callers order the result if they need to).
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

CREATE OR ALTER PROCEDURE [Orchestration].[usp_TenantFacilityRegistration_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT
            t.[TenantId], t.[FacilityId], t.[Name], t.[IndustryName], t.[Location],
            t.[CreatedUtc], t.[CreatedBy], t.[UpdatedUtc], t.[UpdatedBy]
        FROM [Orchestration].[TenantFacilityRegistration] AS t
        WHERE ISNULL(t.[IsDeleted], 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext25 nvarchar(200) = CONCAT(N'Params=(none); User=', SYSTEM_USER);
        DECLARE @ErrorDetails25 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext25, @ErrorDetails25, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
