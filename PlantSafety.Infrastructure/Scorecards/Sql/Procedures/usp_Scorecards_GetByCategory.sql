-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scorecards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Scorecards_GetByCategory.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Shared read-model stored procedure backing all 6 Scorecards category
--                  Application-layer repositories. Three folders (LaggingIndicators,
--                  LeadingIndicators, CulturalPrecursorMetrics) map 1:1 onto the Scorecard
--                  aggregate's own IndicatorCategory taxonomy and pass a literal @Category value.
--                  The other three (ReportingDistribution, ScorecardDataSources,
--                  ScorecardStructureVisualization) are cross-cutting scorecard facets that do
--                  not correspond to any IndicatorCategory value -- for those, the C# repository
--                  passes @Category = NULL, meaning "no category filter, return all scorecards
--                  for the tenant/site/date range" (assumption, flagged in
--                  ScorecardsCategoryDapperRepository.cs remarks).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scorecards].[usp_Scorecards_GetByCategory]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3),
    @Category NVARCHAR(30) = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ScorecardId,
        TenantId,
        SiteId,
        ScorecardTitle AS Title,
        Category,
        Status,
        ISNULL(PublishedAtUtc, CreatedAtUtc) AS EffectiveAtUtc,
        OwnerId
    FROM [Scorecards].[Scorecard]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND (@Category IS NULL OR Category = @Category)
      AND IsDeleted = 0
      AND ISNULL(PublishedAtUtc, CreatedAtUtc) >= @FromUtc
      AND ISNULL(PublishedAtUtc, CreatedAtUtc) <= @ToUtc
    ORDER BY ISNULL(PublishedAtUtc, CreatedAtUtc) DESC;
END
GO
