-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PMScheduling_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PMScheduling rows. Backs IPMSchedulingRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_PMScheduling_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT PMSchedulingId, TenantId, SiteId, Status, EquipmentId, EquipmentDescription, PmTaskType, DueAtUtc, IsOverdue, EscalationPriorityCode, CompletionNotes, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[PMScheduling]
    WHERE IsDeleted = 0;
END
GO
