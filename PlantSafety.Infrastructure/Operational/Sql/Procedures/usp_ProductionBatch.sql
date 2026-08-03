-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ProductionTracking
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ProductionBatch.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Insert/list procedures for Batches produced within a ProductionRun. Backs
--                  AddProductionBatchCommand, GetRunBatchesQuery, ListProductionBatchesQuery.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionBatch_Insert]
    @BatchId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
    @BatchCode NVARCHAR(50),
    @Status NVARCHAR(20),
    @StartedAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3)
AS
SET NOCOUNT ON;

INSERT INTO [Operational].[ProductionBatches]
    (BatchId, TenantId, ProductionRunId, BatchCode, Status, StartedAtUtc, CreatedAtUtc)
VALUES
    (@BatchId, @TenantId, @ProductionRunId, @BatchCode, @Status, @StartedAtUtc, @CreatedAtUtc);
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionBatch_ByRun]
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    b.BatchId, b.BatchCode, b.ProductionRunId, r.RunCode, r.LineName, b.Status, b.StartedAtUtc,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[ProductionBatches] b
INNER JOIN [Operational].[ProductionRuns] r ON r.ProductionRunId = b.ProductionRunId
WHERE b.TenantId = @TenantId
  AND b.ProductionRunId = @ProductionRunId
ORDER BY b.StartedAtUtc DESC, b.BatchId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionBatch_ListAll]
    @TenantId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    b.BatchId, b.BatchCode, b.ProductionRunId, r.RunCode, r.LineName, b.Status, b.StartedAtUtc,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[ProductionBatches] b
INNER JOIN [Operational].[ProductionRuns] r ON r.ProductionRunId = b.ProductionRunId
WHERE b.TenantId = @TenantId
ORDER BY b.StartedAtUtc DESC, b.BatchId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO
