-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards.Department
--  Layer:          Infrastructure - SQL Scripts
--  File:           Department.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Department read-model aggregate backing
--                  IDepartmentRepository.GetDepartmentScoresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Department' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Department] (
        [ScoreCardId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [DepartmentName]    NVARCHAR(200)       NOT NULL,
        [ScoreValue]        DECIMAL(18,4)       NOT NULL,
        [RankPercentile]    DECIMAL(18,4)       NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Department_TenantId_ScoredAtUtc]
        ON [Analytics].[Department] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [DepartmentName]);
END
GO
