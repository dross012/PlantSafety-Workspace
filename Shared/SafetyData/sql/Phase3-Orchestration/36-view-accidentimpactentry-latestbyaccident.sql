-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        36-view-accidentimpactentry-latestbyaccident.sql
-- Purpose:       Creates Orchestration.vw_AccidentImpactEntry_LatestByAccident
--                against [PlantSafety.CoreDB] -- the single most-recent (by
--                OccurredAtUtc) impact entry per AccidentId, mirroring the
--                "current computed impact for this accident" shape (the last
--                element of what IAccidentImpactStore.GetByAccidentId
--                already returns in oldest-first order).
--
-- Determinism / soft-delete: no ORDER BY at the top level; ties broken
-- deterministically on EventId. No IsDeleted filter (append-only rows).
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [Orchestration].[vw_AccidentImpactEntry_LatestByAccident]
AS
WITH Ranked AS (
    SELECT
        e.[EventId], e.[AccidentId], e.[TenantId], e.[FacilityId], e.[SourceModule], e.[EventType], e.[OccurredAtUtc],
        e.[ComplianceRequiredElementsCount], e.[ComplianceCompletedElementsCount], e.[ComplianceMissingElementsCount],
        e.[CompliancePercentComplete], e.[ComplianceTier], e.[ComplianceNotes],
        e.[RiskHazardLevel], e.[RiskProgramLevel], e.[RiskRegulatorLevel], e.[RiskEventLevel], e.[RiskOverallLevel],
        e.[Category], e.[RequiresImmediateAttention], e.[Notification], e.[CreatedUtc],
        ROW_NUMBER() OVER (PARTITION BY e.[AccidentId] ORDER BY e.[OccurredAtUtc] DESC, e.[EventId] DESC) AS RowNum
    FROM [Orchestration].[AccidentImpactEntry] AS e
    WHERE ISNULL(e.[IsDeleted], 0) = 0
)
SELECT
    EventId, AccidentId, TenantId, FacilityId, SourceModule, EventType, OccurredAtUtc,
    ComplianceRequiredElementsCount, ComplianceCompletedElementsCount, ComplianceMissingElementsCount,
    CompliancePercentComplete, ComplianceTier, ComplianceNotes,
    RiskHazardLevel, RiskProgramLevel, RiskRegulatorLevel, RiskEventLevel, RiskOverallLevel,
    Category, RequiresImmediateAttention, Notification, CreatedUtc
FROM Ranked
WHERE RowNum = 1;
GO
