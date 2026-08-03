-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           DimAsset.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Asset/equipment dimension table (SCD Type 2). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DimAsset' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[DimAsset]
    (
        SurrogateKey      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DimAsset PRIMARY KEY,
        BusinessKey        UNIQUEIDENTIFIER NOT NULL,
        EffectiveFromUtc    DATETIME2(3)     NOT NULL,
        EffectiveToUtc      DATETIME2(3)     NULL,
        IsCurrent            BIT              NOT NULL CONSTRAINT DF_DimAsset_IsCurrent DEFAULT (1),
        RevisionNumber        INT              NOT NULL CONSTRAINT DF_DimAsset_Revision DEFAULT (1),
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        AssetName              NVARCHAR(200)    NOT NULL,
        AssetClass              NVARCHAR(100)    NOT NULL,
        SiteKey                  UNIQUEIDENTIFIER NOT NULL,
        CriticalityTier            NVARCHAR(30)     NOT NULL,
        MaintenanceStrategy          NVARCHAR(30)     NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_DimAsset_BusinessKey_Current ON [AnalyticsUnifiedModel].[DimAsset] (BusinessKey, IsCurrent);
    CREATE NONCLUSTERED INDEX IX_DimAsset_Tenant_Site ON [AnalyticsUnifiedModel].[DimAsset] (TenantId, SiteKey, IsCurrent);
    CREATE UNIQUE NONCLUSTERED INDEX UX_DimAsset_BusinessKey_CurrentOnly
        ON [AnalyticsUnifiedModel].[DimAsset] (BusinessKey) WHERE IsCurrent = 1;
END
GO
