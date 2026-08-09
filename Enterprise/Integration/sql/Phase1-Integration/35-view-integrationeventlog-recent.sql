-- =============================================
-- Enterprise.Integration -- Phase1-Integration
-- Script:        35-view-integrationeventlog-recent.sql
-- Purpose:       EnterpriseIntegration.vw_IntegrationEventLog_RecentBySystem
--                -- per-source-system event counts and most recent event
--                time, a common integration health-check query pattern.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [EnterpriseIntegration].[vw_IntegrationEventLog_RecentBySystem]
AS
SELECT
    l.[SourceSystem],
    COUNT(*) AS [EventCount],
    MAX(l.[OccurredAtUtc]) AS [MostRecentEventAtUtc]
FROM [EnterpriseIntegration].[IntegrationEventLog] AS l
WHERE ISNULL(l.[IsDeleted], 0) = 0
GROUP BY l.[SourceSystem];
GO
