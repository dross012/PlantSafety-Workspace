-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PPEZoneMapping.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PPEZoneMapping aggregate root (IPPEZoneMappingRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PPE')
    EXEC('CREATE SCHEMA PPE AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PPEZoneMapping' AND schema_id = SCHEMA_ID('PPE'))
BEGIN
    CREATE TABLE [PPE].[PPEZoneMapping]
    (
        PPEZoneMappingId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PPEZoneMapping PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        ZoneCode NVARCHAR(400) NOT NULL,
        RequiredPpeCategories NVARCHAR(400) NOT NULL,
        AccessControlIntegrated BIT NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PPEZoneMapping_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PPEZoneMapping_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PPEZoneMapping_TenantId_Site ON [PPE].[PPEZoneMapping] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPEZoneMapping_Status ON [PPE].[PPEZoneMapping] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPEZoneMapping_CreatedAtUtc ON [PPE].[PPEZoneMapping] (CreatedAtUtc);
END
GO
