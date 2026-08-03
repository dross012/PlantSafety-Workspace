-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ProductionTracking
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ProductionRun_Write.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Insert/GetById/Update procedures for the ProductionRun aggregate write-side
--                  repository. Update carries an ETag optimistic-concurrency check via
--                  MIN_ACTIVE_ROWVERSION-style comparison against RowVersion, mirroring
--                  usp_WorkOrder_Update.sql's pattern. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRun_Insert]
    @ProductionRunId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(20),
    @RunCode NVARCHAR(50),
    @LineId UNIQUEIDENTIFIER,
    @LineName NVARCHAR(200),
    @MachineId UNIQUEIDENTIFIER,
    @OperatorName NVARCHAR(200) = NULL,
    @IdealCycleTimeSeconds FLOAT,
    @StartedAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

INSERT INTO [Operational].[ProductionRuns]
    (ProductionRunId, TenantId, SiteId, Status, RunCode, LineId, LineName, MachineId, OperatorName,
     IdealCycleTimeSeconds, GoodUnitCount, ScrapUnitCount, DowntimeMinutesTotal,
     StartedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc)
VALUES
    (@ProductionRunId, @TenantId, @SiteId, @Status, @RunCode, @LineId, @LineName, @MachineId, @OperatorName,
     @IdealCycleTimeSeconds, 0, 0, 0,
     @StartedAtUtc, @CreatedAtUtc, @CreatedBy, @CreatedAtUtc);
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRun_GetById]
    @ProductionRunId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
    ProductionRunId, TenantId, SiteId, Status, RunCode, LineId, LineName, MachineId, OperatorName,
    IdealCycleTimeSeconds, GoodUnitCount, ScrapUnitCount, DowntimeMinutesTotal,
    StartedAtUtc, EndedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc,
    CONVERT(VARCHAR(20), RowVersion, 1) AS ETag
FROM [Operational].[ProductionRuns]
WHERE ProductionRunId = @ProductionRunId
  AND TenantId = @TenantId
  AND IsDeleted = 0;
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRun_Update]
    @ProductionRunId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @GoodUnitCount INT,
    @ScrapUnitCount INT,
    @DowntimeMinutesTotal FLOAT,
    @EndedAtUtc DATETIME2(3) = NULL,
    @UpdatedAtUtc DATETIME2(3),
    @ETag VARCHAR(20) = NULL
AS
SET NOCOUNT ON;

UPDATE [Operational].[ProductionRuns]
SET
    Status = @Status,
    GoodUnitCount = @GoodUnitCount,
    ScrapUnitCount = @ScrapUnitCount,
    DowntimeMinutesTotal = @DowntimeMinutesTotal,
    EndedAtUtc = @EndedAtUtc,
    UpdatedAtUtc = @UpdatedAtUtc
WHERE ProductionRunId = @ProductionRunId
  AND TenantId = @TenantId
  AND IsDeleted = 0
  AND (@ETag IS NULL OR CONVERT(VARCHAR(20), RowVersion, 1) = @ETag);

SELECT @@ROWCOUNT;
GO
