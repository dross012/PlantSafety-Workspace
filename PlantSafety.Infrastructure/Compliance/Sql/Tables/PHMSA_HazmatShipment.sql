-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.ShippingPapers
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PHMSA_HazmatShipment.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Table backing the HazmatShipment aggregate root (ShippingPapers module,
--                  PlantSafety.Domain.Compliance.PHMSA.ShippingPapers) -- rich aggregate
--                  persistence, tenant-scoped, mirroring [Compliance].[Permits]. MaterialIds,
--                  PackagingIds, ShippingPaperAdditionalModalRequirements, PlacardingRequirements,
--                  and SegregationViolations are list-valued on the domain aggregate and are each
--                  stored as a single NVARCHAR(MAX) column holding a native SQL Server JSON array
--                  (see PHMSA_HazmatMaterial.sql header for rationale); SegregationViolations holds
--                  an array of JSON objects rather than scalars, still validated with ISJSON.
--                  MaterialIds/PackagingIds are deliberately NOT FK columns (they are JSON arrays
--                  of guids, not single scalar references), consistent with the aggregate's own
--                  in-memory shape -- referential integrity for those relationships is enforced by
--                  the Domain layer at the aggregate boundary, not the database. The generated
--                  ShippingPaper is flattened onto this same row (ShippingPaper* columns) rather
--                  than a child table because it is a single owned value object per shipment, not a
--                  collection.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PHMSA_HazmatShipment' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PHMSA_HazmatShipment]
    (
        HazmatShipmentId                              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PHMSA_HazmatShipment PRIMARY KEY,
        TenantId                                       UNIQUEIDENTIFIER NOT NULL,
        SiteId                                         UNIQUEIDENTIFIER NULL,
        Status                                         NVARCHAR(20)     NOT NULL,
        MaterialIds                                    NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_PHMSA_HazmatShipment_MaterialIds DEFAULT (N'[]'),
        PackagingIds                                   NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_PHMSA_HazmatShipment_PackagingIds DEFAULT (N'[]'),
        ShippingPaperBasicDescription                  NVARCHAR(MAX)    NULL,
        ShippingPaperEmergencyResponseRef              NVARCHAR(MAX)    NULL,
        ShippingPaperCertification                     NVARCHAR(MAX)    NULL,
        ShippingPaperWasteManifestReference             NVARCHAR(MAX)    NULL,
        ShippingPaperAdditionalModalRequirements         NVARCHAR(MAX)    NULL,
        ShippingPaperGeneratedAtUtc                       DATETIME2(3)     NULL,
        EmergencyResponseErgGuideNumber                    NVARCHAR(10)     NULL,
        EmergencyResponseContactName                        NVARCHAR(200)    NULL,
        EmergencyResponseContactPhone                        NVARCHAR(50)     NULL,
        EmergencyResponseGuidanceSummary                      NVARCHAR(MAX)    NULL,
        Mode                                                    NVARCHAR(10)     NOT NULL,
        PlacardingRequirements                                  NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_PHMSA_HazmatShipment_PlacardingRequirements DEFAULT (N'[]'),
        SegregationViolations                                   NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_PHMSA_HazmatShipment_SegregationViolations DEFAULT (N'[]'),
        CreatedAtUtc                                            DATETIME2(3)     NOT NULL,
        CreatedBy                                               UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc                                            DATETIME2(3)     NULL,
        UpdatedBy                                               UNIQUEIDENTIFIER NULL,
        IsDeleted                                               BIT              NOT NULL CONSTRAINT DF_PHMSA_HazmatShipment_IsDeleted DEFAULT (0),
        RowVersion                                               ROWVERSION       NOT NULL,
        CONSTRAINT CK_PHMSA_HazmatShipment_MaterialIds_Json CHECK (ISJSON(MaterialIds) = 1),
        CONSTRAINT CK_PHMSA_HazmatShipment_PackagingIds_Json CHECK (ISJSON(PackagingIds) = 1),
        CONSTRAINT CK_PHMSA_HazmatShipment_ModalReq_Json CHECK (ShippingPaperAdditionalModalRequirements IS NULL OR ISJSON(ShippingPaperAdditionalModalRequirements) = 1),
        CONSTRAINT CK_PHMSA_HazmatShipment_PlacardingRequirements_Json CHECK (ISJSON(PlacardingRequirements) = 1),
        CONSTRAINT CK_PHMSA_HazmatShipment_SegregationViolations_Json CHECK (ISJSON(SegregationViolations) = 1)
    );

    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatShipment_TenantId_Site ON [Compliance].[PHMSA_HazmatShipment] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatShipment_Status ON [Compliance].[PHMSA_HazmatShipment] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatShipment_CreatedAtUtc ON [Compliance].[PHMSA_HazmatShipment] (CreatedAtUtc);
    CREATE NONCLUSTERED INDEX IX_PHMSA_HazmatShipment_TenantId ON [Compliance].[PHMSA_HazmatShipment] (TenantId);
END
GO
