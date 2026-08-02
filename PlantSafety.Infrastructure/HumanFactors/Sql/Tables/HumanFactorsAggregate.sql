-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors
--  Layer:          Infrastructure -- SQL Scripts
--  File:           HumanFactorsAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the HumanFactors aggregate root (IHumanFactorsRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named
--                  "HumanFactorsAggregate" (not bare "HumanFactors") for clarity and to avoid
--                  any ambiguity with the pre-existing, dead HumanFactorsStub.cs/
--                  Ergonomics/FatigueManagement scaffolding that lives under this same module
--                  folder. Phase 4 full build-out: this aggregate previously had zero
--                  persistence at all. Mirrors [Compliance].[AuditingAggregate]'s native
--                  ROWVERSION concurrency convention from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HumanFactors')
    EXEC('CREATE SCHEMA HumanFactors AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HumanFactorsAggregate' AND schema_id = SCHEMA_ID('HumanFactors'))
BEGIN
    CREATE TABLE [HumanFactors].[HumanFactorsAggregate]
    (
        HumanFactorsId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HumanFactorsAggregate PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                UNIQUEIDENTIFIER NULL,
        Status                NVARCHAR(50)     NOT NULL,
        FactorType            NVARCHAR(100)    NOT NULL,
        AffectedUserId        UNIQUEIDENTIFIER NULL,
        SeverityRating        INT              NOT NULL,
        Description           NVARCHAR(2000)   NOT NULL,
        CreatedAtUtc          DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc          DATETIME2(3)     NULL,
        UpdatedBy             UNIQUEIDENTIFIER NULL,
        IsDeleted             BIT              NOT NULL CONSTRAINT DF_HumanFactorsAggregate_IsDeleted DEFAULT (0),
        RowVersion            ROWVERSION       NOT NULL,
        CONSTRAINT CK_HumanFactorsAggregate_SeverityRating CHECK (SeverityRating BETWEEN 1 AND 5)
    );

    CREATE NONCLUSTERED INDEX IX_HumanFactorsAggregate_TenantId_Site ON [HumanFactors].[HumanFactorsAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_HumanFactorsAggregate_Status ON [HumanFactors].[HumanFactorsAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_HumanFactorsAggregate_CreatedAtUtc ON [HumanFactors].[HumanFactorsAggregate] (CreatedAtUtc);
END
GO
