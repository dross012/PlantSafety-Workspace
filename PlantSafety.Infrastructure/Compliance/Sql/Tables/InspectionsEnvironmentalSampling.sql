-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.InspectionsEnvironmentalSampling
--  Layer:          Infrastructure - SQL Scripts
--  File:           InspectionsEnvironmentalSampling.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the InspectionsEnvironmentalSampling read-model aggregate backing
--                  IEnvironmentalSamplingRepository.GetInspectionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsEnvironmentalSampling' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InspectionsEnvironmentalSampling] (
        [InspectionId]            UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [SampleType]              NVARCHAR(300)       NOT NULL,
        [ScheduledAtUtc]          DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsEnvironmentalSampling_TenantId_ScheduledAtUtc]
        ON [Compliance].[InspectionsEnvironmentalSampling] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
GO