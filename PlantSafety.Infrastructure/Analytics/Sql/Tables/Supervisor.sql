-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.ScoreCards.Supervisor
--  Layer:          Infrastructure - SQL Scripts
--  File:           Supervisor.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Supervisor read-model aggregate backing
--                  ISupervisorRepository.GetSupervisorScoresAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Supervisor' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Supervisor] (
        [ScoreCardId]       UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]          UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]            UNIQUEIDENTIFIER    NULL,
        [SupervisorId]      UNIQUEIDENTIFIER    NOT NULL,
        [ScoreValue]        DECIMAL(18,4)       NOT NULL,
        [TeamSize]          INT                 NOT NULL,
        [Status]            NVARCHAR(50)        NOT NULL,
        [ScoredAtUtc]       DATETIME2(3)        NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Supervisor_TenantId_ScoredAtUtc]
        ON [Analytics].[Supervisor] ([TenantId], [ScoredAtUtc])
        INCLUDE ([SiteId], [Status], [SupervisorId]);
END
GO
