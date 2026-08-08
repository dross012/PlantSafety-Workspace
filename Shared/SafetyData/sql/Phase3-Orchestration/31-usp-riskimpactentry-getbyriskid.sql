-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        31-usp-riskimpactentry-getbyriskid.sql
-- Purpose:       Creates Orchestration.usp_RiskImpactEntry_GetByRiskId
--                against [PlantSafety.CoreDB] -- mirrors IRiskImpactStore.
--                GetByRiskId(Guid riskId): every recorded entry for the
--                given RiskId, oldest first (OccurredAtUtc ASC) -- its full
--                observed lifecycle.
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

CREATE OR ALTER PROCEDURE [Orchestration].[usp_RiskImpactEntry_GetByRiskId]
    @RiskId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @RiskId IS NULL
            THROW 50001, N'RiskId is required.', 1;

        SELECT
            e.[EventId], e.[RiskId], e.[TenantId], e.[FacilityId], e.[SourceModule], e.[EventType], e.[OccurredAtUtc],
            e.[ComplianceRequiredElementsCount], e.[ComplianceCompletedElementsCount], e.[ComplianceMissingElementsCount],
            e.[CompliancePercentComplete], e.[ComplianceTier], e.[ComplianceNotes],
            e.[RiskHazardLevel], e.[RiskProgramLevel], e.[RiskRegulatorLevel], e.[RiskEventLevel], e.[RiskOverallLevel],
            e.[Category], e.[RequiresImmediateAttention], e.[Notification],
            e.[CreatedUtc], e.[CreatedBy], e.[UpdatedUtc], e.[UpdatedBy]
        FROM [Orchestration].[RiskImpactEntry] AS e
        WHERE e.[RiskId] = @RiskId
          AND ISNULL(e.[IsDeleted], 0) = 0
        ORDER BY e.[OccurredAtUtc] ASC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext31 nvarchar(200) = CONCAT(N'Params=RiskId=', @RiskId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails31 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext31, @ErrorDetails31, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
