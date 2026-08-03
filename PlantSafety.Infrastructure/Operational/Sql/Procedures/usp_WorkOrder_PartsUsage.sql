-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_PartsUsage.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Parts consumed by a work order. Backs GetWorkOrderPartsUsageQuery
--                  (v1/maintenance/work-orders/{id}/parts). See WorkOrderPartUsage.sql remarks --
--                  no writer populates this table yet, so this returns an empty set until a future
--                  "RecordWorkOrderPartUsage" command is added. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_PartsUsage]
    @TenantId UNIQUEIDENTIFIER,
    @WorkOrderId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
    u.PartId, u.PartNumber, u.PartName, u.QuantityUsed, u.WasAvailable,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[WorkOrderPartUsage] u
INNER JOIN [Operational].[WorkOrders] w ON w.WorkOrderId = u.WorkOrderId
WHERE w.TenantId = @TenantId AND w.WorkOrderId = @WorkOrderId AND w.IsDeleted = 0;
GO
