-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_MachineHistory.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Paginated chronological maintenance history for a single machine across all its
--                  work orders. Backs GetMachineHistoryQuery
--                  (v1/maintenance/machines/{id}/history). Reads real data from
--                  [Operational].[MaintenanceEvents]. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_MachineHistory]
    @TenantId UNIQUEIDENTIFIER,
    @MachineId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    EventId, OccurredAtUtc, WorkOrderCode, ISNULL(TechnicianName, N'') AS TechnicianName, Summary, WorkOrderId,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[MaintenanceEvents]
WHERE TenantId = @TenantId AND MachineId = @MachineId
ORDER BY OccurredAtUtc DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO
