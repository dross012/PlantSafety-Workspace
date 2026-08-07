-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards.Dashboards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           OSHAExposureSnapshot.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Point-in-time OSHA coverage snapshots, one row per (TenantId, SiteId, Module,
--                  SnapshotAtUtc) -- the real data behind "exposure trend over time". Shape
--                  mirrors [Analytics].[KPISnapshots] (generic metric-name + numeric-value(s) +
--                  UTC timestamp), the established precedent for this exact kind of tile in this
--                  codebase.
--                  SiteId NULL means a tenant-wide snapshot (all sites combined).
--                  Rows are written as a side effect of GetOshaSiteExposureQuery/
--                  GetOshaTenantExposureQuery (Compliance.OSHAStandards.Dashboards) whenever a
--                  dashboard is actually viewed -- there is no background sweep enumerating every
--                  tenant/site/module combination (no registry of which modules are relevant to
--                  which site exists anywhere in this codebase to drive one), so trend history
--                  accumulates from real usage, not a fabricated backfill. There is no historical
--                  data before a site/module pair's first dashboard view.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OSHAExposureSnapshot' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[OSHAExposureSnapshot] (
        [SnapshotId]         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OSHAExposureSnapshot PRIMARY KEY,
        [TenantId]           UNIQUEIDENTIFIER NOT NULL,
        [SiteId]             UNIQUEIDENTIFIER NULL,
        [Module]             NVARCHAR(100)    NOT NULL,
        [TotalRequirements]  INT              NOT NULL,
        [CoveredCount]       INT              NOT NULL,
        [GapCount]           INT              NOT NULL,
        [HighGapCount]       INT              NOT NULL,
        [CriticalGapCount]   INT              NOT NULL,
        [SnapshotAtUtc]      DATETIME2(3)     NOT NULL CONSTRAINT DF_OSHAExposureSnapshot_SnapshotAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE NONCLUSTERED INDEX [IX_OSHAExposureSnapshot_Tenant_Site_Module_SnapshotAtUtc]
        ON [Compliance].[OSHAExposureSnapshot] ([TenantId], [SiteId], [Module], [SnapshotAtUtc]);
END
GO
