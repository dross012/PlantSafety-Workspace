-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard.Executive
--  Layer:          Infrastructure - SQL Scripts
--  File:           Executive.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Executive read-model aggregate backing
--                  IExecutiveRepository.GetExecutiveSummaryAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Executive' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Executive] (
        [SummaryId]         UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [MetricName]        NVARCHAR(200)       NOT NULL,
        [MetricValue]       DECIMAL(18,4)       NOT NULL,
        [TargetValue]       DECIMAL(18,4)       NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [SnapshotAtUtc]     DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Executive_TenantId_SnapshotAtUtc]
        ON [Analytics].[Executive] ([TenantId], [SnapshotAtUtc])
        INCLUDE ([SiteId], [Status], [MetricName]);
END
GO
