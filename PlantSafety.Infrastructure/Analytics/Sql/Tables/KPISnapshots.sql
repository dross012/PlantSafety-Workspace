-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard.KPISnapshots
--  Layer:          Infrastructure - SQL Scripts
--  File:           KPISnapshots.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the KPISnapshots read-model aggregate backing
--                  IKPISnapshotsRepository.GetKpiSnapshotsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'KPISnapshots' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[KPISnapshots] (
        [SnapshotId]        UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [KpiName]           NVARCHAR(200)       NOT NULL,
        [KpiValue]          DECIMAL(18,4)       NOT NULL,
        [UnitOfMeasure]     NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [SnapshotAtUtc]     DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_KPISnapshots_TenantId_SnapshotAtUtc]
        ON [Analytics].[KPISnapshots] ([TenantId], [SnapshotAtUtc])
        INCLUDE ([SiteId], [Status], [KpiName]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[KPISnapshots]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[KPISnapshots] ADD RowVersion ROWVERSION NOT NULL;
END
GO
