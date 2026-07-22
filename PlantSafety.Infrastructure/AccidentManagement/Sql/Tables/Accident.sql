-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           Accident.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the Accident aggregate root (IAccidentRepository). Tenant-scoped
--                  (TenantId NOT NULL uniqueidentifier) so the platform-wide Row-Level-Security
--                  discovery in 90-row-level-security.sql picks it up automatically.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AccidentManagement')
    EXEC('CREATE SCHEMA AccidentManagement AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Accident' AND schema_id = SCHEMA_ID('AccidentManagement'))
BEGIN
    CREATE TABLE [AccidentManagement].[Accident]
    (
        AccidentId                UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Accident PRIMARY KEY,
        TenantId                  UNIQUEIDENTIFIER NOT NULL,
        SiteId                    UNIQUEIDENTIFIER NULL,
        Status                    NVARCHAR(30)     NOT NULL,
        AccidentTitle             NVARCHAR(400)    NOT NULL,
        Location                  NVARCHAR(400)    NOT NULL,
        Severity                  NVARCHAR(30)     NOT NULL,
        InjuredPersonCount        INT              NOT NULL CONSTRAINT DF_Accident_InjuredCount DEFAULT (0),
        OccurredAtUtc             DATETIME2(3)     NOT NULL,
        Category                  NVARCHAR(100)    NOT NULL,
        SiteSecuredAndTriaged     BIT              NOT NULL CONSTRAINT DF_Accident_SiteSecured DEFAULT (0),
        InvestigatorId            UNIQUEIDENTIFIER NULL,
        InvestigatorAssignedAtUtc DATETIME2(3)     NULL,
        RootCause                 NVARCHAR(30)     NULL,
        RootCauseNotes            NVARCHAR(MAX)    NULL,
        RootCauseRecordedAtUtc    DATETIME2(3)     NULL,
        RegulatoryFilingRequired  BIT              NOT NULL CONSTRAINT DF_Accident_RegFilingRequired DEFAULT (0),
        RegulatoryAgency          NVARCHAR(200)    NULL,
        RegulatoryReferenceNumber NVARCHAR(100)    NULL,
        RegulatoryFiledAtUtc      DATETIME2(3)     NULL,
        ClosedAtUtc               DATETIME2(3)     NULL,
        OwnerId                   UNIQUEIDENTIFIER NOT NULL,
        ReviewRequired            BIT              NOT NULL CONSTRAINT DF_Accident_ReviewRequired DEFAULT (1),
        CreatedAtUtc              DATETIME2(3)     NOT NULL,
        CreatedBy                 UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc              DATETIME2(3)     NULL,
        UpdatedBy                 UNIQUEIDENTIFIER NULL,
        IsDeleted                 BIT              NOT NULL CONSTRAINT DF_Accident_IsDeleted DEFAULT (0),
        ConcurrencyToken          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Accident_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_Accident_TenantId_Category ON [AccidentManagement].[Accident] (TenantId, Category, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_Accident_TenantId_Site ON [AccidentManagement].[Accident] (TenantId, SiteId, IsDeleted);
END
GO
