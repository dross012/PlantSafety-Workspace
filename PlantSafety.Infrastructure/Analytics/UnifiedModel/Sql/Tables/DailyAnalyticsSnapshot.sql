-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           DailyAnalyticsSnapshot.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Immutable, append-only historical daily rollup snapshot table across the
--                  six unified-model domains. Rows are never updated or deleted.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DailyAnalyticsSnapshot' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[DailyAnalyticsSnapshot]
    (
        SnapshotId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DailyAnalyticsSnapshot PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteKey                  UNIQUEIDENTIFIER NULL,
        BusinessDate               DATE             NOT NULL,
        Domain                       NVARCHAR(40)     NOT NULL,
        EventCount                     BIGINT           NOT NULL CONSTRAINT DF_DailySnapshot_EventCount DEFAULT (0),
        CompliantCount                   BIGINT           NOT NULL CONSTRAINT DF_DailySnapshot_CompliantCount DEFAULT (0),
        ComplianceRate                     FLOAT            NOT NULL CONSTRAINT DF_DailySnapshot_ComplianceRate DEFAULT (0),
        AverageCycleTimeDays                 FLOAT            NULL,
        TrendDirection                         NVARCHAR(20)     NOT NULL CONSTRAINT DF_DailySnapshot_Trend DEFAULT ('Stable'),
        GeneratedAtUtc                           DATETIME2(3)     NOT NULL,
        ComputationVersion                         INT              NOT NULL CONSTRAINT DF_DailySnapshot_CompVersion DEFAULT (1)
    );

    CREATE NONCLUSTERED INDEX IX_DailyAnalyticsSnapshot_Tenant_Date_Domain
        ON [AnalyticsUnifiedModel].[DailyAnalyticsSnapshot] (TenantId, BusinessDate, Domain);
    CREATE NONCLUSTERED INDEX IX_DailyAnalyticsSnapshot_Site
        ON [AnalyticsUnifiedModel].[DailyAnalyticsSnapshot] (SiteKey, BusinessDate);
END
GO
