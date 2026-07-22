-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrderPriority_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single WorkOrderPriority row by identity. Backs IWorkOrderPriorityRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_WorkOrderPriority_GetById]
    @WorkOrderPriorityId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT WorkOrderPriorityId, TenantId, SiteId, Status, PriorityCode, HazardDescription, DueByUtc, EscalatedAtUtc, ResolutionNotes, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[WorkOrderPriority]
    WHERE WorkOrderPriorityId = @WorkOrderPriorityId AND IsDeleted = 0;
END
GO
