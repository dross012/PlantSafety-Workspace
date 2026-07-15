-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.DigitalTwin
--  Layer:          Infrastructure - SQL Scripts
--  File:           Twin.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DigitalTwin asset registry backing IDigitalTwinRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Twin' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[Twin] (
        [TwinId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]               UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                 UNIQUEIDENTIFIER    NULL,
        [AssetName]              NVARCHAR(200)       NOT NULL,
        [AssetType]              NVARCHAR(50)        NOT NULL,
        [Status]                 NVARCHAR(50)        NOT NULL CONSTRAINT DF_Twin_Status DEFAULT ('Provisioning'),
        [LastSyncAtUtc]          DATETIME2(3)        NOT NULL,
        [TelemetryHealthScore]   FLOAT               NOT NULL CONSTRAINT DF_Twin_HealthScore DEFAULT (0),
        [CreatedAtUtc]           DATETIME2(3)        NOT NULL,
        [CorrelationId]          UNIQUEIDENTIFIER    NOT NULL,
        [ActorId]                UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Twin_TenantId_SiteId]
        ON [DigitalTwin].[Twin] ([TenantId], [SiteId])
        INCLUDE ([AssetName], [AssetType], [Status], [LastSyncAtUtc]);
END
GO
