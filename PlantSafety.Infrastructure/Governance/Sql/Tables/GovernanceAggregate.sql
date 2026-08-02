-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Governance
--  Layer:          Infrastructure -- SQL Scripts
--  File:           GovernanceAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the Governance aggregate root (IGovernanceRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named
--                  "GovernanceAggregate" (not bare "Governance") for clarity and to avoid any
--                  ambiguity with the pre-existing, dead GovernanceStub.cs scaffold that lives
--                  under this same module folder. Phase 4 full build-out: this aggregate
--                  previously had zero persistence at all. Mirrors
--                  [Compliance].[AuditingAggregate]'s native ROWVERSION concurrency convention
--                  from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Governance')
    EXEC('CREATE SCHEMA Governance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GovernanceAggregate' AND schema_id = SCHEMA_ID('Governance'))
BEGIN
    CREATE TABLE [Governance].[GovernanceAggregate]
    (
        GovernanceId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_GovernanceAggregate PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        Status               NVARCHAR(50)     NOT NULL,
        GovernanceArea       NVARCHAR(200)    NOT NULL,
        PolicyReferenceId    UNIQUEIDENTIFIER NULL,
        ReviewCycleMonths    INT              NOT NULL CONSTRAINT DF_GovernanceAggregate_ReviewCycleMonths DEFAULT (12),
        OwnerUserId          UNIQUEIDENTIFIER NOT NULL,
        CreatedAtUtc         DATETIME2(3)     NOT NULL,
        CreatedBy            UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc         DATETIME2(3)     NULL,
        UpdatedBy            UNIQUEIDENTIFIER NULL,
        IsDeleted            BIT              NOT NULL CONSTRAINT DF_GovernanceAggregate_IsDeleted DEFAULT (0),
        RowVersion           ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_GovernanceAggregate_TenantId_Site ON [Governance].[GovernanceAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_GovernanceAggregate_Status ON [Governance].[GovernanceAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_GovernanceAggregate_CreatedAtUtc ON [Governance].[GovernanceAggregate] (CreatedAtUtc);
END
GO
