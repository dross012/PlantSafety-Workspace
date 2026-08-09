-- =============================================
-- Shared.EmployeeDomain -- Phase1-Employee
-- Script:        35-view-employee-safetysummary.sql
-- Purpose:       EmployeeDomain.vw_Employee_SafetySummary -- current-state
--                per-employee rollup (certification/exposure counts +
--                injury/score snapshot), mirroring Shared.SafetyData's
--                "current row per entity" view shape.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER VIEW [EmployeeDomain].[vw_Employee_SafetySummary]
AS
SELECT
    e.[EmployeeId],
    e.[Name],
    e.[Role],
    e.[InjuryTotalCount],
    e.[InjuryLastAtUtc],
    e.[SafetyScoreValue],
    e.[SafetyScoreComputedAtUtc],
    COUNT(DISTINCT c.[EmployeeCertificationId]) AS [ActiveCertificationCount],
    COUNT(DISTINCT x.[EmployeeExposureRecordId]) AS [ExposureRecordCount]
FROM [EmployeeDomain].[Employee] AS e
LEFT JOIN [EmployeeDomain].[EmployeeCertification] AS c
    ON c.[EmployeeId] = e.[EmployeeId] AND ISNULL(c.[IsDeleted], 0) = 0
    AND (c.[ExpiresAtUtc] IS NULL OR c.[ExpiresAtUtc] > SYSUTCDATETIME())
LEFT JOIN [EmployeeDomain].[EmployeeExposureRecord] AS x
    ON x.[EmployeeId] = e.[EmployeeId] AND ISNULL(x.[IsDeleted], 0) = 0
WHERE ISNULL(e.[IsDeleted], 0) = 0
GROUP BY e.[EmployeeId], e.[Name], e.[Role], e.[InjuryTotalCount], e.[InjuryLastAtUtc],
         e.[SafetyScoreValue], e.[SafetyScoreComputedAtUtc];
GO
