-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Procedures
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ProceduresAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the Procedures aggregate root (IProceduresRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named "ProceduresAggregate"
--                  (not "Procedures") to avoid clashing with pre-existing
--                  [Compliance].[Procedures*] category tables (e.g. ProceduresSafety,
--                  ProceduresSOPs). Phase 4 full build-out: this aggregate previously had zero
--                  persistence at all. Mirrors [Compliance].[Permits]'s native ROWVERSION
--                  concurrency convention from day one (no retrofit needed).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProceduresAggregate' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ProceduresAggregate]
    (
        ProceduresId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProceduresAggregate PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Status                 NVARCHAR(50)     NOT NULL,
        ProcedureTitle         NVARCHAR(400)    NOT NULL,
        ProcedureNumber        NVARCHAR(400)    NOT NULL,
        Category               NVARCHAR(100)    NOT NULL,
        RevisionNumber         INT              NOT NULL CONSTRAINT DF_ProceduresAggregate_RevisionNumber DEFAULT (1),
        OwnerId                UNIQUEIDENTIFIER NOT NULL,
        EffectiveAtUtc         DATETIME2(3)     NULL,
        ReviewDueAtUtc         DATETIME2(3)     NULL,
        CreatedAtUtc           DATETIME2(3)     NOT NULL,
        CreatedBy              UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc           DATETIME2(3)     NULL,
        UpdatedBy              UNIQUEIDENTIFIER NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_ProceduresAggregate_IsDeleted DEFAULT (0),
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_ProceduresAggregate_TenantId_Site ON [Compliance].[ProceduresAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ProceduresAggregate_Status ON [Compliance].[ProceduresAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ProceduresAggregate_CreatedAtUtc ON [Compliance].[ProceduresAggregate] (CreatedAtUtc);
END
GO
