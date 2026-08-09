-- =============================================
-- AI.EnterpriseOrchestration -- Phase1-EnterpriseOrchestrationLog
-- Script:        35-view-enterpriseorchestrationlog-latest.sql
-- Purpose:       AIEnterpriseOrchestration.vw_EnterpriseOrchestrationLog_Latest
--                -- the single most recent enterprise-wide orchestration run.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [AIEnterpriseOrchestration].[vw_EnterpriseOrchestrationLog_Latest]
AS
SELECT TOP (1)
    [EnterpriseOrchestrationLogId], [FacilityCount], [FacilitiesRequiringEvacuationJson],
    [FacilitiesWithRegulatoryExposureJson], [FacilitiesWithAnomalousPredictionsJson],
    [HighestSeverityCode], [AlertsJson], [GeneratedAtUtc]
FROM [AIEnterpriseOrchestration].[EnterpriseOrchestrationLog]
WHERE ISNULL([IsDeleted], 0) = 0
ORDER BY [GeneratedAtUtc] DESC;
GO
