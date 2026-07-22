-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ShiftFatigue_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted ShiftFatigue rows. Backs IShiftFatigueRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_ShiftFatigue_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT ShiftFatigueId, TenantId, SiteId, Status, EmployeeId, ShiftPattern, ConsecutiveNightShifts, OvertimeHours, FatigueRiskScore, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[ShiftFatigue]
    WHERE IsDeleted = 0;
END
GO
