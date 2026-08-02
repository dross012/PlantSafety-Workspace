-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Compliance
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ComplianceRuleAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the (self-named) Compliance aggregate root
--                  (IComplianceRepository). Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Named "ComplianceRuleAggregate" -- deliberately NOT bare "Compliance" -- to
--                  avoid the severe ambiguity risk called out in this module's build-out task: the
--                  aggregate's own simple name ("Compliance") collides with both its immediate
--                  namespace segment (PlantSafety.Domain.Compliance.Compliance) and the top-level
--                  "Compliance" module folder/schema itself, and would also collide with the
--                  pre-existing read-only category slices (Obligations/RegulatoryRequirements/
--                  Tracking) under the same Compliance.Compliance module folder. Phase 4 full
--                  build-out: this aggregate previously had zero persistence at all. Mirrors
--                  [Compliance].[Permits]'s native ROWVERSION concurrency convention from day one.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ComplianceRuleAggregate' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[ComplianceRuleAggregate]
    (
        ComplianceId           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ComplianceRuleAggregate PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        Status                  NVARCHAR(50)     NOT NULL,
        RegulationReference     NVARCHAR(400)    NOT NULL,
        RuleDescription         NVARCHAR(2000)   NOT NULL,
        ComplianceScore         DECIMAL(5,2)     NOT NULL CONSTRAINT DF_ComplianceRuleAggregate_Score DEFAULT (0),
        NonConformanceCount     INT              NOT NULL CONSTRAINT DF_ComplianceRuleAggregate_NCCount DEFAULT (0),
        ResponsiblePartyId      UNIQUEIDENTIFIER NOT NULL,
        LastEvaluatedAtUtc      DATETIME2(3)     NULL,
        CreatedAtUtc            DATETIME2(3)     NOT NULL,
        CreatedBy               UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc            DATETIME2(3)     NULL,
        UpdatedBy               UNIQUEIDENTIFIER NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_ComplianceRuleAggregate_IsDeleted DEFAULT (0),
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_ComplianceRuleAggregate_TenantId_Site ON [Compliance].[ComplianceRuleAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ComplianceRuleAggregate_Status ON [Compliance].[ComplianceRuleAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_ComplianceRuleAggregate_CreatedAtUtc ON [Compliance].[ComplianceRuleAggregate] (CreatedAtUtc);
END
GO
