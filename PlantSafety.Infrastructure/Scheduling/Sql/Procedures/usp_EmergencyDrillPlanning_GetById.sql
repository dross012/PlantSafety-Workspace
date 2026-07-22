-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EmergencyDrillPlanning_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single EmergencyDrillPlanning row by identity. Backs IEmergencyDrillPlanningRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_EmergencyDrillPlanning_GetById]
    @EmergencyDrillPlanningId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT EmergencyDrillPlanningId, TenantId, SiteId, Status, DrillType, ShiftCode, ScheduledAtUtc, ParticipationCount, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[EmergencyDrillPlanning]
    WHERE EmergencyDrillPlanningId = @EmergencyDrillPlanningId AND IsDeleted = 0;
END
GO
