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

INSERT INTO [Operational].[ProductionLines]
    (ProductionLineId, TenantId, SiteId, LineCode, LineName, Status, CreatedAtUtc, CreatedBy, UpdatedAtUtc)
VALUES
    (@ProductionLineId, @TenantId, @SiteId, @LineCode, @LineName, @Status, @CreatedAtUtc, @CreatedBy, @CreatedAtUtc);
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

UPDATE [Operational].[ProductionLines]
SET Status = @Status, UpdatedAtUtc = @UpdatedAtUtc
WHERE ProductionLineId = @ProductionLineId
  AND TenantId = @TenantId
  AND IsDeleted = 0;
GO
