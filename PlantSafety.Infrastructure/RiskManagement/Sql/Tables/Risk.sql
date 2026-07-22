-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           Risk.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the Risk aggregate root (IRiskRepository). Tenant-scoped
--                  (TenantId NOT NULL uniqueidentifier) so the platform-wide Row-Level-Security
--                  discovery in 90-row-level-security.sql picks it up automatically. This is the
--                  Domain.RiskManagement bounded context (Full Risk Management Process aggregate),
--                  distinct from the pre-existing PlantSafety.Infrastructure.OperationalSafety
--                  .RiskManagement module -- a different schema/namespace is used to avoid any
--                  collision. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'RiskMgmt')
    EXEC('CREATE SCHEMA RiskMgmt AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Risk' AND schema_id = SCHEMA_ID('RiskMgmt'))
BEGIN
    CREATE TABLE [RiskMgmt].[Risk]
    (
        RiskId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Risk PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NULL,
        Status              NVARCHAR(30)     NOT NULL,
        RiskTitle           NVARCHAR(400)    NOT NULL,
        Category            NVARCHAR(30)     NOT NULL,
        HazardDomainCategory NVARCHAR(100)   NOT NULL CONSTRAINT DF_Risk_HazardDomainCategory DEFAULT (N'HazardIdentificationMeasurement'),
        Location            NVARCHAR(400)    NOT NULL,
        HazardSource        NVARCHAR(400)    NOT NULL,
        OwnerId             UNIQUEIDENTIFIER NOT NULL,
        CurrentSeverity     INT              NULL,
        CurrentLikelihood   INT              NULL,
        Priority            NVARCHAR(10)     NULL,
        DocumentedAtUtc     DATETIME2(3)     NULL,
        ResidualSeverity    INT              NULL,
        ResidualLikelihood  INT              NULL,
        LastValidatedAtUtc  DATETIME2(3)     NULL,
        ReviewCadence       NVARCHAR(30)     NOT NULL,
        LastReviewedAtUtc   DATETIME2(3)     NULL,
        NextReviewDueUtc    DATETIME2(3)     NULL,
        ReviewRequired      BIT              NOT NULL CONSTRAINT DF_Risk_ReviewRequired DEFAULT (1),
        ReviewCount         INT              NOT NULL CONSTRAINT DF_Risk_ReviewCount DEFAULT (0),
        RiskVersion         INT              NOT NULL CONSTRAINT DF_Risk_Version DEFAULT (1),
        CreatedAtUtc        DATETIME2(3)     NOT NULL,
        CreatedBy           UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc        DATETIME2(3)     NULL,
        UpdatedBy           UNIQUEIDENTIFIER NULL,
        IsDeleted           BIT              NOT NULL CONSTRAINT DF_Risk_IsDeleted DEFAULT (0),
        ConcurrencyToken    UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Risk_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_Risk_TenantId_Category ON [RiskMgmt].[Risk] (TenantId, Category, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_Risk_TenantId_Site ON [RiskMgmt].[Risk] (TenantId, SiteId, IsDeleted);
END
GO
