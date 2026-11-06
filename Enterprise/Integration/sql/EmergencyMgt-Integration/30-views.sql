-- =============================================
-- Enterprise.Integration -- EmergencyMgt-Integration
-- Script:  30-views.sql
-- Purpose: Read-only views backing the PlantSafety API proxy endpoints
--          (/integration/emergency/summary, /lifecycle, /corrective-actions, /readiness, /kpis).
--          Deterministic, explicit column lists, no nondeterministic functions in projections.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Read-only emergency event summary (backs /integration/emergency/summary)
CREATE OR ALTER VIEW [EmergencyMgtIntegration].[vwEmergencySummary]
AS
    SELECT
        SourceEmergencyEventId, EmergencyTypeName, Severity, StatusCode, IsStatusTerminal,
        Location, InitiatedBy, StartedUtc, EndedUtc, ReceivedUtc
    FROM [EmergencyMgtIntegration].[EmergencySummaryInbound];
GO

-- Read-only lifecycle transition history (backs /integration/emergency/lifecycle)
CREATE OR ALTER VIEW [EmergencyMgtIntegration].[vwEmergencyLifecycle]
AS
    SELECT
        SourceEmergencyEventId, FromStatus, ToStatus, TransitionUtc, ReceivedUtc
    FROM [EmergencyMgtIntegration].[EmergencyLifecycleInbound];
GO

-- Read-only corrective action status (backs /integration/emergency/corrective-actions)
CREATE OR ALTER VIEW [EmergencyMgtIntegration].[vwEmergencyCorrectiveActions]
AS
    SELECT
        SourceCorrectiveActionId, SourceEmergencyEventId, Description, AssignedTo, DueUtc,
        StatusCode, ReceivedUtc
    FROM [EmergencyMgtIntegration].[EmergencyCorrectiveActionInbound];
GO

-- Read-only readiness score feed (backs /integration/emergency/readiness)
CREATE OR ALTER VIEW [EmergencyMgtIntegration].[vwEmergencyReadiness]
AS
    SELECT
        SourceReadinessScoreId, OverallScore, FactorScoresJson, ComputedUtc, ReceivedUtc
    FROM [EmergencyMgtIntegration].[EmergencyReadinessInbound];
GO

-- Read-only composite KPI rollup (backs /integration/emergency/kpis) -- one row per known
-- emergency type, joining summary counts, terminal/open split, and the single most recent
-- readiness score (readiness is module-wide, not per-type, so it is repeated across rows by
-- design, not duplicated data-entry).
CREATE OR ALTER VIEW [EmergencyMgtIntegration].[vwEmergencyKPIs]
AS
    SELECT
        s.EmergencyTypeName,
        COUNT(*) AS EventCount,
        SUM(CASE WHEN s.IsStatusTerminal = 0 THEN 1 ELSE 0 END) AS OpenCount,
        SUM(CASE WHEN s.IsStatusTerminal = 1 THEN 1 ELSE 0 END) AS ClosedCount,
        (SELECT TOP 1 r.OverallScore FROM [EmergencyMgtIntegration].[EmergencyReadinessInbound] r ORDER BY r.ComputedUtc DESC) AS LatestOverallReadinessScore,
        (SELECT TOP 1 r.ComputedUtc FROM [EmergencyMgtIntegration].[EmergencyReadinessInbound] r ORDER BY r.ComputedUtc DESC) AS LatestReadinessComputedUtc
    FROM [EmergencyMgtIntegration].[EmergencySummaryInbound] s
    GROUP BY s.EmergencyTypeName;
GO
