-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AiGovernance
--  Layer:          Infrastructure -- SQL Scripts
--  File:           AiGovernanceAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the AiGovernance aggregate root (IAiGovernanceRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named "AiGovernanceAggregate"
--                  (not bare "AiGovernance") to avoid any ambiguity with the pre-existing, unrelated
--                  Features.AiGovernance "GovernedModel" registry that already lives under the
--                  [AiGovernance] schema (table [AiGovernance].[GovernedModel], procs
--                  usp_Model_Create/GetById/List) and is live-wired into DI. This is a distinct
--                  DDD aggregate: an AI system governance/oversight registry record. Phase 4 full
--                  build-out: this aggregate previously had zero persistence at all. Mirrors
--                  [Compliance].[AuditingAggregate]'s native ROWVERSION concurrency convention from
--                  day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AiGovernance')
    EXEC('CREATE SCHEMA AiGovernance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AiGovernanceAggregate' AND schema_id = SCHEMA_ID('AiGovernance'))
BEGIN
    CREATE TABLE [AiGovernance].[AiGovernanceAggregate]
    (
        AiGovernanceId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AiGovernanceAggregate PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        SiteId                   UNIQUEIDENTIFIER NULL,
        Status                   NVARCHAR(50)     NOT NULL,
        SystemName               NVARCHAR(200)    NOT NULL,
        AiUseCase                NVARCHAR(2000)   NOT NULL,
        RiskClassification       NVARCHAR(50)     NOT NULL,
        OwnerUserId              UNIQUEIDENTIFIER NOT NULL,
        RequiresHumanOversight   BIT              NOT NULL CONSTRAINT DF_AiGovernanceAggregate_RequiresHumanOversight DEFAULT (1),
        CreatedAtUtc             DATETIME2(3)     NOT NULL,
        CreatedBy                UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc             DATETIME2(3)     NULL,
        UpdatedBy                UNIQUEIDENTIFIER NULL,
        IsDeleted                BIT              NOT NULL CONSTRAINT DF_AiGovernanceAggregate_IsDeleted DEFAULT (0),
        RowVersion               ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_AiGovernanceAggregate_TenantId_Site ON [AiGovernance].[AiGovernanceAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_AiGovernanceAggregate_Status ON [AiGovernance].[AiGovernanceAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_AiGovernanceAggregate_CreatedAtUtc ON [AiGovernance].[AiGovernanceAggregate] (CreatedAtUtc);
END
GO
