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

BEGIN TRY

INSERT INTO [Operational].[ProductionBatches]
    (BatchId, TenantId, ProductionRunId, BatchCode, Status, StartedAtUtc, CreatedAtUtc)
VALUES
    (@BatchId, @TenantId, @ProductionRunId, @BatchCode, @Status, @StartedAtUtc, @CreatedAtUtc);

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ProductionBatches', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'BatchId=', @BatchId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'BatchId=', @BatchId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
END CATCH
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
