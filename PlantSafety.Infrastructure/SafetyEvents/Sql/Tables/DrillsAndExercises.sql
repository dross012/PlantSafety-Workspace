-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.DrillsAndExercises
--  Layer:          Infrastructure – SQL Scripts
--  File:           DrillsAndExercises.sql
--  Created:        2026-07-13
--  Updated:        2026-08-02 -- Phase 4 full build-out: this table previously backed only the
--                  read-only list query (IDrillsAndExercisesRepository.GetDrillsAsync). It now
--                  additionally backs the DrillsAndExercises aggregate root write model
--                  (Domain.SafetyEvents.DrillsAndExercises.Interfaces.IDrillsAndExercisesRepository).
--                  Extended additively: every pre-existing column is untouched in shape except
--                  ConductedAtUtc/ConductedBy, whose nullability is relaxed to NULL (a drill can
--                  be Scheduled, before ever being conducted -- the pre-existing NOT NULL
--                  constraint was only ever satisfiable by the list read model's implicit
--                  assumption that every row was already conducted). New lifecycle-attribute/
--                  audit/concurrency columns (ScheduledAtUtc, CompletedBy/CompletedAtUtc/
--                  DebriefNotes, CancelledBy/CancelledReason/CancelledAtUtc, ClosedBy/ClosedAtUtc,
--                  ArchivedAtUtc, CreatedAtUtc/CreatedBy, UpdatedAtUtc/UpdatedBy, IsDeleted,
--                  RowVersion) are added via idempotent ALTER TABLE ... ADD guarded by
--                  sys.columns existence checks, mirroring [Compliance].[Permits]'s native
--                  ROWVERSION concurrency convention. DrillId remains the physical primary-key
--                  column name (aliased to DrillsAndExercisesId in usp_DrillsAndExercises_GetById
--                  / write procs' parameter mapping) to avoid disturbing the pre-existing list
--                  proc/read model.
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the DrillsAndExercises read-model, backing
--                  IDrillsAndExercisesRepository.GetDrillsAsync (list) and the write-side
--                  aggregate repository (GetByIdAsync/AddAsync/UpdateAsync).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DrillsAndExercises' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[DrillsAndExercises] (
        DrillId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        DrillType        NVARCHAR(50)     NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        ConductedAtUtc   DATETIME2(3)     NULL,
        ConductedBy      UNIQUEIDENTIFIER NULL,
        ParticipantCount INT              NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_DrillsAndExercises_TenantId_SiteId] ON [SafetyEvents].[DrillsAndExercises] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_DrillsAndExercises_ConductedAtUtc] ON [SafetyEvents].[DrillsAndExercises] (ConductedAtUtc);
END
GO

-- ---- Phase 4 additive extension: relax legacy NOT NULL constraints that predate scheduling ----
IF EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'DrillsAndExercises' AND t.schema_id = SCHEMA_ID('SafetyEvents') AND c.name = 'ConductedAtUtc' AND c.is_nullable = 0)
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ALTER COLUMN ConductedAtUtc DATETIME2(3) NULL;
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'DrillsAndExercises' AND t.schema_id = SCHEMA_ID('SafetyEvents') AND c.name = 'ConductedBy' AND c.is_nullable = 0)
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ALTER COLUMN ConductedBy UNIQUEIDENTIFIER NULL;
GO

-- ---- Phase 4 additive extension: aggregate lifecycle attributes + audit/concurrency columns ----
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'ScheduledAtUtc')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD ScheduledAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'CompletedBy')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD CompletedBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'CompletedAtUtc')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD CompletedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'DebriefNotes')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD DebriefNotes NVARCHAR(2000) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'CancelledBy')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD CancelledBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'CancelledReason')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD CancelledReason NVARCHAR(2000) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'CancelledAtUtc')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD CancelledAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'ClosedBy')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD ClosedBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'ClosedAtUtc')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD ClosedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'ArchivedAtUtc')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD ArchivedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'CreatedAtUtc')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD CreatedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'CreatedBy')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD CreatedBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'UpdatedAtUtc')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD UpdatedAtUtc DATETIME2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'UpdatedBy')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD UpdatedBy UNIQUEIDENTIFIER NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'IsDeleted')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD IsDeleted BIT NOT NULL CONSTRAINT DF_DrillsAndExercises_IsDeleted DEFAULT (0);
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]') AND name = 'RowVersion')
    ALTER TABLE [SafetyEvents].[DrillsAndExercises] ADD RowVersion ROWVERSION;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DrillsAndExercises_CreatedAtUtc' AND object_id = OBJECT_ID('[SafetyEvents].[DrillsAndExercises]'))
    CREATE NONCLUSTERED INDEX [IX_DrillsAndExercises_CreatedAtUtc] ON [SafetyEvents].[DrillsAndExercises] (CreatedAtUtc);
GO
