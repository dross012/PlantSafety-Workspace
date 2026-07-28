-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Tracking
--  Layer:          Infrastructure - SQL Scripts
--  File:           Tracking.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Tracking read-model aggregate backing
--                  ITrackingRepository.GetTrackingAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Tracking' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[Tracking] (
        [TrackingItemId]          UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ItemDescription]         NVARCHAR(300)       NOT NULL,
        [Category]                NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [LastUpdatedAtUtc]        DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Tracking_TenantId_LastUpdatedAtUtc]
        ON [Compliance].[Tracking] ([TenantId], [LastUpdatedAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[Tracking]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[Tracking] ADD RowVersion ROWVERSION NOT NULL;
END
GO