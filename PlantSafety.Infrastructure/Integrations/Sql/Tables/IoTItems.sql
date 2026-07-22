-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.IoT
--  Layer:          Infrastructure – SQL Scripts
--  File:           IoTItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.IoT read-model, backing
--                  IIoTRepository.GetIoTReadingsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IoTItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[IoTItems] (
        ReadingId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId       UNIQUEIDENTIFIER NOT NULL,
        SiteId         UNIQUEIDENTIFIER NULL,
        DeviceId       UNIQUEIDENTIFIER NOT NULL,
        SensorType     NVARCHAR(100)    NOT NULL,
        ReadingValue   FLOAT            NOT NULL,
        Status         NVARCHAR(50)     NOT NULL,
        RecordedAtUtc  DATETIME2(3)     NOT NULL,
        IngestedBy     UNIQUEIDENTIFIER NOT NULL,
        IsDeleted      BIT              NOT NULL CONSTRAINT DF_IoTItems_IsDeleted DEFAULT (0),
        CreatedUtc     DATETIME2(3)     NOT NULL CONSTRAINT DF_IoTItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy      UNIQUEIDENTIFIER NULL,
        UpdatedUtc     DATETIME2(3)     NULL,
        UpdatedBy      UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_IoTItems_TenantId_SiteId] ON [Integrations].[IoTItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_IoTItems_RecordedAtUtc] ON [Integrations].[IoTItems] (RecordedAtUtc);
    CREATE NONCLUSTERED INDEX [IX_IoTItems_DeviceId] ON [Integrations].[IoTItems] (DeviceId);
END
GO
