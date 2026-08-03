-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           DimSite.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Site dimension table (SCD Type 2). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DimSite' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[DimSite]
    (
        SurrogateKey        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DimSite PRIMARY KEY,
        BusinessKey          UNIQUEIDENTIFIER NOT NULL,
        EffectiveFromUtc      DATETIME2(3)     NOT NULL,
        EffectiveToUtc        DATETIME2(3)     NULL,
        IsCurrent              BIT              NOT NULL CONSTRAINT DF_DimSite_IsCurrent DEFAULT (1),
        RevisionNumber          INT              NOT NULL CONSTRAINT DF_DimSite_Revision DEFAULT (1),
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteName                 NVARCHAR(200)    NOT NULL,
        Region                    NVARCHAR(100)    NOT NULL,
        FacilityType              NVARCHAR(60)     NOT NULL,
        OperationalStatus          NVARCHAR(30)     NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_DimSite_BusinessKey_Current ON [AnalyticsUnifiedModel].[DimSite] (BusinessKey, IsCurrent);
    CREATE NONCLUSTERED INDEX IX_DimSite_Tenant ON [AnalyticsUnifiedModel].[DimSite] (TenantId, IsCurrent);
    -- Exactly one current row per business key, enforced at the application/ETL layer (SCD2
    -- current-row uniqueness is not expressible as a single declarative SQL constraint without a
    -- filtered unique index, which is added separately once IsCurrent semantics stabilize).
    CREATE UNIQUE NONCLUSTERED INDEX UX_DimSite_BusinessKey_CurrentOnly
        ON [AnalyticsUnifiedModel].[DimSite] (BusinessKey) WHERE IsCurrent = 1;
END
GO
