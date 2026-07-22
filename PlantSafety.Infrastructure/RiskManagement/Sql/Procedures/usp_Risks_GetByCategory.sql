-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Risks_GetByCategory.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Shared read-model stored procedure backing all 11 RiskManagement category
--                  Application-layer repositories. Filters on HazardDomainCategory (the
--                  Infrastructure-layer hazard-topic classification tag -- see usp_Risk_Insert.sql
--                  header note), NOT the aggregate's own RiskLevel-derived Category, since the 11
--                  category folders (ChemicalEnvironmentalSafety, LotoConfinedSpaces, etc.) do not
--                  correspond to the 4-value RiskLevel taxonomy. Each category repository method
--                  calls this same procedure with its own literal @Category value.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [RiskMgmt].[usp_Risks_GetByCategory]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3),
    @Category NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RiskId,
        TenantId,
        SiteId,
        RiskTitle AS Title,
        HazardDomainCategory AS Category,
        Status,
        ISNULL(LastReviewedAtUtc, CreatedAtUtc) AS EffectiveAtUtc,
        OwnerId
    FROM [RiskMgmt].[Risk]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND HazardDomainCategory = @Category
      AND IsDeleted = 0
      AND ISNULL(LastReviewedAtUtc, CreatedAtUtc) >= @FromUtc
      AND ISNULL(LastReviewedAtUtc, CreatedAtUtc) <= @ToUtc
    ORDER BY ISNULL(LastReviewedAtUtc, CreatedAtUtc) DESC;
END
GO
