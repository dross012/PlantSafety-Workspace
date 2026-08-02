-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Policy
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PolicyAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the Policy aggregate root (IPolicyRepository). Tenant-scoped
--                  (TenantId NOT NULL uniqueidentifier). Named "PolicyAggregate" (not "Policy") to
--                  avoid clashing with pre-existing [Compliance].[Policy*] category tables (e.g.
--                  PolicyApprovals, PolicyChemicalHandling) and any future literal "Policy" table
--                  from the separate "Features" legacy tree. Phase 4 full build-out: this aggregate
--                  previously had zero persistence at all. Mirrors [Compliance].[Permits]'s native
--                  ROWVERSION concurrency convention from day one (no retrofit needed).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyAggregate' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyAggregate]
    (
        PolicyId               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PolicyAggregate PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        Status                  NVARCHAR(50)     NOT NULL,
        PolicyTitle             NVARCHAR(400)    NOT NULL,
        PolicyVersion           INT              NOT NULL CONSTRAINT DF_PolicyAggregate_PolicyVersion DEFAULT (1),
        Category                NVARCHAR(100)    NOT NULL,
        OwnerId                 UNIQUEIDENTIFIER NOT NULL,
        EffectiveAtUtc          DATETIME2(3)     NULL,
        AcknowledgmentRequired  BIT              NOT NULL CONSTRAINT DF_PolicyAggregate_AckRequired DEFAULT (1),
        AcknowledgmentCount     INT              NOT NULL CONSTRAINT DF_PolicyAggregate_AckCount DEFAULT (0),
        CreatedAtUtc            DATETIME2(3)     NOT NULL,
        CreatedBy               UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc            DATETIME2(3)     NULL,
        UpdatedBy               UNIQUEIDENTIFIER NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_PolicyAggregate_IsDeleted DEFAULT (0),
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_PolicyAggregate_TenantId_Site ON [Compliance].[PolicyAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PolicyAggregate_Status ON [Compliance].[PolicyAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PolicyAggregate_CreatedAtUtc ON [Compliance].[PolicyAggregate] (CreatedAtUtc);
END
GO
