-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Placarding
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PHMSA_HazmatContainer.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Table backing the HazmatContainer aggregate root (Placarding module,
--                  PlantSafety.Domain.Compliance.PHMSA.Placarding) -- rich aggregate persistence,
--                  tenant-scoped, mirroring [Compliance].[Permits]. Markings/Labels/Placards are
--                  list-valued on the domain aggregate and are each stored as a single NVARCHAR(MAX)
--                  column holding a native SQL Server JSON array of strings (see
--                  PHMSA_HazmatMaterial.sql header for rationale). MaterialId and PackagingSpecId
--                  are required/optional FKs respectively into the sibling PHMSA tables and are
--                  both indexed per FK-indexing policy.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PHMSA_HazmatContainer' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PHMSA_HazmatContainer]
    (
        HazmatContainerId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PHMSA_HazmatContainer PRIMARY KEY,
        TenantId                  UNIQUEIDENTIFIER NOT NULL,
        SiteId                    UNIQUEIDENTIFIER NULL,
        Status                    NVARCHAR(20)     NOT NULL,
        ContainerType               NVARCHAR(30)     NOT NULL,
        MaterialId                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_PHMSA_HazmatContainer_HazmatMaterial REFERENCES [Compliance].[PHMSA_HazmatMaterial] (HazmatMaterialId),
        PackagingSpecId               UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_PHMSA_HazmatContainer_PackagingSpecification REFERENCES [Compliance].[PHMSA_PackagingSpecification] (PackagingSpecificationId),
        Markings                       NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_PHMSA_HazmatContainer_Markings DEFAULT (N'[]'),
        Labels                          NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_PHMSA_HazmatContainer_Labels DEFAULT (N'[]'),
        Placards                        NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_PHMSA_HazmatContainer_Placards DEFAULT (N'[]'),
        OrientationArrowsRequired        BIT              NOT NULL CONSTRAINT DF_PHMSA_HazmatContainer_OrientationArrowsRequired DEFAULT (0),
        CompatibilityGroup                NVARCHAR(5)      NULL,
        CreatedAtUtc                       DATETIME2(3)     NOT NULL,
        CreatedBy                          UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc                       DATETIME2(3)     NULL,
        UpdatedBy                          UNIQUEIDENTIFIER NULL,
        IsDeleted                          BIT              NOT NULL CONSTRAINT DF_PHMSA_HazmatContainer_IsDeleted DEFAULT (0),
        RowVersion                         ROWVERSION       NOT NULL,
        CONSTRAINT CK_PHMSA_HazmatContainer_Markings_Json CHECK (ISJSON(Markings) = 1),
        CONSTRAINT CK_PHMSA_HazmatContainer_Labels_Json CHECK (ISJSON(Labels) = 1),
        CONSTRAINT CK_PHMSA_HazmatContainer_Placards_Json CHECK (ISJSON(Placards) = 1)
    );

    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatContainer_TenantId_Site ON [Compliance].[PHMSA_HazmatContainer] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatContainer_Status ON [Compliance].[PHMSA_HazmatContainer] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatContainer_CreatedAtUtc ON [Compliance].[PHMSA_HazmatContainer] (CreatedAtUtc);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatContainer_TenantId ON [Compliance].[PHMSA_HazmatContainer] (TenantId);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatContainer_MaterialId ON [Compliance].[PHMSA_HazmatContainer] (MaterialId);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatContainer_PackagingSpecId ON [Compliance].[PHMSA_HazmatContainer] (PackagingSpecId);
END
GO
