-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture.CultureIndex
--  Layer:          Infrastructure - SQL Scripts
--  File:           CultureIndex.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CultureIndex read-model aggregate backing
--                  ICultureIndexRepository.GetCultureIndexSnapshotsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CultureIndex' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[CultureIndex] (
        [IndexSnapshotId]   UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [CultureDimension]  NVARCHAR(200)       NOT NULL,
        [IndexScore]        DECIMAL(18,4)       NOT NULL,
        [TrendDirection]    NVARCHAR(50)        NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [CalculatedAtUtc]   DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CultureIndex_TenantId_CalculatedAtUtc]
        ON [Analytics].[CultureIndex] ([TenantId], [CalculatedAtUtc])
        INCLUDE ([SiteId], [Status], [CultureDimension]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[CultureIndex]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[CultureIndex] ADD RowVersion ROWVERSION NOT NULL;
END
GO
