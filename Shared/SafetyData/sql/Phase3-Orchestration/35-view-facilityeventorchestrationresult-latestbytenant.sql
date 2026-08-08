-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        35-view-facilityeventorchestrationresult-latestbytenant.sql
-- Purpose:       Creates Orchestration.vw_FacilityEventOrchestrationResult_LatestByTenant
--                against [PlantSafety.CoreDB] -- the single most-recent
--                (by OccurredAtUtc) orchestration result per TenantId,
--                mirroring the "most recent result for this tenant" shape
--                IFacilityEventOrchestrationResultStore.GetRecent(tenantId,
--                maxResults: 1) already computes in memory.
--
-- Determinism / soft-delete: no ORDER BY at the top level (views must not
-- embed one per validate-view-determinism; callers order the result), tie
-- broken deterministically on EventId when OccurredAtUtc ties (ROW_NUMBER
-- ORDER BY OccurredAtUtc DESC, EventId DESC). No IsDeleted filter -- this
-- table carries no soft-delete column (event-history rows are immutable,
-- append-only, never soft-deleted; see 20-usp-facilityeventorchestrationresult-record.sql's
-- idempotent-insert-only semantics).
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [Orchestration].[vw_FacilityEventOrchestrationResult_LatestByTenant]
AS
WITH Ranked AS (
    SELECT
        r.[EventId], r.[TenantId], r.[FacilityId], r.[SourceModule], r.[EventType], r.[OccurredAtUtc],
        r.[ComplianceRequiredElementsCount], r.[ComplianceCompletedElementsCount], r.[ComplianceMissingElementsCount],
        r.[CompliancePercentComplete], r.[ComplianceTier], r.[ComplianceNotes],
        r.[RiskHazardLevel], r.[RiskProgramLevel], r.[RiskRegulatorLevel], r.[RiskEventLevel], r.[RiskOverallLevel],
        r.[Category], r.[RequiresImmediateAttention], r.[Notification], r.[CreatedUtc],
        ROW_NUMBER() OVER (PARTITION BY r.[TenantId] ORDER BY r.[OccurredAtUtc] DESC, r.[EventId] DESC) AS RowNum
    FROM [Orchestration].[FacilityEventOrchestrationResult] AS r
    WHERE ISNULL(r.[IsDeleted], 0) = 0
)
SELECT
    EventId, TenantId, FacilityId, SourceModule, EventType, OccurredAtUtc,
    ComplianceRequiredElementsCount, ComplianceCompletedElementsCount, ComplianceMissingElementsCount,
    CompliancePercentComplete, ComplianceTier, ComplianceNotes,
    RiskHazardLevel, RiskProgramLevel, RiskRegulatorLevel, RiskEventLevel, RiskOverallLevel,
    Category, RequiresImmediateAttention, Notification, CreatedUtc
FROM Ranked
WHERE RowNum = 1;
GO
