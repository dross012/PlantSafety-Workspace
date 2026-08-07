-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Packaging
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PHMSA_PackagingSpecification.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Table backing the PackagingSpecification aggregate root (Packaging module,
--                  PlantSafety.Domain.Compliance.PHMSA.Packaging) -- rich aggregate persistence,
--                  tenant-scoped, mirroring [Compliance].[Permits]. PerformanceTests is list-valued
--                  on the domain aggregate and is stored as a single NVARCHAR(MAX) column holding a
--                  native SQL Server JSON array of strings (see PHMSA_HazmatMaterial.sql header for
--                  why -- no existing rich module has a child-table precedent for array-valued
--                  fields). AssociatedMaterialId is an optional FK to
--                  [Compliance].[PHMSA_HazmatMaterial] and is indexed per FK-indexing policy.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PHMSA_PackagingSpecification' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PHMSA_PackagingSpecification]
    (
        PackagingSpecificationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PHMSA_PackagingSpecification PRIMARY KEY,
        TenantId                  UNIQUEIDENTIFIER NOT NULL,
        SiteId                    UNIQUEIDENTIFIER NULL,
        Status                    NVARCHAR(20)     NOT NULL,
        DOTSpec                    NVARCHAR(20)     NOT NULL,
        PerformanceTests           NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_PHMSA_PackagingSpecification_PerformanceTests DEFAULT (N'[]'),
        MaxCapacity                 DECIMAL(18,4)    NOT NULL,
        CapacityUnit                 NVARCHAR(10)     NOT NULL,
        ClosureInstructions          NVARCHAR(MAX)    NOT NULL,
        ReuseRestrictions             NVARCHAR(MAX)    NULL,
        AssociatedMaterialId          UNIQUEIDENTIFIER NULL CONSTRAINT FK_PHMSA_PackagingSpecification_HazmatMaterial REFERENCES [Compliance].[PHMSA_HazmatMaterial] (HazmatMaterialId),
        CreatedAtUtc                   DATETIME2(3)     NOT NULL,
        CreatedBy                      UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc                   DATETIME2(3)     NULL,
        UpdatedBy                      UNIQUEIDENTIFIER NULL,
        IsDeleted                      BIT              NOT NULL CONSTRAINT DF_PHMSA_PackagingSpecification_IsDeleted DEFAULT (0),
        RowVersion                     ROWVERSION       NOT NULL,
        CONSTRAINT CK_PHMSA_PackagingSpecification_PerformanceTests_Json CHECK (ISJSON(PerformanceTests) = 1)
    );

    CREATE NONCLUSTERED INDEX IX_PHMSA_PackagingSpecification_TenantId_Site ON [Compliance].[PHMSA_PackagingSpecification] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_PackagingSpecification_Status ON [Compliance].[PHMSA_PackagingSpecification] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_PackagingSpecification_CreatedAtUtc ON [Compliance].[PHMSA_PackagingSpecification] (CreatedAtUtc);
    CREATE NONCLUSTERED INDEX IX_PHMSA_PackagingSpecification_TenantId ON [Compliance].[PHMSA_PackagingSpecification] (TenantId);
    CREATE NONCLUSTERED INDEX IX_PHMSA_PackagingSpecification_DOTSpec ON [Compliance].[PHMSA_PackagingSpecification] (DOTSpec);
    CREATE NONCLUSTERED INDEX IX_PHMSA_PackagingSpecification_AssociatedMaterialId ON [Compliance].[PHMSA_PackagingSpecification] (AssociatedMaterialId);
END
GO
