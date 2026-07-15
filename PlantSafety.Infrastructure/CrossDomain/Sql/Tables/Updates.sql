-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Updates
--  Layer:          Infrastructure - SQL Scripts
--  File:           Updates.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Updates read-model, backing
--                  IUpdatesRepository.GetUpdatesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Updates' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Updates] (
        UpdateId           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NULL,
        ComponentName        NVARCHAR(200)    NOT NULL,
        VersionFrom           NVARCHAR(50)     NOT NULL,
        VersionTo              NVARCHAR(50)     NOT NULL,
        Status                NVARCHAR(50)     NOT NULL,
        AppliedAtUtc          DATETIME2(3)     NOT NULL,
        AppliedBy             UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Updates_TenantId_SiteId ON [CrossDomain].[Updates] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Updates_AppliedAtUtc ON [CrossDomain].[Updates] (AppliedAtUtc);
END
GO
