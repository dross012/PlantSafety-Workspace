-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ScoreCardsAggregate_GetAllActive.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted ScoreCardsAggregate rows. Backs
--                  IScoreCardsRepository.ListAsync, whose ScoreCardsSpecification predicate is
--                  applied in-memory over the candidate set returned here (the Domain interface
--                  carries no tenant/site scoping parameter to push down to SQL). Mirrors
--                  usp_Permits_GetAllActive.sql's identical precedent. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Analytics].[usp_ScoreCardsAggregate_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ScoreCardsId, TenantId, SiteId, Status, ScoreCardName, ReportingPeriod, KpiCategory, PerformanceIndex,
        GeneratedForSiteId, GeneratedAtUtc, PublishedByUserId, PublishedAtUtc, FinalizedByUserId, FinalizedAtUtc,
        ClosedByUserId, ClosedAtUtc, ArchivedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [Analytics].[ScoreCardsAggregate]
    WHERE IsDeleted = 0;
END
GO
