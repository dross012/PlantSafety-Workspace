-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.EmergencyManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_EmergencyManagementItems_GetEmergencies.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IEmergencyManagementRepository.GetEmergenciesAsync. Named
--                  usp_EmergencyManagementItems_* (not usp_EmergencyManagement_*) to
--                  avoid colliding with the unrelated procedures already referenced
--                  by the separate Features-tree EmergencyManagementRepository in
--                  this same module.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_EmergencyManagementItems_GetEmergencies]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        EmergencyId,
        TenantId,
        SiteId,
        EmergencyType,
        Severity,
        Status,
        ReportedAtUtc,
        ReportedBy
    FROM [SafetyEvents].[EmergencyManagementItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ReportedAtUtc >= @FromUtc
      AND ReportedAtUtc <= @ToUtc
    ORDER BY ReportedAtUtc DESC;
END
GO
