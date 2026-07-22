-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PPEInventory.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PPEInventory aggregate root (IPPEInventoryRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PPE')
    EXEC('CREATE SCHEMA PPE AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PPEInventory' AND schema_id = SCHEMA_ID('PPE'))
BEGIN
    CREATE TABLE [PPE].[PPEInventory]
    (
        PPEInventoryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PPEInventory PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        PpeItemCode NVARCHAR(400) NOT NULL,
        StockQuantity INT NOT NULL,
        IssuedQuantity INT NOT NULL,
        ExpiresAtUtc DATETIME2(3) NULL,
        ReorderThreshold INT NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PPEInventory_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PPEInventory_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PPEInventory_TenantId_Site ON [PPE].[PPEInventory] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPEInventory_Status ON [PPE].[PPEInventory] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPEInventory_CreatedAtUtc ON [PPE].[PPEInventory] (CreatedAtUtc);
END
GO
