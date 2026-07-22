-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrderPriority_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted WorkOrderPriority rows. Backs IWorkOrderPriorityRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_WorkOrderPriority_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT WorkOrderPriorityId, TenantId, SiteId, Status, PriorityCode, HazardDescription, DueByUtc, EscalatedAtUtc, ResolutionNotes, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[WorkOrderPriority]
    WHERE IsDeleted = 0;
END
GO
