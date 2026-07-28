-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards.Training
--  Layer:          Infrastructure - SQL Scripts
--  File:           ScoreCardsTraining.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ScoreCards.Training read-model aggregate
--                  backing ITrainingRepository.GetTrainingScoresAsync. Named
--                  ScoreCardsTraining (rather than bare Training) purely for SQL
--                  object clarity; the [Analytics] schema already isolates it from
--                  any Compliance/Certification.Training or
--                  RegulatoryCalendar.Renewals.Training object.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ScoreCardsTraining' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[ScoreCardsTraining] (
        [ScoreCardId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [EmployeeId]        UNIQUEIDENTIFIER    NOT NULL,
        [CompletionRate]    DECIMAL(18,4)       NOT NULL,
        [CoursesCompleted]  INT                 NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL,
        [RowVersion]        ROWVERSION          NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ScoreCardsTraining_TenantId_ScoredAtUtc]
        ON [Analytics].[ScoreCardsTraining] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [EmployeeId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[ScoreCardsTraining]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Analytics].[ScoreCardsTraining] ADD RowVersion ROWVERSION NOT NULL;
END
GO
