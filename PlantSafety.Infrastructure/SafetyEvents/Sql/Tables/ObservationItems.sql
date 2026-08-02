-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.Observation
--  Layer:          Infrastructure – SQL Scripts
--  File:           ObservationItems.sql
--  Created:        2026-07-13
--  Updated:        2026-08-02 -- Phase 4 full build-out: additively extended with the columns
--                  needed to persist the full Observation aggregate write path
--                  (Location/Description/CorrectiveActionRequired/FollowUpDueAtUtc/UpdatedAtUtc/
--                  UpdatedBy/IsDeleted/RowVersion), backing the new
--                  usp_ObservationItems_Insert/_Update/_GetById procedures and
--                  IObservationRepository (Domain write-side aggregate repository).
--                  Pre-existing columns are reused for overlapping aggregate properties rather
--                  than duplicated: Category serves as HazardCategory, ObservedAtUtc serves as
--                  CreatedAtUtc, and ObservedBy serves as both ObserverId and CreatedBy (the
--                  observer is always the record's creator per the Create factory). All ALTER
--                  guards are idempotent (IF NOT EXISTS on sys.columns).
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Observation read-model, backing
--                  IObservationRepository.GetObservationsAsync (read) and, following this
--                  update, the full aggregate write path (Insert/Update/GetById). Named
--                  ObservationItems (not Observation) to avoid colliding with the
--                  unrelated [SafetyEvents].[usp_Observation_*] procedures already
--                  referenced by the separate Features-tree ObservationRepository
--                  in this same module.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ObservationItems' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[ObservationItems] (
        ObservationId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        ObservationType NVARCHAR(50)     NOT NULL,
        Category        NVARCHAR(50)     NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        ObservedAtUtc   DATETIME2(3)     NOT NULL,
        ObservedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_ObservationItems_TenantId_SiteId] ON [SafetyEvents].[ObservationItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_ObservationItems_ObservedAtUtc] ON [SafetyEvents].[ObservationItems] (ObservedAtUtc);
END
GO

-- ---- Phase 4 additive columns (aggregate write path) ------------------------

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[ObservationItems]') AND name = 'Location')
    ALTER TABLE [SafetyEvents].[ObservationItems] ADD Location NVARCHAR(400) NOT NULL CONSTRAINT DF_ObservationItems_Location DEFAULT ('');
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[ObservationItems]') AND name = 'Description')
    ALTER TABLE [SafetyEvents].[ObservationItems] ADD Description NVARCHAR(2000) NOT NULL CONSTRAINT DF_ObservationItems_Description DEFAULT ('');
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[ObservationItems]') AND name = 'CorrectiveActionRequired')
    ALTER TABLE [SafetyEvents].[ObservationItems] ADD CorrectiveActionRequired BIT NOT NULL CONSTRAINT DF_ObservationItems_CorrectiveActionRequired DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[ObservationItems]') AND name = 'FollowUpDueAtUtc')
    ALTER TABLE [SafetyEvents].[ObservationItems] ADD FollowUpDueAtUtc DATETIME2(3) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[ObservationItems]') AND name = 'UpdatedAtUtc')
    ALTER TABLE [SafetyEvents].[ObservationItems] ADD UpdatedAtUtc DATETIME2(3) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[ObservationItems]') AND name = 'UpdatedBy')
    ALTER TABLE [SafetyEvents].[ObservationItems] ADD UpdatedBy UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[ObservationItems]') AND name = 'IsDeleted')
    ALTER TABLE [SafetyEvents].[ObservationItems] ADD IsDeleted BIT NOT NULL CONSTRAINT DF_ObservationItems_IsDeleted DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[ObservationItems]') AND name = 'RowVersion')
    ALTER TABLE [SafetyEvents].[ObservationItems] ADD RowVersion ROWVERSION NOT NULL;
GO
