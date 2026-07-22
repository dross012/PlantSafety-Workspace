-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_HazardIsolationSequencing_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single HazardIsolationSequencing row by identity. Backs IHazardIsolationSequencingRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_HazardIsolationSequencing_GetById]
    @HazardIsolationSequencingId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT HazardIsolationSequencingId, TenantId, SiteId, Status, HazardZone, TaskType, WindowStartUtc, WindowEndUtc, ConflictDetected, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [Scheduling].[HazardIsolationSequencing]
    WHERE HazardIsolationSequencingId = @HazardIsolationSequencingId AND IsDeleted = 0;
END
GO
