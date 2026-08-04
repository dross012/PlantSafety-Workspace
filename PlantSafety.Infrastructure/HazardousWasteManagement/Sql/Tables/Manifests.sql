-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HazardousWasteManagement.Manifests
--  Layer:          Infrastructure - SQL Scripts
--  File:           Manifests.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.2
--  Description:    Table DDL for the HazardousWasteManifest aggregate root backing
--                  IHazardousWasteManifestRepository. RetentionExpiresAtUtc and IsImmutable
--                  encode the data-governance invariants (3-year retention, immutable once
--                  transporter-signed). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HazardousWasteManagement')
BEGIN
    EXEC('CREATE SCHEMA [HazardousWasteManagement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Manifests' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
BEGIN
    CREATE TABLE [HazardousWasteManagement].[Manifests] (
        [HazardousWasteManifestId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ManifestNumber] NVARCHAR(50) NOT NULL,
        [GeneratorEpaId] NVARCHAR(50) NOT NULL,
        [WasteStreamId] UNIQUEIDENTIFIER NOT NULL,
        [TransporterId] UNIQUEIDENTIFIER NULL,
        [TsdfFacilityId] UNIQUEIDENTIFIER NULL,
        [GeneratedAtUtc] DATETIME2(3) NOT NULL,
        [TransporterSignedAtUtc] DATETIME2(3) NULL,
        [TsdfReceivedAtUtc] DATETIME2(3) NULL,
        [ClosedAtUtc] DATETIME2(3) NULL,
        [IsImmutable] BIT NOT NULL CONSTRAINT DF_Manifests_IsImmutable DEFAULT (0),
        [DiscrepancyReason] NVARCHAR(2000) NULL,
        [RetentionExpiresAtUtc] DATETIME2(3) NOT NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_Manifests_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_Manifests_ManifestNumber]
        ON [HazardousWasteManagement].[Manifests] ([TenantId], [ManifestNumber]);

    CREATE NONCLUSTERED INDEX [IX_Manifests_TenantId_Status]
        ON [HazardousWasteManagement].[Manifests] ([TenantId], [Status])
        INCLUDE ([SiteId], [GeneratedAtUtc]);
END
GO
