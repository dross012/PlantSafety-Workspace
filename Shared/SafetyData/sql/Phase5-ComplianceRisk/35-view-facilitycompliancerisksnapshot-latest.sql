-- =============================================
-- Shared.SafetyData -- Phase5-ComplianceRisk
-- Script:        35-view-facilitycompliancerisksnapshot-latest.sql
-- Purpose:       Creates ComplianceRisk.vw_FacilityComplianceRiskSnapshot_Latest
--                against [PlantSafety.CoreDB] -- the "latest compliance/risk
--                snapshot per facility" view called for in this folder's
--                task brief: one row per FacilityId, combining that
--                facility's single most recent ComplianceScore, RiskScore,
--                ComplianceReadiness, and RiskReadiness rows (each
--                independently ranked latest-first per table, then joined
--                on FacilityId) -- mirroring what a caller would get by
--                independently calling usp_ComplianceScore_GetLatestByFacility
--                / usp_RiskScore_GetLatestByFacility /
--                usp_ComplianceReadiness_GetLatestByFacility /
--                usp_RiskReadiness_GetLatestByFacility for the same
--                FacilityId and combining the results, exposed as a single
--                denormalized read model per this folder's "a view per
--                subsystem" requirement (covering all four tables in one
--                object rather than four near-duplicate single-table
--                views).
--
-- Determinism / soft-delete: no ORDER BY at the top level; ties within a
-- single source table broken deterministically on FacilityId (there is only
-- one row per FacilityId per source table already, by definition of "the
-- latest one", so no further tiebreaker is required). No IsDeleted filter
-- (append-only rows, never soft-deleted).
--
-- LEFT JOIN rationale: a facility may have a recorded ComplianceScore
-- before its first RiskReadiness has ever been computed (or vice versa) --
-- an orchestration cycle for one facet may run before another -- so this
-- view must not silently drop a facility missing one of the four snapshots.
-- The anchor (leftmost) table is ComplianceScore purely because it is
-- listed first in this folder's task brief; any facility that has recorded
-- at least one of the four snapshot kinds will still be missing from this
-- view's output only if it has never recorded a ComplianceScore -- callers
-- needing the full union across all four tables independently should query
-- the four GetLatestByFacility procedures directly instead.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [ComplianceRisk].[vw_FacilityComplianceRiskSnapshot_Latest]
AS
WITH LatestComplianceScore AS (
    SELECT
        s.[FacilityId], s.[ComputedAtUtc], s.[RequiredElementsCount], s.[CompletedElementsCount],
        s.[MissingElementsCount], s.[PercentComplete], s.[ComplianceTier], s.[Notes],
        ROW_NUMBER() OVER (PARTITION BY s.[FacilityId] ORDER BY s.[ComputedAtUtc] DESC) AS RowNum
    FROM [ComplianceRisk].[ComplianceScore] AS s
    WHERE ISNULL(s.[IsDeleted], 0) = 0
),
LatestRiskScore AS (
    SELECT
        s.[FacilityId], s.[ComputedAtUtc], s.[HazardRiskLevel], s.[ProgramRiskLevel],
        s.[RegulatorRiskLevel], s.[EventRiskLevel], s.[OverallRiskLevel],
        ROW_NUMBER() OVER (PARTITION BY s.[FacilityId] ORDER BY s.[ComputedAtUtc] DESC) AS RowNum
    FROM [ComplianceRisk].[RiskScore] AS s
    WHERE ISNULL(s.[IsDeleted], 0) = 0
),
LatestComplianceReadiness AS (
    SELECT
        r.[FacilityId], r.[ComputedAtUtc], r.[AuditReady], r.[TrainingReady], r.[PermitReady], r.[SopReady], r.[OverallReadinessPercent],
        ROW_NUMBER() OVER (PARTITION BY r.[FacilityId] ORDER BY r.[ComputedAtUtc] DESC) AS RowNum
    FROM [ComplianceRisk].[ComplianceReadiness] AS r
    WHERE ISNULL(r.[IsDeleted], 0) = 0
),
LatestRiskReadiness AS (
    SELECT
        r.[FacilityId], r.[ComputedAtUtc], r.[HazardResponseReady], r.[EmergencyResponseReady], r.[EventRoutingReady], r.[OverallReadinessPercent],
        ROW_NUMBER() OVER (PARTITION BY r.[FacilityId] ORDER BY r.[ComputedAtUtc] DESC) AS RowNum
    FROM [ComplianceRisk].[RiskReadiness] AS r
    WHERE ISNULL(r.[IsDeleted], 0) = 0
)
SELECT
    cs.[FacilityId],
    cs.[ComputedAtUtc]                        AS [ComplianceScoreComputedAtUtc],
    cs.[RequiredElementsCount],
    cs.[CompletedElementsCount],
    cs.[MissingElementsCount],
    cs.[PercentComplete]                      AS [CompliancePercentComplete],
    cs.[ComplianceTier],
    cs.[Notes]                                AS [ComplianceNotes],
    rs.[ComputedAtUtc]                        AS [RiskScoreComputedAtUtc],
    rs.[HazardRiskLevel],
    rs.[ProgramRiskLevel],
    rs.[RegulatorRiskLevel],
    rs.[EventRiskLevel],
    rs.[OverallRiskLevel],
    cr.[ComputedAtUtc]                        AS [ComplianceReadinessComputedAtUtc],
    cr.[AuditReady],
    cr.[TrainingReady],
    cr.[PermitReady],
    cr.[SopReady],
    cr.[OverallReadinessPercent]              AS [ComplianceOverallReadinessPercent],
    rr.[ComputedAtUtc]                        AS [RiskReadinessComputedAtUtc],
    rr.[HazardResponseReady],
    rr.[EmergencyResponseReady],
    rr.[EventRoutingReady],
    rr.[OverallReadinessPercent]              AS [RiskOverallReadinessPercent]
FROM LatestComplianceScore AS cs
LEFT JOIN LatestRiskScore AS rs ON rs.[FacilityId] = cs.[FacilityId] AND rs.RowNum = 1
LEFT JOIN LatestComplianceReadiness AS cr ON cr.[FacilityId] = cs.[FacilityId] AND cr.RowNum = 1
LEFT JOIN LatestRiskReadiness AS rr ON rr.[FacilityId] = cs.[FacilityId] AND rr.RowNum = 1
WHERE cs.RowNum = 1;
GO
