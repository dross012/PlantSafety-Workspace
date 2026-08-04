-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HazardousWasteManagement.Transporters
--  Layer:          Infrastructure - SQL Scripts
--  File:           Transporters.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.2
--  Description:    Table DDL for the Transporter aggregate root backing ITransporterRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HazardousWasteManagement')
BEGIN
    EXEC('CREATE SCHEMA [HazardousWasteManagement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Transporters' AND schema_id = SCHEMA_ID('HazardousWasteManagement'))
BEGIN
    CREATE TABLE [HazardousWasteManagement].[Transporters] (
        [TransporterId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [CompanyName] NVARCHAR(400) NOT NULL,
        [EpaTransporterId] NVARCHAR(50) NOT NULL,
        [LicenseExpiresAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_Transporters_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Transporters_TenantId]
        ON [HazardousWasteManagement].[Transporters] ([TenantId])
        INCLUDE ([Status]);
END
GO
