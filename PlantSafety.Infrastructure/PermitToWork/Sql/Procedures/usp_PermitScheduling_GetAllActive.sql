-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitScheduling_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PermitScheduling rows. Backs IPermitSchedulingRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitScheduling_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT PermitSchedulingId, TenantId, SiteId, Status, PermitNumber, ScheduledStartUtc, ScheduledEndUtc, ConflictDetected, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PermitToWork].[PermitScheduling]
    WHERE IsDeleted = 0;
END
GO
