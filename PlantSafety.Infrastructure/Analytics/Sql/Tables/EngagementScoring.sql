-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture.EngagementScoring
--  Layer:          Infrastructure - SQL Scripts
--  File:           EngagementScoring.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the EngagementScoring read-model aggregate backing
--                  IEngagementScoringRepository.GetEngagementScoresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EngagementScoring' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[EngagementScoring] (
        [ScoreId]           UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [EmployeeId]        UNIQUEIDENTIFIER    NOT NULL,
        [EngagementScore]   DECIMAL(18,4)       NOT NULL,
        [ScoreCategory]     NVARCHAR(100)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EngagementScoring_TenantId_ScoredAtUtc]
        ON [Analytics].[EngagementScoring] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [ScoreCategory]);
END
GO
