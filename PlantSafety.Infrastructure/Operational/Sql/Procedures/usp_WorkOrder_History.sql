-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_History.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Chronological event history for a single work order. Backs
--                  GetWorkOrderHistoryQuery (v1/maintenance/work-orders/{id}/history). Reads real
--                  data from [Operational].[MaintenanceEvents], populated by
--                  WorkOrderDapperRepository.AddAsync/UpdateAsync alongside the outbox write.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_History]
    @TenantId UNIQUEIDENTIFIER,
    @WorkOrderId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
    EventId, OccurredAtUtc, WorkOrderCode, ISNULL(TechnicianName, N'') AS TechnicianName, Summary, WorkOrderId,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[MaintenanceEvents]
WHERE TenantId = @TenantId AND WorkOrderId = @WorkOrderId
ORDER BY OccurredAtUtc DESC;
GO
