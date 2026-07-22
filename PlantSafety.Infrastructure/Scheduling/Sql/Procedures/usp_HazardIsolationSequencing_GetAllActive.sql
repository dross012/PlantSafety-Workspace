-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_HazardIsolationSequencing_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted HazardIsolationSequencing rows. Backs IHazardIsolationSequencingRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_HazardIsolationSequencing_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT HazardIsolationSequencingId, TenantId, SiteId, Status, HazardZone, TaskType, WindowStartUtc, WindowEndUtc, ConflictDetected, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[HazardIsolationSequencing]
    WHERE IsDeleted = 0;
END
GO
