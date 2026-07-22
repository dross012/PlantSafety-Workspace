-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PPEInventory_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PPEInventory row by identity. Backs IPPEInventoryRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PPE].[usp_PPEInventory_GetById]
    @PPEInventoryId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PPEInventoryId, TenantId, SiteId, Status, PpeItemCode, StockQuantity, IssuedQuantity, ExpiresAtUtc, ReorderThreshold, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PPE].[PPEInventory]
    WHERE PPEInventoryId = @PPEInventoryId AND IsDeleted = 0;
END
GO
