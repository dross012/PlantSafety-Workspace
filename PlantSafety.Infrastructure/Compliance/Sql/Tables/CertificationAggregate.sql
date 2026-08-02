-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification
--  Layer:          Infrastructure -- SQL Scripts
--  File:           CertificationAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the Certification aggregate root (ICertificationRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named
--                  "CertificationAggregate" (not bare "Certification") to avoid any ambiguity with
--                  the pre-existing read-only category slices (Contractor/DOT/EPA/
--                  IndustrySpecific/ISO14001/ISO45001/OSHACertifications/Training) that live under
--                  the same Compliance.Certification module folder. Phase 4 full build-out: this
--                  aggregate previously had zero persistence at all. Mirrors
--                  [Compliance].[Permits]'s native ROWVERSION concurrency convention from day one.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationAggregate' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationAggregate]
    (
        CertificationId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CertificationAggregate PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Status                 NVARCHAR(50)     NOT NULL,
        HolderWorkerId         UNIQUEIDENTIFIER NOT NULL,
        CertificationName      NVARCHAR(400)    NOT NULL,
        CertificationBody      NVARCHAR(400)    NOT NULL,
        CertificationNumber    NVARCHAR(200)    NOT NULL,
        IssuedAtUtc            DATETIME2(3)     NOT NULL,
        ExpiresAtUtc           DATETIME2(3)     NULL,
        RenewalRequired        BIT              NOT NULL CONSTRAINT DF_CertificationAggregate_RenewalRequired DEFAULT (1),
        CreatedAtUtc           DATETIME2(3)     NOT NULL,
        CreatedBy              UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc           DATETIME2(3)     NULL,
        UpdatedBy              UNIQUEIDENTIFIER NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_CertificationAggregate_IsDeleted DEFAULT (0),
        RowVersion             ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_CertificationAggregate_TenantId_Site ON [Compliance].[CertificationAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_CertificationAggregate_Status ON [Compliance].[CertificationAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_CertificationAggregate_CreatedAtUtc ON [Compliance].[CertificationAggregate] (CreatedAtUtc);
END
GO
