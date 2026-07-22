-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scorecards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           Scorecard.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the Scorecard aggregate root (IScorecardRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier) so the platform-wide
--                  Row-Level-Security discovery in 90-row-level-security.sql picks it up
--                  automatically. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Scorecards')
    EXEC('CREATE SCHEMA Scorecards AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Scorecard' AND schema_id = SCHEMA_ID('Scorecards'))
BEGIN
    CREATE TABLE [Scorecards].[Scorecard]
    (
        ScorecardId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Scorecard PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        Status            NVARCHAR(30)     NOT NULL,
        ScorecardTitle    NVARCHAR(400)    NOT NULL,
        ScorecardVersion  INT              NOT NULL CONSTRAINT DF_Scorecard_Version DEFAULT (1),
        Category          NVARCHAR(30)     NOT NULL,
        PeriodType        NVARCHAR(30)     NOT NULL,
        PeriodStartUtc    DATETIME2(3)     NOT NULL,
        PeriodEndUtc      DATETIME2(3)     NOT NULL,
        OwnerId           UNIQUEIDENTIFIER NOT NULL,
        PublishedAtUtc    DATETIME2(3)     NULL,
        DistributionCount INT              NOT NULL CONSTRAINT DF_Scorecard_DistCount DEFAULT (0),
        ReviewRequired    BIT              NOT NULL CONSTRAINT DF_Scorecard_ReviewRequired DEFAULT (1),
        ReviewCount       INT              NOT NULL CONSTRAINT DF_Scorecard_ReviewCount DEFAULT (0),
        CreatedAtUtc      DATETIME2(3)     NOT NULL,
        CreatedBy         UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc      DATETIME2(3)     NULL,
        UpdatedBy         UNIQUEIDENTIFIER NULL,
        IsDeleted         BIT              NOT NULL CONSTRAINT DF_Scorecard_IsDeleted DEFAULT (0),
        ConcurrencyToken  UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Scorecard_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_Scorecard_TenantId_Category ON [Scorecards].[Scorecard] (TenantId, Category, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_Scorecard_TenantId_Site ON [Scorecards].[Scorecard] (TenantId, SiteId, IsDeleted);
END
GO
