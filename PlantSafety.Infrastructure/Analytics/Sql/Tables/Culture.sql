-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Culture
--  Layer:          Infrastructure – SQL Scripts
--  File:           Culture.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Culture aggregate root, backing
--                  ICultureRepository.AddAsync/GetByIdAsync/GetAllAsync
--                  (CultureRepository, Analytics.Culture.Persistence). This is the only real
--                  write path in the Analytics module -- every other Analytics stored procedure
--                  under this folder is a read-only "_List" query. Neither this table nor its
--                  stored procedures existed anywhere in the codebase before this pass, even
--                  though CultureRepository referenced them by name -- CreateCultureCommand could
--                  never have succeeded against a real database until now.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Culture' AND schema_id = SCHEMA_ID('Analytics'))
BEGIN
    CREATE TABLE [Analytics].[Culture] (
        CultureId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Culture PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NULL,
        Status              NVARCHAR(30)     NOT NULL,
        SurveyPeriod        NVARCHAR(20)     NOT NULL,
        CultureIndexScore   DECIMAL(5,2)     NOT NULL,
        EngagementScore     DECIMAL(5,2)     NOT NULL,
        ParticipationRate   DECIMAL(5,2)     NOT NULL,
        TopRiskTheme        NVARCHAR(200)    NOT NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL,
        CreatedBy           UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Culture_TenantId] ON [Analytics].[Culture] (TenantId);
END
GO
