-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PreventiveMaintenance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PreventiveMaintenance_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[PreventiveMaintenanceReadModel],
--                  backing IPreventiveMaintenanceRepository.GetPreventiveMaintenanceAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_PreventiveMaintenance_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PreventiveMaintenanceTaskId],
        [TenantId],
        [SiteId],
        [EquipmentId],
        [TaskDescription],
        [Status],
        [ScheduledAtUtc],
        [CompletedBy]
FROM [Operational].[PreventiveMaintenanceReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
GO
