-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth
--  Layer:          Infrastructure - SQL Scripts
--  File:           EnvironmentalHealthReadings.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EnvironmentalHealth aggregate backing
--                  IEnvironmentalHealthReadingRepository.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
BEGIN
    EXEC('CREATE SCHEMA [EnvironmentalHealth]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EnvironmentalHealthReadings' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
BEGIN
    CREATE TABLE [EnvironmentalHealth].[EnvironmentalHealthReadings] (
        [EnvironmentalHealthId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]              UNIQUEIDENTIFIER NOT NULL,
        [SiteId]                UNIQUEIDENTIFIER NULL,
        [MetricType]            NVARCHAR(50)     NOT NULL,
        [MeasurementValue]      DECIMAL(18,4)    NOT NULL,
        [Unit]                  NVARCHAR(20)     NOT NULL,
        [Threshold]             DECIMAL(18,4)    NOT NULL,
        [Status]                NVARCHAR(20)     NOT NULL CONSTRAINT DF_EHR_Status DEFAULT ('Normal'),
        [RecordedAtUtc]         DATETIME2(3)     NOT NULL,
        [CreatedBy]             UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt]             DATETIME2(3)     NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EnvironmentalHealthReadings_TenantId_RecordedAtUtc]
        ON [EnvironmentalHealth].[EnvironmentalHealthReadings] ([TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId], [MetricType], [Status]);
END
GO
