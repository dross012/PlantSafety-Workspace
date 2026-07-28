-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards.Compliance
--  Layer:          Infrastructure - SQL Scripts
--  File:           ScoreCardsCompliance.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ScoreCards.Compliance read-model aggregate
--                  backing IComplianceRepository.GetComplianceScoresAsync. Named
--                  ScoreCardsCompliance (rather than bare Compliance) to avoid any
--                  confusion with the unrelated [Compliance] schema owned by the
--                  Compliance module, even though the two are already isolated by
--                  schema ([Analytics] vs [Compliance]).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ScoreCardsCompliance' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[ScoreCardsCompliance] (
        [ScoreCardId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [ComplianceArea]    NVARCHAR(200)       NOT NULL,
        [ScoreValue]        DECIMAL(18,4)       NOT NULL,
        [MaxScore]          DECIMAL(18,4)       NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ScoreCardsCompliance_TenantId_ScoredAtUtc]
        ON [Analytics].[ScoreCardsCompliance] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [ComplianceArea]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[ScoreCardsCompliance]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[ScoreCardsCompliance] ADD RowVersion ROWVERSION NOT NULL;
END
GO
