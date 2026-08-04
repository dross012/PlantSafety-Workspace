-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HazardousWasteManagement.DisposalCertificates
--  Layer:          Infrastructure - SQL Scripts
--  File:           DisposalCertificates.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.2
--  Description:    Table DDL for the DisposalCertificate aggregate root backing
--                  IDisposalCertificateRepository. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HazardousWasteManagement')
BEGIN
    EXEC('CREATE SCHEMA [HazardousWasteManagement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DisposalCertificates' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
BEGIN
    CREATE TABLE [HazardousWasteManagement].[DisposalCertificates] (
        [DisposalCertificateId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ManifestId] UNIQUEIDENTIFIER NOT NULL,
        [TsdfFacilityId] UNIQUEIDENTIFIER NOT NULL,
        [DisposalMethod] NVARCHAR(100) NOT NULL,
        [DisposalDateUtc] DATETIME2(3) NOT NULL,
        [CertificateReceivedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_DisposalCertificates_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DisposalCertificates_ManifestId]
        ON [HazardousWasteManagement].[DisposalCertificates] ([ManifestId]);
END
GO
