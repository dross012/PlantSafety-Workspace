-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Dashboard
--  Layer:          Infrastructure -- SQL Scripts
--  File:           Dashboard.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the Dashboard aggregate root (IDashboardRepository). Tenant-scoped
--                  (TenantId NOT NULL uniqueidentifier). Phase 4 full build-out: this aggregate
--                  previously had zero persistence at all -- no table, no stored procedures, and no
--                  Application-layer command ever loaded or mutated it. Mirrors
--                  [Analytics].[Culture]'s / [Compliance].[Permits]' native ROWVERSION concurrency
--                  convention from day one.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Dashboard' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Dashboard] (
        DashboardId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Dashboard PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        Status                  NVARCHAR(30)     NOT NULL,
        DashboardName           NVARCHAR(400)    NOT NULL,
        DashboardType           NVARCHAR(100)    NOT NULL,
        OwnerId                 UNIQUEIDENTIFIER NULL,
        RefreshIntervalSeconds  INT              NOT NULL CONSTRAINT DF_Dashboard_RefreshIntervalSeconds DEFAULT (60),
        WidgetCount             INT              NOT NULL CONSTRAINT DF_Dashboard_WidgetCount DEFAULT (0),
        IsPublished             BIT              NOT NULL CONSTRAINT DF_Dashboard_IsPublished DEFAULT (0),
        LastRefreshedAtUtc      DATETIME2(3)     NULL,
        PublishedByUserId       UNIQUEIDENTIFIER NULL,
        PublishedAtUtc          DATETIME2(3)     NULL,
        DeprecatedByUserId      UNIQUEIDENTIFIER NULL,
        DeprecatedAtUtc         DATETIME2(3)     NULL,
        ClosedByUserId          UNIQUEIDENTIFIER NULL,
        ClosedAtUtc             DATETIME2(3)     NULL,
        ArchivedAtUtc           DATETIME2(3)     NULL,
        CreatedAtUtc            DATETIME2(3)     NOT NULL,
        CreatedBy               UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc            DATETIME2(3)     NULL,
        UpdatedBy               UNIQUEIDENTIFIER NULL,
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Dashboard_TenantId_Site] ON [Analytics].[Dashboard] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_Dashboard_Status] ON [Analytics].[Dashboard] (Status);
    CREATE NONCLUSTERED INDEX [IX_Dashboard_CreatedAtUtc] ON [Analytics].[Dashboard] (CreatedAtUtc);
END
GO
