-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_Summary.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Work Order Summary card counts (Open / InProgress-bucket / Completed). Backs
--                  GetWorkOrderSummaryQuery (v1/maintenance/work-orders/summary). The UI's
--                  "InProgress" bucket rolls up Assigned + InProgress + OnHold, since those are all
--                  "not yet finished, not yet cancelled" states from the operator's point of view.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_Summary]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
    SUM(CASE WHEN Status = N'Open' THEN 1 ELSE 0 END) AS OpenCount,
    SUM(CASE WHEN Status IN (N'Assigned', N'InProgress', N'OnHold') THEN 1 ELSE 0 END) AS InProgressCount,
    SUM(CASE WHEN Status = N'Completed' THEN 1 ELSE 0 END) AS CompletedCount
FROM [Operational].[WorkOrders]
WHERE TenantId = @TenantId AND IsDeleted = 0;
GO
