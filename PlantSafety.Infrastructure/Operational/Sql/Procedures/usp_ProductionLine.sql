-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.ProductionTracking
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ProductionLine.sql
--  Created:        2026-08-03
--  SchemaVersion:  2026.1
--  Description:    Insert/GetById procedures for the ProductionLine master-data write-side
--                  repository. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionLine_Insert]
    @ProductionLineId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @LineCode NVARCHAR(50),
    @LineName NVARCHAR(200),
    @Status NVARCHAR(20),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

BEGIN TRY

INSERT INTO [Operational].[ProductionLines]
    (ProductionLineId, TenantId, SiteId, LineCode, LineName, Status, CreatedAtUtc, CreatedBy, UpdatedAtUtc)
VALUES
    (@ProductionLineId, @TenantId, @SiteId, @LineCode, @LineName, @Status, @CreatedAtUtc, @CreatedBy, @CreatedAtUtc);

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ProductionLines', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ProductionLineId=', @ProductionLineId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProductionLineId=', @ProductionLineId, N'; User=', SYSTEM_USER);
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

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionLine_GetById]
    @ProductionLineId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
    ProductionLineId, TenantId, SiteId, LineCode, LineName, Status,
    CreatedAtUtc, CreatedBy, UpdatedAtUtc
FROM [Operational].[ProductionLines]
WHERE ProductionLineId = @ProductionLineId
  AND TenantId = @TenantId
  AND IsDeleted = 0;
GO

CREATE OR ALTER PROCEDURE [Operational].[usp_ProductionLine_Update]
    @ProductionLineId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(20),
    @UpdatedAtUtc DATETIME2(3)
AS
SET NOCOUNT ON;

BEGIN TRY

UPDATE [Operational].[ProductionLines]
SET Status = @Status, UpdatedAtUtc = @UpdatedAtUtc
WHERE ProductionLineId = @ProductionLineId
  AND TenantId = @TenantId
  AND IsDeleted = 0;

        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ProductionLines', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ProductionLineId=', @ProductionLineId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProductionLineId=', @ProductionLineId, N'; User=', SYSTEM_USER);
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
