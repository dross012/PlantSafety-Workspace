-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContinuousImprovement.RCA
--  Layer:          Infrastructure – SQL Scripts
--  File:           RCA.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RCA read-model, backing
--                  IRCARepository.GetRcaRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ContinuousImprovement')
BEGIN
    EXEC('CREATE SCHEMA [ContinuousImprovement]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RCA' AND schema_id = SCHEMA_ID('ContinuousImprovement'))
BEGIN
    CREATE TABLE [ContinuousImprovement].[RCA] (
        RcaId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        ProblemStatement  NVARCHAR(MAX)    NOT NULL,
        Methodology       NVARCHAR(50)     NOT NULL,
        Status            NVARCHAR(50)     NOT NULL,
        InitiatedAtUtc    DATETIME2(3)     NOT NULL,
        OwnerUserId       UNIQUEIDENTIFIER NOT NULL,
        IsDeleted         BIT              NOT NULL CONSTRAINT DF_RCA_IsDeleted DEFAULT (0),
        CreatedAtUtc      DATETIME2(3)     NOT NULL CONSTRAINT DF_RCA_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy         UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc     DATETIME2(3)     NULL,
        ModifiedBy        UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RCA_TenantId_SiteId] ON [ContinuousImprovement].[RCA] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_RCA_InitiatedAtUtc] ON [ContinuousImprovement].[RCA] (InitiatedAtUtc);
END
GO

-- ============================================================================
-- Phase 4 full build-out (2026-08-02): additive columns backing the newly built RCA aggregate's
-- full lifecycle (StartAnalysis/SubmitForApproval/Close/Archive) plus a native RowVersion for
-- optimistic-concurrency ETags. This table pre-existed as a read-only projection (no lifecycle
-- attribute columns, no RowVersion) -- every column below is added via an idempotent guard so
-- re-running this script against an already-migrated database is a no-op. The pre-existing
-- CreatedAtUtc/CreatedBy/IsDeleted/ModifiedAtUtc/ModifiedBy audit columns are reused as-is (no
-- rename) -- the aggregate's write-side repository maps its own CreatedBy/CreatedAtUtc onto these
-- same columns and reuses ModifiedAtUtc/ModifiedBy (not a new "UpdatedAtUtc/UpdatedBy" pair) for
-- symmetry with the table's own pre-existing naming.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'StartedBy')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD StartedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'StartedAtUtc')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD StartedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'SubmittedBy')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD SubmittedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'Findings')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD Findings NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'SubmittedAtUtc')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD SubmittedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'ClosedBy')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD ClosedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'RootCauseSummary')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD RootCauseSummary NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'ClosedAtUtc')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD ClosedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'ArchivedAtUtc')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD ArchivedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[ContinuousImprovement].[RCA]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [ContinuousImprovement].[RCA] ADD RowVersion ROWVERSION NOT NULL;
END
GO

-- NOTE: the longest RCAStatus enum member is 'PendingApproval' (15 chars); the pre-existing
-- Status NVARCHAR(50) column already comfortably accommodates every lifecycle status name, so no
-- widening ALTER is required here.
