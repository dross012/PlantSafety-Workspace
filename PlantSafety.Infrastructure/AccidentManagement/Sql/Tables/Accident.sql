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
--  UPDATED:        2026-07-28 (Migration Commander Phase 2, AccidentManagement) -- added a native
--                  RowVersion ROWVERSION NOT NULL column, additive alongside the pre-existing
--                  ConcurrencyToken UNIQUEIDENTIFIER DEFAULT (NEWID()) column. Investigation of
--                  AccidentDapperRepository (both AddAsync/UpdateAsync parameter binding and the
--                  private AccidentRow read-model) confirms ConcurrencyToken is never read back,
--                  never bound as a stored-procedure parameter, and never compared in a WHERE
--                  clause anywhere in the codebase -- it is written (NEWID() on insert, NEWID() on
--                  every update in usp_Accident_Update) but functionally vestigial/dead as an
--                  optimistic-concurrency mechanism today. Per additive-only convention it is left
--                  in place untouched (removing a column is out of scope for this pass and would
--                  require a validated deletion-discipline gate); RowVersion is the new
--                  canonical, ROWVERSION-typed concurrency column matching every other Phase 1/2
--                  module (SafetyEvents.Incident, Analytics.Culture, ProcessManagement.Process,
--                  etc.), exposed to callers as ETag via CONVERT(NVARCHAR(34), CAST(RowVersion AS
--                  BINARY(8)), 1) in usp_Accident_GetById/GetAllActive. Idempotent via COL_LENGTH
--                  guard; safe to re-run against a database that already has the column.
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

-- Additive: native ROWVERSION concurrency column, alongside the pre-existing (vestigial)
-- ConcurrencyToken UNIQUEIDENTIFIER column -- see header comment. Idempotent.
IF COL_LENGTH('[AccidentManagement].[Accident]', 'RowVersion') IS NULL
BEGIN
    ALTER TABLE [AccidentManagement].[Accident] ADD RowVersion ROWVERSION NOT NULL;
END
GO
