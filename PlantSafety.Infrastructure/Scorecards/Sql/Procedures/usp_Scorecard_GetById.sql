-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scorecards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Scorecard_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single Scorecard aggregate root row by identity. Backs
--                  IScorecardRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scorecards].[usp_Scorecard_GetById]
    @ScorecardId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ScorecardId, TenantId, SiteId, Status, ScorecardTitle, ScorecardVersion, Category, PeriodType,
        PeriodStartUtc, PeriodEndUtc, OwnerId, PublishedAtUtc, DistributionCount, ReviewRequired,
        ReviewCount, CreatedAtUtc, CreatedBy
    FROM [Scorecards].[Scorecard]
    WHERE ScorecardId = @ScorecardId AND IsDeleted = 0;
END
GO
