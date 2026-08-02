-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitScheduling_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PermitScheduling row by identity. Backs IPermitSchedulingRepository.GetByIdAsync.
--                  Additive-only evolution.
--  UPDATED:        2026-07-29 -- Projects RowVersion as a string ETag (Gap 2 remediation),
--                  matching the ProcessManagement.usp_Process_GetById convention.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitScheduling_GetById]
    @PermitSchedulingId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PermitSchedulingId, TenantId, SiteId, Status, PermitNumber, ScheduledStartUtc, ScheduledEndUtc, ConflictDetected, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PermitToWork].[PermitScheduling]
    WHERE PermitSchedulingId = @PermitSchedulingId AND IsDeleted = 0;
END
GO
