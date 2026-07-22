-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.PredictiveMaintenance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PredictiveMaintenance_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[PredictiveMaintenanceReadModel],
--                  backing IPredictiveMaintenanceRepository.GetPredictiveMaintenanceAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_PredictiveMaintenance_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [PredictiveMaintenanceAlertId],
        [TenantId],
        [SiteId],
        [EquipmentId],
        [PredictedFailureType],
        [Status],
        [PredictedAtUtc],
        [ReviewedBy]
FROM [Operational].[PredictiveMaintenanceReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [PredictedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [PredictedAtUtc] DESC;
GO
