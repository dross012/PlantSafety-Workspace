-- =============================================
-- Shared.SafetyData -- Phase4-Facilities
-- Script:        35-view-facilityprofile-compliancesummary.sql
-- Purpose:       Creates Facilities.vw_FacilityProfile_ComplianceSummary
--                against [PlantSafety.CoreDB] -- a single denormalized read
--                model joining [Facilities].[FacilityProfile] +
--                [Facilities].[FacilityComplianceProfile] +
--                [Facilities].[FacilityDashboardMetadata], mirroring what
--                FacilityProfile.ComplianceProfile already exposes in
--                memory as a single nested object -- the "current row per
--                facility" shape called for in this folder's task brief,
--                applied here as a 1:1:1 identity+scorecard+dashboard join
--                rather than a "latest of many" ranking (all three tables
--                are current-state snapshots, not event-history: see
--                10-indexes.sql's header for why).
--
-- Determinism / soft-delete: no ORDER BY (callers order the result),
-- explicit column list, no nondeterministic functions. IsDeleted filtered
-- on all three base tables (per validate-soft-delete-filtering).
--
-- LEFT JOIN rationale: a FacilityProfile may exist before its compliance
-- scorecard and/or dashboard metadata have ever been upserted (each is
-- computed and persisted by its own independent Upsert call), so this view
-- must not silently drop such rows.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [Facilities].[vw_FacilityProfile_ComplianceSummary]
AS
SELECT
    f.[FacilityId],
    f.[Name]                       AS [FacilityName],
    f.[IndustryName],
    f.[Location],
    f.[Flags],
    f.[UpdatedUtc]                 AS [FacilityProfileUpdatedUtc],
    c.[RequiredElementsCount],
    c.[CompletedElementsCount],
    c.[MissingElementsCount],
    c.[ComplianceScorePercent],
    c.[RiskLevel]                  AS [ComplianceRiskLevel],
    c.[UpdatedUtc]                 AS [ComplianceProfileUpdatedUtc],
    d.[KeyIndicatorsJson],
    d.[RequiredWidgetsJson],
    d.[RequiredChartsJson],
    d.[RequiredAlertsJson],
    d.[UpdatedUtc]                 AS [DashboardMetadataUpdatedUtc]
FROM [Facilities].[FacilityProfile] AS f
LEFT JOIN [Facilities].[FacilityComplianceProfile] AS c
    ON c.[FacilityId] = f.[FacilityId] AND ISNULL(c.[IsDeleted], 0) = 0
LEFT JOIN [Facilities].[FacilityDashboardMetadata] AS d
    ON d.[FacilityId] = f.[FacilityId] AND ISNULL(d.[IsDeleted], 0) = 0
WHERE ISNULL(f.[IsDeleted], 0) = 0;
GO
