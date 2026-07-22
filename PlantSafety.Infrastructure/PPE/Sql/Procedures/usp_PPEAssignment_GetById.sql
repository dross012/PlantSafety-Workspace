-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPEAssignment_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PPEAssignment row by identity. Backs IPPEAssignmentRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PPE].[usp_PPEAssignment_GetById]
    @PPEAssignmentId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PPEAssignmentId, TenantId, SiteId, Status, WorkerId, JobRole, HazardZone, AssignedPpeItems, FitTestExpiresAtUtc, ReplacementReason, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PPE].[PPEAssignment]
    WHERE PPEAssignmentId = @PPEAssignmentId AND IsDeleted = 0;
END
GO
