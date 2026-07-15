-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentManagementItems_GetIncidents.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IIncidentManagementRepository.GetIncidentsAsync. Named
--                  usp_IncidentManagementItems_* (not usp_IncidentManagement_*) to
--                  avoid colliding with the unrelated procedures already referenced
--                  by the separate Features-tree IncidentManagementRepository in
--                  this same module.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentManagementItems_GetIncidents]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        IncidentId,
        TenantId,
        SiteId,
        IncidentType,
        Severity,
        Status,
        OccurredAtUtc,
        ReportedBy
    FROM [SafetyEvents].[IncidentManagementItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND OccurredAtUtc >= @FromUtc
      AND OccurredAtUtc <= @ToUtc
    ORDER BY OccurredAtUtc DESC;
END
GO
