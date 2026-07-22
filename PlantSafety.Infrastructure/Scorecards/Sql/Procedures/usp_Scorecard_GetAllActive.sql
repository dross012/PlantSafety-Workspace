-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scorecards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Scorecard_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted Scorecard aggregate root rows. Backs
--                  IScorecardRepository.ListAsync, whose ScorecardSpecification predicate is
--                  applied in-memory over the candidate set returned here. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scorecards].[usp_Scorecard_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ScorecardId, TenantId, SiteId, Status, ScorecardTitle, ScorecardVersion, Category, PeriodType,
        PeriodStartUtc, PeriodEndUtc, OwnerId, PublishedAtUtc, DistributionCount, ReviewRequired,
        ReviewCount, CreatedAtUtc, CreatedBy
    FROM [Scorecards].[Scorecard]
    WHERE IsDeleted = 0;
END
GO
