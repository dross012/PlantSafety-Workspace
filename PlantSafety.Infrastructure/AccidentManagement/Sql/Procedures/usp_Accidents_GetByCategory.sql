-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Accidents_GetByCategory.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Shared read-model stored procedure backing all 4 AccidentManagement category
--                  Application-layer repositories (ISiteSecuringAndTriageRepository,
--                  IInvestigationRootCauseAnalysisRepository, ICorrectiveActionsCAPARepository,
--                  IRegulatoryComplianceDocumentationRepository). Each category repository
--                  method calls this same procedure with its own literal @Category value
--                  (matching InvestigationPhase.Code), avoiding 4 near-duplicate procedures.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_Accidents_GetByCategory]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3),
    @Category NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        AccidentId,
        TenantId,
        SiteId,
        AccidentTitle AS Title,
        Category,
        Status,
        ISNULL(ClosedAtUtc, OccurredAtUtc) AS EffectiveAtUtc,
        OwnerId
    FROM [AccidentManagement].[Accident]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND Category = @Category
      AND IsDeleted = 0
      AND ISNULL(ClosedAtUtc, OccurredAtUtc) >= @FromUtc
      AND ISNULL(ClosedAtUtc, OccurredAtUtc) <= @ToUtc
    ORDER BY ISNULL(ClosedAtUtc, OccurredAtUtc) DESC;
END
GO
