-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PMScheduling_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PMScheduling row by identity. Backs IPMSchedulingRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_PMScheduling_GetById]
    @PMSchedulingId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PMSchedulingId, TenantId, SiteId, Status, EquipmentId, EquipmentDescription, PmTaskType, DueAtUtc, IsOverdue, EscalationPriorityCode, CompletionNotes, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[PMScheduling]
    WHERE PMSchedulingId = @PMSchedulingId AND IsDeleted = 0;
END
GO
