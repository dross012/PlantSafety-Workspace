-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.EnvironmentalMonitoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           EnvironmentalMonitoringReadModel.sql
--  Created:        2026-08-20
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EnvironmentalMonitoringReadModel read-model aggregate backing
--                  IEnvironmentalMonitoringRepository.GetEnvironmentalMonitoringAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EnvironmentalMonitoringReadModel' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[EnvironmentalMonitoringReadModel] (
        [EnvironmentalMonitoringRecordId]UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [SensorId]                UNIQUEIDENTIFIER    NOT NULL,
        [SensorName]              NVARCHAR(400)       NOT NULL,
        [ReadingType]             NVARCHAR(400)       NOT NULL,
        [ReadingValue]            DECIMAL(18,4)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [RecordedAtUtc]           DATETIME2(3)        NOT NULL,
        [RecordedBy]              UNIQUEIDENTIFIER    NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EnvironmentalMonitoringReadModel_TenantId_RecordedAtUtc]
        ON [Operational].[EnvironmentalMonitoringReadModel] ([TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Operational].[EnvironmentalMonitoringReadModel]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Operational].[EnvironmentalMonitoringReadModel] ADD RowVersion ROWVERSION NOT NULL;
END
GO