-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Classification
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PHMSA_HazmatMaterial.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Table backing the HazmatMaterial aggregate root (Classification module,
--                  PlantSafety.Domain.Compliance.PHMSA.Classification) -- persistence for a rich
--                  DDD aggregate, not flat OSHA/FMCSA-style reference data. Tenant-scoped
--                  (TenantId NOT NULL uniqueidentifier), matching [Compliance].[Permits]. One flat
--                  row per aggregate; rehydrated via reflection from a HazmatMaterialRow record by
--                  the (separately built) repository, mirroring the Permits precedent. SpecialProvisions
--                  is list-valued on the domain aggregate; none of the existing rich modules
--                  (Permits/CertificationAggregate/ProceduresAggregate/PolicyAggregate/
--                  ComplianceRuleAggregate/AuditingAggregate/ManagementOfChange) have an
--                  array-valued field to establish a child-table precedent, so per this build's
--                  guidance it is stored as a single NVARCHAR(MAX) column holding a native SQL
--                  Server JSON array of strings, validated with ISJSON. No unique constraint on
--                  (TenantId, UNNumber): multiple classification rows can legitimately exist per UN
--                  number over time (Draft/Superseded/Retired lifecycle states), so a plain
--                  uniqueness constraint would be wrong; a filtered unique index instead enforces
--                  "at most one Classified row per (TenantId, UNNumber)".
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PHMSA_HazmatMaterial' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PHMSA_HazmatMaterial]
    (
        HazmatMaterialId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PHMSA_HazmatMaterial PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        Status                 NVARCHAR(20)     NOT NULL,
        UNNumber                NVARCHAR(20)     NOT NULL,
        ProperShippingName      NVARCHAR(300)    NOT NULL,
        HazardClass             NVARCHAR(10)     NOT NULL,
        PackingGroup             NVARCHAR(20)     NOT NULL,
        SpecialProvisions       NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_PHMSA_HazmatMaterial_SpecialProvisions DEFAULT (N'[]'),
        ERGGuideNumber           NVARCHAR(10)     NULL,
        IsMarinePollutant        BIT              NOT NULL CONSTRAINT DF_PHMSA_HazmatMaterial_IsMarinePollutant DEFAULT (0),
        IsTIH                    BIT              NOT NULL CONSTRAINT DF_PHMSA_HazmatMaterial_IsTIH DEFAULT (0),
        TIHZone                  NVARCHAR(5)      NULL,
        CreatedAtUtc             DATETIME2(3)     NOT NULL,
        CreatedBy                UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc             DATETIME2(3)     NULL,
        UpdatedBy                UNIQUEIDENTIFIER NULL,
        IsDeleted                BIT              NOT NULL CONSTRAINT DF_PHMSA_HazmatMaterial_IsDeleted DEFAULT (0),
        RowVersion               ROWVERSION       NOT NULL,
        CONSTRAINT CK_PHMSA_HazmatMaterial_SpecialProvisions_Json CHECK (ISJSON(SpecialProvisions) = 1)
    );

    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatMaterial_TenantId_Site ON [Compliance].[PHMSA_HazmatMaterial] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatMaterial_Status ON [Compliance].[PHMSA_HazmatMaterial] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatMaterial_CreatedAtUtc ON [Compliance].[PHMSA_HazmatMaterial] (CreatedAtUtc);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatMaterial_TenantId ON [Compliance].[PHMSA_HazmatMaterial] (TenantId);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatMaterial_UNNumber ON [Compliance].[PHMSA_HazmatMaterial] (UNNumber);

    -- Enforces "at most one Classified record per (TenantId, UNNumber)" without blocking the
    -- Draft/Superseded/Retired history rows that legitimately share a UN number over time.
    CREATE UNIQUE NONCLUSTERED INDEX UX_PHMSA_HazmatMaterial_TenantId_UNNumber_Classified
        ON [Compliance].[PHMSA_HazmatMaterial] (TenantId, UNNumber)
        WHERE Status = 'Classified';
END
GO
