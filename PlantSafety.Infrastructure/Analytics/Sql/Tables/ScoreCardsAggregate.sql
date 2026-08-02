-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ScoreCardsAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the top-level ScoreCards aggregate root
--                  (IScoreCardsRepository). Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Named ScoreCardsAggregate (rather than bare ScoreCards) to avoid any confusion
--                  with the pre-existing [Analytics].[ScoreCardsCompliance] /
--                  [Analytics].[ScoreCardsTraining] read-model tables owned by the separate
--                  ScoreCards.{Compliance,Department,Supervisor,Training} category read-slices --
--                  those are untouched by this migration. Phase 4 full build-out: this aggregate
--                  previously had zero persistence at all (only a Create factory with no write
--                  path). Mirrors [Compliance].[Permits]'s native ROWVERSION concurrency
--                  convention from day one (no retrofit needed).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Analytics')
    EXEC('CREATE SCHEMA Analytics AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ScoreCardsAggregate' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[ScoreCardsAggregate]
    (
        ScoreCardsId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ScoreCardsAggregate PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Status                 NVARCHAR(50)     NOT NULL,
        ScoreCardName          NVARCHAR(400)    NOT NULL,
        ReportingPeriod        NVARCHAR(50)     NOT NULL,
        KpiCategory             NVARCHAR(200)    NOT NULL,
        PerformanceIndex        DECIMAL(18,4)    NOT NULL,
        GeneratedForSiteId      UNIQUEIDENTIFIER NULL,
        GeneratedAtUtc          DATETIME2(3)     NOT NULL,
        PublishedByUserId       UNIQUEIDENTIFIER NULL,
        PublishedAtUtc          DATETIME2(3)     NULL,
        FinalizedByUserId       UNIQUEIDENTIFIER NULL,
        FinalizedAtUtc          DATETIME2(3)     NULL,
        ClosedByUserId          UNIQUEIDENTIFIER NULL,
        ClosedAtUtc             DATETIME2(3)     NULL,
        ArchivedAtUtc           DATETIME2(3)     NULL,
        CreatedAtUtc            DATETIME2(3)     NOT NULL,
        CreatedBy               UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc            DATETIME2(3)     NULL,
        UpdatedBy               UNIQUEIDENTIFIER NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_ScoreCardsAggregate_IsDeleted DEFAULT (0),
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_ScoreCardsAggregate_TenantId_Site ON [Analytics].[ScoreCardsAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ScoreCardsAggregate_Status ON [Analytics].[ScoreCardsAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ScoreCardsAggregate_CreatedAtUtc ON [Analytics].[ScoreCardsAggregate] (CreatedAtUtc);
END
GO
