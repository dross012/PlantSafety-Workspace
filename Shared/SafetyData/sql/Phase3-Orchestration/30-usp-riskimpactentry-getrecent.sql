-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        30-usp-riskimpactentry-getrecent.sql
-- Purpose:       Creates Orchestration.usp_RiskImpactEntry_GetRecent against
--                [PlantSafety.CoreDB] -- mirrors IRiskImpactStore.
--                GetRecent(string? tenantId, int maxResults): newest-first
--                (OccurredAtUtc DESC), optionally filtered by TenantId,
--                capped at @MaxResults.
--
-- GOVERNANCE NOTE: no viewer-access AuditLog INSERT -- read-only, high
-- frequency query path.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Orchestration].[usp_RiskImpactEntry_GetRecent]
    @TenantId   nvarchar(100) = NULL,
    @MaxResults int           = 50
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @MaxResults IS NULL OR @MaxResults <= 0
            THROW 50001, N'MaxResults must be a positive integer.', 1;

        SELECT TOP (@MaxResults)
            e.[EventId], e.[RiskId], e.[TenantId], e.[FacilityId], e.[SourceModule], e.[EventType], e.[OccurredAtUtc],
            e.[ComplianceRequiredElementsCount], e.[ComplianceCompletedElementsCount], e.[ComplianceMissingElementsCount],
            e.[CompliancePercentComplete], e.[ComplianceTier], e.[ComplianceNotes],
            e.[RiskHazardLevel], e.[RiskProgramLevel], e.[RiskRegulatorLevel], e.[RiskEventLevel], e.[RiskOverallLevel],
            e.[Category], e.[RequiresImmediateAttention], e.[Notification],
            e.[CreatedUtc], e.[CreatedBy], e.[UpdatedUtc], e.[UpdatedBy]
        FROM [Orchestration].[RiskImpactEntry] AS e
        WHERE (@TenantId IS NULL OR e.[TenantId] = @TenantId)
          AND ISNULL(e.[IsDeleted], 0) = 0
        ORDER BY e.[OccurredAtUtc] DESC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext30 nvarchar(200) = CONCAT(N'Params=TenantId=', @TenantId, N'; MaxResults=', @MaxResults, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails30 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext30, @ErrorDetails30, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
