-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.RootCauseAnalysis
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_RootCauseAnalysis_GetRootCauseAnalyses.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IRootCauseAnalysisRepository.GetRootCauseAnalysesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_RootCauseAnalysis_GetRootCauseAnalyses]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        AnalysisId,
        TenantId,
        SiteId,
        RelatedEventId,
        Methodology,
        Status,
        CompletedAtUtc,
        LeadAnalyst
    FROM [SafetyEvents].[RootCauseAnalysis]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND CompletedAtUtc >= @FromUtc
      AND CompletedAtUtc <= @ToUtc
    ORDER BY CompletedAtUtc DESC;
END
GO
