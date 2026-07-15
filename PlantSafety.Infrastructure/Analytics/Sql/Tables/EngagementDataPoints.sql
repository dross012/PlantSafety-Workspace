-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Engagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           EngagementDataPoints.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EngagementDataPoints read-model aggregate backing
--                  IEngagementRepository.GetDataPointsAsync. The DTO
--                  (EngagementDataPointDto) is a positional record with only
--                  Label/Value/Trend, so this table additionally carries a
--                  surrogate DataPointId primary key plus TenantId/SiteId/
--                  RecordedAtUtc filter columns that are not projected into the
--                  DTO itself but are required to answer the tenant/site/date-range
--                  scoped query.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EngagementDataPoints' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[EngagementDataPoints] (
        [DataPointId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [Label]              NVARCHAR(300)       NOT NULL,
        [Value]              DECIMAL(18,4)       NOT NULL,
        [Trend]              NVARCHAR(50)        NULL,
        [RecordedAtUtc]      DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EngagementDataPoints_TenantId_RecordedAtUtc]
        ON [Analytics].[EngagementDataPoints] ([TenantId], [RecordedAtUtc])
        INCLUDE ([SiteId], [Label]);
END
GO
