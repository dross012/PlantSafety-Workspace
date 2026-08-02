-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin
--  Layer:          Infrastructure -- SQL Scripts
--  File:           DigitalTwinAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the DigitalTwin aggregate root (IDigitalTwinRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier). Named "DigitalTwinAggregate"
--                  (not bare "DigitalTwin") to avoid any ambiguity with the pre-existing, unrelated
--                  Features.DigitalTwin asset-registry table ([DigitalTwin].[Twin], procs
--                  usp_Twin_Create/GetById/List) that already lives under the [DigitalTwin] schema
--                  and is live-wired into DI. This is a distinct DDD aggregate: a digital-twin
--                  model registry record. Phase 4 full build-out: this aggregate previously had
--                  zero persistence at all. Mirrors [Compliance].[AuditingAggregate]'s native
--                  ROWVERSION concurrency convention from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'DigitalTwin')
    EXEC('CREATE SCHEMA DigitalTwin AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DigitalTwinAggregate' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[DigitalTwinAggregate]
    (
        DigitalTwinId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DigitalTwinAggregate PRIMARY KEY,
        TenantId                  UNIQUEIDENTIFIER NOT NULL,
        SiteId                    UNIQUEIDENTIFIER NULL,
        Status                    NVARCHAR(50)     NOT NULL,
        TwinName                  NVARCHAR(200)    NOT NULL,
        AssetReferenceId          UNIQUEIDENTIFIER NULL,
        ModelType                 NVARCHAR(100)    NOT NULL,
        LastSyncedAtUtc           DATETIME2(3)     NULL,
        CreatedAtUtc              DATETIME2(3)     NOT NULL,
        CreatedBy                 UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc              DATETIME2(3)     NULL,
        UpdatedBy                 UNIQUEIDENTIFIER NULL,
        IsDeleted                 BIT              NOT NULL CONSTRAINT DF_DigitalTwinAggregate_IsDeleted DEFAULT (0),
        RowVersion                ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_DigitalTwinAggregate_TenantId_Site ON [DigitalTwin].[DigitalTwinAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_DigitalTwinAggregate_Status ON [DigitalTwin].[DigitalTwinAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_DigitalTwinAggregate_CreatedAtUtc ON [DigitalTwin].[DigitalTwinAggregate] (CreatedAtUtc);
END
GO
