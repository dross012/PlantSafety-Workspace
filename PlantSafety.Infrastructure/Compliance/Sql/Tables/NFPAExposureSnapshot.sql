-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.NFPAStandards.Dashboards
--  Layer:          Infrastructure - SQL Scripts
--  File:           NFPAExposureSnapshot.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Point-in-time NFPA guidance-coverage snapshots, one row per (TenantId,
--                  SiteId, Module, SnapshotAtUtc) -- the real data behind "coverage trend over
--                  time". Mirrors Compliance.OSHAExposureSnapshot's shape (generic
--                  metric-name + numeric-value(s) + UTC timestamp), the established precedent
--                  for this exact kind of tile in this codebase.
--                  SiteId NULL means a tenant-wide snapshot (all sites combined).
--                  Rows are written as a side effect of the NFPA coverage dashboard queries
--                  whenever a dashboard is actually viewed -- there is no background sweep
--                  enumerating every tenant/site/module combination, so trend history
--                  accumulates from real usage, not a fabricated backfill. There is no
--                  historical data before a site/module pair's first dashboard view.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NFPAExposureSnapshot' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NFPAExposureSnapshot] (
        [SnapshotId]        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_NFPAExposureSnapshot_SnapshotId DEFAULT (NEWID()) CONSTRAINT PK_NFPAExposureSnapshot PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER NULL,
        [Module]            NVARCHAR(100)    NOT NULL,
        [TotalHazards]      INT              NOT NULL,
        [CoveredCount]      INT              NOT NULL,
        [GapCount]          INT              NOT NULL,
        [HighGapCount]      INT              NOT NULL,
        [CriticalGapCount]  INT              NOT NULL,
        [SnapshotAtUtc]     DATETIME2(3)     NOT NULL CONSTRAINT DF_NFPAExposureSnapshot_SnapshotAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE NONCLUSTERED INDEX [IX_NFPAExposureSnapshot_Tenant_Site_Module_SnapshotAtUtc]
        ON [Compliance].[NFPAExposureSnapshot] ([TenantId], [SiteId], [Module], [SnapshotAtUtc]);
END
GO
