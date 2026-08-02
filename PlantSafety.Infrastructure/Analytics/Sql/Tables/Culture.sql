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
--  UPDATED:        2026-07-27 -- added RowVersion (native ROWVERSION), following the
--                  SafetyEvents.Incident/ContinuousImprovement.CorrectiveAction/ProcessManagement.
--                  Process precedent: the canonical concurrency mechanism for new modules per
--                  docs/architecture/sql-error-handling-modernization-plan.md Sec.2. No update
--                  path exists yet for Culture (ICultureRepository is Create/Read-only today), so
--                  nothing currently enforces it in a WHERE clause -- it is added now so the
--                  column exists correctly-shaped from day one rather than being retrofitted once
--                  an UpdateAsync is eventually added.
--  UPDATED:        2026-08-02 -- Phase 4 full build-out: added the Review/Publish/Close/Archive
--                  lifecycle attribute columns and UpdatedAtUtc/UpdatedBy, backing the new
--                  ICultureRepository.UpdateAsync write path (usp_Culture_Update). Mirrors
--                  [Compliance].[Permits]'s identical lifecycle-column shape. New columns are added
--                  via idempotent ALTER for any environment where this table was already created
--                  under the pre-2026-08-02 shape.
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
        ReviewedByUserId    UNIQUEIDENTIFIER NULL,
        ReviewedAtUtc       DATETIME2(3)     NULL,
        PublishedByUserId   UNIQUEIDENTIFIER NULL,
        PublishedAtUtc      DATETIME2(3)     NULL,
        ClosedByUserId      UNIQUEIDENTIFIER NULL,
        ClosedAtUtc         DATETIME2(3)     NULL,
        ArchivedAtUtc       DATETIME2(3)     NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL,
        CreatedBy           UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc        DATETIME2(3)     NULL,
        UpdatedBy           UNIQUEIDENTIFIER NULL,
        RowVersion          ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Culture_TenantId] ON [Analytics].[Culture] (TenantId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Culture]') AND name = 'ReviewedByUserId')
    ALTER TABLE [Analytics].[Culture] ADD ReviewedByUserId UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Culture]') AND name = 'ReviewedAtUtc')
    ALTER TABLE [Analytics].[Culture] ADD ReviewedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Culture]') AND name = 'PublishedByUserId')
    ALTER TABLE [Analytics].[Culture] ADD PublishedByUserId UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Culture]') AND name = 'PublishedAtUtc')
    ALTER TABLE [Analytics].[Culture] ADD PublishedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Culture]') AND name = 'ClosedByUserId')
    ALTER TABLE [Analytics].[Culture] ADD ClosedByUserId UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Culture]') AND name = 'ClosedAtUtc')
    ALTER TABLE [Analytics].[Culture] ADD ClosedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Culture]') AND name = 'ArchivedAtUtc')
    ALTER TABLE [Analytics].[Culture] ADD ArchivedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Culture]') AND name = 'UpdatedAtUtc')
    ALTER TABLE [Analytics].[Culture] ADD UpdatedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Analytics].[Culture]') AND name = 'UpdatedBy')
    ALTER TABLE [Analytics].[Culture] ADD UpdatedBy UNIQUEIDENTIFIER NULL;
GO
