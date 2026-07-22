-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.DigitalTwin
--  Layer:          Infrastructure – SQL Scripts
--  File:           DigitalTwin.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DigitalTwin asset registry, backing
--                  IDigitalTwinRepository (usp_Twin_Create/GetById/List).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'DigitalTwin')
BEGIN
    EXEC('CREATE SCHEMA [DigitalTwin]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Twin' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[Twin] (
        TwinId                UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        AssetName              NVARCHAR(200)    NOT NULL,
        AssetType              NVARCHAR(50)     NOT NULL,
        Status                 NVARCHAR(50)     NOT NULL,
        LastSyncAtUtc          DATETIME2(3)     NOT NULL,
        TelemetryHealthScore   FLOAT            NOT NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_DigitalTwin_Twin_IsDeleted DEFAULT (0),
        CorrelationId          UNIQUEIDENTIFIER NULL,
        CreatedAtUtc           DATETIME2(3)     NOT NULL CONSTRAINT DF_DigitalTwin_Twin_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy              UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc          DATETIME2(3)     NULL,
        ModifiedBy             UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DigitalTwin_Twin_TenantId_SiteId] ON [DigitalTwin].[Twin] (TenantId, SiteId);
END
GO
