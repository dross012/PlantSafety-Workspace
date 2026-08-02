-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Engagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           EngagementRecords.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the Engagement aggregate root (IEngagementRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named "EngagementRecords"
--                  (not "Engagement") to avoid any collision with the pre-existing, unrelated
--                  [Analytics].[EngagementDataPoints] / [Analytics].[EngagementScoring] read-model
--                  tables (generic metric time-series, not this per-worker aggregate). Phase 4 full
--                  build-out: this aggregate previously had zero persistence at all -- confirmed
--                  zero files anywhere under PlantSafety.Application\Analytics\Engagement
--                  referenced Domain.Analytics.Engagement. Mirrors [Compliance].[Permits]'s native
--                  ROWVERSION concurrency convention from day one (no retrofit needed).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Analytics')
    EXEC('CREATE SCHEMA Analytics AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EngagementRecords' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[EngagementRecords]
    (
        EngagementId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EngagementRecords PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Status                 NVARCHAR(50)     NOT NULL,
        WorkerId               UNIQUEIDENTIFIER NOT NULL,
        EngagementScore        DECIMAL(18,4)    NOT NULL,
        SurveyPeriod           NVARCHAR(50)     NOT NULL,
        ParticipationChannel   NVARCHAR(100)    NOT NULL,
        ResponseCount          INT              NOT NULL,
        LastEngagedAtUtc       DATETIME2(3)     NULL,
        CreatedAtUtc           DATETIME2(3)     NOT NULL,
        CreatedBy              UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc           DATETIME2(3)     NULL,
        UpdatedBy              UNIQUEIDENTIFIER NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_EngagementRecords_IsDeleted DEFAULT (0),
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_EngagementRecords_TenantId_Site ON [Analytics].[EngagementRecords] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EngagementRecords_Status ON [Analytics].[EngagementRecords] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EngagementRecords_CreatedAtUtc ON [Analytics].[EngagementRecords] (CreatedAtUtc);
END
GO
