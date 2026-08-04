-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CyberPhysicalSecurity
--  Layer:          Infrastructure -- SQL Scripts
--  File:           OtAsset.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the OtAsset aggregate root (OT Asset Inventory subdomain).
--                  Access to raw OT asset data must be restricted to authorized security roles
--                  (enforced via GRANT/DENY on the [CyberPhysicalSecurity] schema and API-layer
--                  [Authorize] policies -- see CyberPhysicalSecurityModule.Permissions).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CyberPhysicalSecurity')
    EXEC('CREATE SCHEMA CyberPhysicalSecurity AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OtAsset' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
BEGIN
    CREATE TABLE [CyberPhysicalSecurity].[OtAsset]
    (
        OtAssetId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OtAsset PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NOT NULL,
        AssetTag             NVARCHAR(128)    NOT NULL,
        AssetName            NVARCHAR(256)    NOT NULL,
        AssetType            NVARCHAR(30)     NOT NULL,
        Vendor               NVARCHAR(128)    NOT NULL CONSTRAINT DF_OtAsset_Vendor DEFAULT (N''),
        Model                NVARCHAR(128)    NOT NULL CONSTRAINT DF_OtAsset_Model DEFAULT (N''),
        FirmwareVersion      NVARCHAR(64)     NOT NULL CONSTRAINT DF_OtAsset_Firmware DEFAULT (N''),
        NetworkZone          NVARCHAR(64)     NOT NULL CONSTRAINT DF_OtAsset_Zone DEFAULT (N''),
        Criticality          NVARCHAR(20)     NOT NULL,
        Status               NVARCHAR(20)     NOT NULL,
        LastReconciledAtUtc  DATETIME2(3)     NULL,
        CreatedAtUtc         DATETIME2(3)     NOT NULL,
        CreatedBy            UNIQUEIDENTIFIER NOT NULL,
        RowVersion           ROWVERSION       NOT NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_OtAsset_Tenant_AssetTag ON [CyberPhysicalSecurity].[OtAsset] (TenantId, AssetTag);
    CREATE NONCLUSTERED INDEX IX_OtAsset_Tenant_Status ON [CyberPhysicalSecurity].[OtAsset] (TenantId, Status);
    CREATE NONCLUSTERED INDEX IX_OtAsset_Tenant_Site ON [CyberPhysicalSecurity].[OtAsset] (TenantId, SiteId);
END
GO
