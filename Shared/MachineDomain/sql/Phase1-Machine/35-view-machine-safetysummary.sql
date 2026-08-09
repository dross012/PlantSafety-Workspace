-- =============================================
-- Shared.MachineDomain -- Phase1-Machine
-- Script:        35-view-machine-safetysummary.sql
-- Purpose:       MachineDomain.vw_Machine_SafetySummary -- current-state
--                per-machine rollup with failure count and overdue-
--                maintenance flag.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [MachineDomain].[vw_Machine_SafetySummary]
AS
SELECT
    m.[MachineId],
    m.[Name],
    m.[MachineType],
    m.[FacilityId],
    m.[NextScheduledMaintenanceAtUtc],
    CASE WHEN m.[NextScheduledMaintenanceAtUtc] IS NOT NULL AND m.[NextScheduledMaintenanceAtUtc] <= SYSUTCDATETIME()
         THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [IsMaintenanceOverdue],
    COUNT(f.[MachineFailureRecordId]) AS [FailureCount],
    m.[SafetyScoreValue],
    m.[SafetyScoreComputedAtUtc]
FROM [MachineDomain].[Machine] AS m
LEFT JOIN [MachineDomain].[MachineFailureRecord] AS f
    ON f.[MachineId] = m.[MachineId] AND ISNULL(f.[IsDeleted], 0) = 0
WHERE ISNULL(m.[IsDeleted], 0) = 0
GROUP BY m.[MachineId], m.[Name], m.[MachineType], m.[FacilityId], m.[NextScheduledMaintenanceAtUtc],
         m.[SafetyScoreValue], m.[SafetyScoreComputedAtUtc];
GO
