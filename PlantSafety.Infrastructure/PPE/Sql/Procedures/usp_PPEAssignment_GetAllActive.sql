-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPEAssignment_GetAllActive.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PPEAssignment rows. Backs IPPEAssignmentRepository.ListAsync
--                  (specification is evaluated in-memory over this candidate set,
--                  since the interface carries no tenant/site scoping parameter).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PPE].[usp_PPEAssignment_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT PPEAssignmentId, TenantId, SiteId, Status, WorkerId, JobRole, HazardZone, AssignedPpeItems, FitTestExpiresAtUtc, ReplacementReason, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PPE].[PPEAssignment]
    WHERE IsDeleted = 0;
END
GO
