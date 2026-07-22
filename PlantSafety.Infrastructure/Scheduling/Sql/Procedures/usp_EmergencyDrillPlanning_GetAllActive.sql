-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_EmergencyDrillPlanning_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted EmergencyDrillPlanning rows. Backs IEmergencyDrillPlanningRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_EmergencyDrillPlanning_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT EmergencyDrillPlanningId, TenantId, SiteId, Status, DrillType, ShiftCode, ScheduledAtUtc, ParticipationCount, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[EmergencyDrillPlanning]
    WHERE IsDeleted = 0;
END
GO
