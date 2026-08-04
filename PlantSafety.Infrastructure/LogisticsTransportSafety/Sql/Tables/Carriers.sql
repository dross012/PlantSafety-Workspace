-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         LogisticsTransportSafety.Carriers
--  Layer:          Infrastructure - SQL Scripts
--  File:           Carriers.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code sql-database-skill agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Carrier aggregate root backing ICarrierRepository (Domain
--                  write repository) and the Application-layer read-model repository of the same
--                  simple name. Column list is column-for-column with
--                  CarrierDapperRepository.CarrierRow. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'LogisticsTransportSafety')
BEGIN
    EXEC('CREATE SCHEMA [LogisticsTransportSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Carriers' AND schema_id = SCHEMA_ID('LogisticsTransportSafety'))
BEGIN
    CREATE TABLE [LogisticsTransportSafety].[Carriers] (
        [CarrierId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [CarrierName] NVARCHAR(200) NOT NULL,
        [CarrierType] NVARCHAR(50) NOT NULL,
        [InsuranceExpiryAtUtc] DATETIME2(3) NULL,
        [SafetyRatingScore] DECIMAL(5,2) NULL,
        [QualifiedAtUtc] DATETIME2(3) NULL,
        [SuspensionReason] NVARCHAR(1000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_Carriers_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Carriers_TenantId_SiteId]
        ON [LogisticsTransportSafety].[Carriers] ([TenantId], [SiteId])
        INCLUDE ([Status], [CarrierName], [CarrierType], [SafetyRatingScore]);

    CREATE NONCLUSTERED INDEX [IX_Carriers_TenantId_CreatedAtUtc]
        ON [LogisticsTransportSafety].[Carriers] ([TenantId], [CreatedAtUtc])
        INCLUDE ([SiteId], [Status], [CarrierName], [CarrierType], [SafetyRatingScore]);
END
GO
