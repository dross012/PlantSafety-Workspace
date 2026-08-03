-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_List.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Paginated list of WorkOrder rows, optionally filtered by Status and/or a
--                  case-insensitive substring match on TechnicianName. Backs ListWorkOrdersQuery
--                  (v1/maintenance/work-orders). Carries a COUNT(*) OVER() window-function
--                  TotalCount on every row so the single-result-set DapperQueryExecutor.QueryAsync
--                  path can report paging metadata without a second round trip -- mirrors
--                  usp_Permit_List.sql. Deterministic ordering (DueAtUtc ASC, WorkOrderId) per
--                  Handbook §1.5. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_List]
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50) = NULL,
    @Technician NVARCHAR(200) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    WorkOrderId, WorkOrderCode, MachineId, MachineCode,
    ISNULL(TechnicianName, N'') AS TechnicianName, Priority, Status,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[WorkOrders]
WHERE TenantId = @TenantId
  AND IsDeleted = 0
  AND (@Status IS NULL OR Status = @Status)
  AND (@Technician IS NULL OR TechnicianName LIKE '%' + @Technician + '%')
ORDER BY DueAtUtc ASC, WorkOrderId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO
