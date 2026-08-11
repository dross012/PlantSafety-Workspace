-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ProductionTracking
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ProductionRunEvent_Downtime.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Insert procedures for ProductionRunEvents (timeline)/ProductionDowntimeEvents,
--                  plus the by-run downtime list feeding the real
--                  Observability.Telemetry.TelemetryAnomalyDetector-backed bottleneck detection
--                  read path (see ProductionTrackingKpiQueryHandlers.cs). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRunEvent_Insert]
    @EventId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
    @RunCode NVARCHAR(50),
    @RunEventType NVARCHAR(50),
    @Description NVARCHAR(2000) = NULL,
    @OccurredAtUtc DATETIME2(3)
AS
SET NOCOUNT ON;

BEGIN TRY

INSERT INTO [Operational].[ProductionRunEvents]
    (EventId, TenantId, ProductionRunId, RunCode, RunEventType, Description, OccurredAtUtc)
VALUES
    (@EventId, @TenantId, @ProductionRunId, @RunCode, @RunEventType, @Description, @OccurredAtUtc);

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ProductionRunEvents', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EventId=', @EventId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EventId=', @EventId, N'; User=', SYSTEM_USER);
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

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionDowntimeEvent_Insert]
    @EventId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
    @MachineId UNIQUEIDENTIFIER,
    @ReasonCode NVARCHAR(50),
    @StartUtc DATETIME2(3),
    @EndUtc DATETIME2(3) = NULL,
    @IsPlanned BIT = 0
AS
SET NOCOUNT ON;

BEGIN TRY

INSERT INTO [Operational].[ProductionDowntimeEvents]
    (EventId, TenantId, ProductionRunId, MachineId, ReasonCode, StartUtc, EndUtc, IsPlanned)
VALUES
    (@EventId, @TenantId, @ProductionRunId, @MachineId, @ReasonCode, @StartUtc, @EndUtc, @IsPlanned);

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ProductionDowntimeEvents', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'EventId=', @EventId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EventId=', @EventId, N'; User=', SYSTEM_USER);
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

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionDowntimeEvent_ListByRun]
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT ReasonCode, StartUtc, EndUtc, IsPlanned
FROM [Operational].[ProductionDowntimeEvents]
WHERE TenantId = @TenantId
  AND ProductionRunId = @ProductionRunId
ORDER BY StartUtc ASC;
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRun_Timeline]
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    EventId, OccurredAtUtc AS TimestampUtc, RunEventType AS EventType, ISNULL(Description, N'') AS Description, ProductionRunId,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[ProductionRunEvents]
WHERE TenantId = @TenantId
  AND ProductionRunId = @ProductionRunId
ORDER BY OccurredAtUtc ASC, EventId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionRun_Events]
    @TenantId UNIQUEIDENTIFIER,
    @ProductionRunId UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 25
AS
SET NOCOUNT ON;

DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

SELECT
    EventId, OccurredAtUtc AS TimestampUtc, RunCode, ProductionRunId, RunEventType AS EventType, ISNULL(Description, N'') AS Description,
    COUNT(*) OVER() AS TotalCount
FROM [Operational].[ProductionRunEvents]
WHERE TenantId = @TenantId
  AND ProductionRunId = @ProductionRunId
ORDER BY OccurredAtUtc DESC, EventId ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
GO
