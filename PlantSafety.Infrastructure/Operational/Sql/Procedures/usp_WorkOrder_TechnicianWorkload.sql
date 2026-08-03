-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_WorkOrder_TechnicianWorkload.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Technician workload cards, aggregated from currently-assigned (non-terminal)
--                  work orders grouped by technician. WorkloadPercent is computed against a fixed
--                  platform-default capacity of 8 concurrently assigned work orders -- no
--                  per-technician capacity configuration exists yet (documented assumption).
--                  Backs GetTechnicianWorkloadQuery (v1/maintenance/technicians/workload).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_WorkOrder_TechnicianWorkload]
    @TenantId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
DECLARE @CapacityPerTechnician DECIMAL(10, 2) = 8.0;

WITH Workload AS (
    SELECT
        TechnicianId,
        MAX(TechnicianName) AS TechnicianName,
        COUNT(*) AS AssignedCount
    FROM [Operational].[WorkOrders]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND TechnicianId IS NOT NULL
      AND Status IN (N'Assigned', N'InProgress', N'OnHold')
    GROUP BY TechnicianId
)
SELECT
    TechnicianId,
    TechnicianName,
    AssignedCount,
    CAST(ROUND(AssignedCount / @CapacityPerTechnician * 100.0, 0) AS INT) AS WorkloadPercent,
    CASE
        WHEN AssignedCount / @CapacityPerTechnician >= 1.0 THEN N'Overloaded'
        WHEN AssignedCount / @CapacityPerTechnician >= 0.75 THEN N'Busy'
        ELSE N'Available'
    END AS AvailabilityStatus,
    COUNT(*) OVER() AS TotalCount
FROM Workload
ORDER BY AssignedCount DESC, TechnicianName ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO
