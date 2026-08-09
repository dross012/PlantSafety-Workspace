-- =============================================
-- Enterprise.RulesEngine -- Phase1-RulesEngine
-- Script:        35-view-rule-matchrate.sql
-- Purpose:       EnterpriseRules.vw_Rule_MatchRate -- per-rule evaluation
--                count and match rate, a common rules-engine reporting need.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [EnterpriseRules].[vw_Rule_MatchRate]
AS
SELECT
    r.[RuleId], r.[Name], r.[RuleType], r.[IsEnabled],
    COUNT(l.[RuleEvaluationLogId]) AS [EvaluationCount],
    SUM(CASE WHEN l.[Matched] = 1 THEN 1 ELSE 0 END) AS [MatchCount]
FROM [EnterpriseRules].[Rule] AS r
LEFT JOIN [EnterpriseRules].[RuleEvaluationLog] AS l
    ON l.[RuleId] = r.[RuleId] AND ISNULL(l.[IsDeleted], 0) = 0
WHERE ISNULL(r.[IsDeleted], 0) = 0
GROUP BY r.[RuleId], r.[Name], r.[RuleType], r.[IsEnabled];
GO
