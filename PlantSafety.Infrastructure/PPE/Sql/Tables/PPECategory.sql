-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PPECategory.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PPECategory aggregate root (IPPECategoryRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PPE')
    EXEC('CREATE SCHEMA PPE AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PPECategory' AND schema_id = SCHEMA_ID('PPE'))
BEGIN
    CREATE TABLE [PPE].[PPECategory]
    (
        PPECategoryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PPECategory PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        CategoryCode NVARCHAR(400) NOT NULL,
        HazardsMitigated NVARCHAR(400) NOT NULL,
        InspectionIntervalDays INT NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PPECategory_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PPECategory_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PPECategory_TenantId_Site ON [PPE].[PPECategory] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPECategory_Status ON [PPE].[PPECategory] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPECategory_CreatedAtUtc ON [PPE].[PPECategory] (CreatedAtUtc);
END
GO
