-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.RootCauseAnalysis
--  Layer:          Infrastructure – SQL Scripts
--  File:           RootCauseAnalysis.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RootCauseAnalysis read-model, backing
--                  IRootCauseAnalysisRepository.GetRootCauseAnalysesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RootCauseAnalysis' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[RootCauseAnalysis] (
        AnalysisId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        RelatedEventId  UNIQUEIDENTIFIER NULL,
        Methodology     NVARCHAR(50)     NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        CompletedAtUtc  DATETIME2(3)     NOT NULL,
        LeadAnalyst     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RootCauseAnalysis_TenantId_SiteId] ON [SafetyEvents].[RootCauseAnalysis] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_RootCauseAnalysis_CompletedAtUtc] ON [SafetyEvents].[RootCauseAnalysis] (CompletedAtUtc);
END
GO

-- ============================================================================
-- Phase 4 full build-out (2026-08-02): additive columns backing the newly built
-- RootCauseAnalysis aggregate's full lifecycle (SubmitForReview/Approve/Close/Archive) plus the
-- standard audit-column set. This table pre-existed as a read-only projection (no CreatedAtUtc/
-- CreatedBy/IsDeleted, and CompletedAtUtc was NOT NULL) -- every column below is added via an
-- idempotent guard so re-running this script against an already-migrated database is a no-op.
-- CompletedAtUtc is relaxed to nullable since it is now only populated once Approve()
-- transitions the aggregate to Completed (an InProgress/PendingReview analysis has no completion
-- timestamp yet). The primary key column remains AnalysisId (matches the pre-existing read model
-- / RootCauseAnalysisItemDto.AnalysisId); the aggregate's own RootCauseAnalysisId property maps
-- onto this same column.
-- ============================================================================

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'CompletedAtUtc' AND is_nullable = 0
)
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ALTER COLUMN CompletedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'SubmittedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD SubmittedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'SubmittedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD SubmittedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'ApprovedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD ApprovedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'ClosedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD ClosedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'ClosedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD ClosedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'ArchivedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD ArchivedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'CreatedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_RootCauseAnalysis_CreatedAtUtc DEFAULT (SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD CreatedBy UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RootCauseAnalysis_CreatedBy DEFAULT ('00000000-0000-0000-0000-000000000000');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'UpdatedAtUtc')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD UpdatedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'UpdatedBy')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD UpdatedBy UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD IsDeleted BIT NOT NULL CONSTRAINT DF_RootCauseAnalysis_IsDeleted DEFAULT (0);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[SafetyEvents].[RootCauseAnalysis]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [SafetyEvents].[RootCauseAnalysis] ADD RowVersion ROWVERSION NOT NULL;
END
GO

-- NOTE: the longest RootCauseAnalysisStatus enum member is 'PendingReview' (13 chars); the
-- pre-existing Status NVARCHAR(50) column already comfortably accommodates every lifecycle status
-- name, so no widening ALTER is required here.
