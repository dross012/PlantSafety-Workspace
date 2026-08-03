-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.WorkOrders
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_Part_List.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Paginated list of Part inventory rows. Backs ListPartsInventoryQuery
--                  (v1/maintenance/parts). Deterministic ordering (PartNumber ASC). Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_Part_List]
    @TenantId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    PartId, PartNumber, PartName, QuantityOnHand, ReorderThreshold, Location,
    CAST(CASE WHEN QuantityOnHand <= ReorderThreshold THEN 1 ELSE 0 END AS BIT) AS NeedsReorder,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[Parts]
WHERE TenantId = @TenantId AND IsDeleted = 0
ORDER BY PartNumber ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO
