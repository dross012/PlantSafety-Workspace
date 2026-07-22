-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ShiftFatigue_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single ShiftFatigue row by identity. Backs IShiftFatigueRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_ShiftFatigue_GetById]
    @ShiftFatigueId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT ShiftFatigueId, TenantId, SiteId, Status, EmployeeId, ShiftPattern, ConsecutiveNightShifts, OvertimeHours, FatigueRiskScore, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[ShiftFatigue]
    WHERE ShiftFatigueId = @ShiftFatigueId AND IsDeleted = 0;
END
GO
