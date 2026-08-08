-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        21-usp-facilityeventorchestrationresult-getrecent.sql
-- Purpose:       Creates Orchestration.usp_FacilityEventOrchestrationResult_GetRecent
--                against [PlantSafety.CoreDB] -- mirrors
--                IFacilityEventOrchestrationResultStore.GetRecent(string?
--                tenantId, int maxResults) / InMemoryFacilityEventOrchestrationResultStore.
--                GetRecent: newest-first (OccurredAtUtc DESC), optionally
--                filtered by TenantId, capped at @MaxResults.
--
-- GOVERNANCE NOTE: no viewer-access AuditLog INSERT -- read-only, high
-- frequency query path (dashboard polling); matches Phase2-SafetyTaxonomy's
-- 20-usp-get-industries.sql precedent for read procedures.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Orchestration].[usp_FacilityEventOrchestrationResult_GetRecent]
    @TenantId   nvarchar(100) = NULL,
    @MaxResults int           = 50
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @MaxResults IS NULL OR @MaxResults <= 0
            THROW 50001, N'MaxResults must be a positive integer.', 1;

        SELECT TOP (@MaxResults)
            r.[EventId], r.[TenantId], r.[FacilityId], r.[SourceModule], r.[EventType], r.[OccurredAtUtc],
            r.[ComplianceRequiredElementsCount], r.[ComplianceCompletedElementsCount], r.[ComplianceMissingElementsCount],
            r.[CompliancePercentComplete], r.[ComplianceTier], r.[ComplianceNotes],
            r.[RiskHazardLevel], r.[RiskProgramLevel], r.[RiskRegulatorLevel], r.[RiskEventLevel], r.[RiskOverallLevel],
            r.[Category], r.[RequiresImmediateAttention], r.[Notification],
            r.[CreatedUtc], r.[CreatedBy], r.[UpdatedUtc], r.[UpdatedBy]
        FROM [Orchestration].[FacilityEventOrchestrationResult] AS r
        WHERE (@TenantId IS NULL OR r.[TenantId] = @TenantId)
          AND ISNULL(r.[IsDeleted], 0) = 0
        ORDER BY r.[OccurredAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext21 nvarchar(200) = CONCAT(N'Params=TenantId=', @TenantId, N'; MaxResults=', @MaxResults, N'; User=', SYSTEM_USER);
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
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
