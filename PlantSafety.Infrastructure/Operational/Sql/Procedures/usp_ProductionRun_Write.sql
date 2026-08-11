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

BEGIN TRY

INSERT INTO [Operational].[ProductionRuns]
    (ProductionRunId, TenantId, SiteId, Status, RunCode, LineId, LineName, MachineId, OperatorName,
     IdealCycleTimeSeconds, GoodUnitCount, ScrapUnitCount, DowntimeMinutesTotal,
     StartedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc)
VALUES
    (@ProductionRunId, @TenantId, @SiteId, @Status, @RunCode, @LineId, @LineName, @MachineId, @OperatorName,
     @IdealCycleTimeSeconds, 0, 0, 0,
     @StartedAtUtc, @CreatedAtUtc, @CreatedBy, @CreatedAtUtc);

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ProductionRuns', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ProductionRunId=', @ProductionRunId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProductionRunId=', @ProductionRunId, N'; User=', SYSTEM_USER);
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

BEGIN TRY

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

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ProductionRuns', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ProductionRunId=', @ProductionRunId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProductionRunId=', @ProductionRunId, N'; User=', SYSTEM_USER);
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
