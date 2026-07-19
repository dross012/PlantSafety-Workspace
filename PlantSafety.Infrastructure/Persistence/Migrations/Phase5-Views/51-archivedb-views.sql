USE [PlantSafety.ArchiveDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 5 Views & Summaries
-- Script:        51-archivedb-views.sql
-- Author:        View & Summary Builder (unattended agent run)
-- Purpose:       Active-record views for PlantSafety.ArchiveDB's 2 tables.
--
-- Deliberately NOT created:
--   - No cross-table reporting view: sys.foreign_keys confirms ZERO foreign keys exist
--     between Archival.ArchivalJob and Archival.TierPolicy (verified live) -- ArchivalJob
--     carries a DatasetId that references a Data Lake dataset outside this database, not
--     a local FK, so there is no genuine in-database relationship to join.
--   - No tenant-scoped view: no established view-parameterization pattern exists (see
--     CoreDB rationale); TenantId scoping remains an app/SP-layer responsibility.
--   - No materialized summary table: ArchivalJob has Status/SourceTier/TargetTier and
--     could theoretically support a "jobs by status per day" rollup, but no aggregation
--     consumer (dashboard, report) is documented anywhere in memory for ArchiveDB, so
--     inventing one would violate the No-Guessing Rule. Recommended as an OPTIONAL
--     summary if/when an Archive operations dashboard is scoped: a
--     Archival.ArchivalJobSummary (SummaryDateKey, TenantId, SourceTier, TargetTier,
--     Status, JobCount) would follow the exact same full-rebuild pattern used for
--     CoreDB's SafetyEvents.IncidentManagementSummary.
-- =============================================

CREATE OR ALTER VIEW Archival.vw_ArchivalJob_Active
AS
SELECT
    e.ArchivalJobId, e.DatasetId, e.TenantId, e.PartitionKey, e.SourceTier, e.TargetTier,
    e.Status, e.StartedAtUtc, e.CompletedAtUtc, e.FailureReason, e.CorrelationId, e.CausationId,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Archival.ArchivalJob AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Archival.vw_TierPolicy_Active
AS
SELECT
    e.TierPolicyId, e.Zone, e.SourceTier, e.TargetTier, e.TransitionAfterDays,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Archival.TierPolicy AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO
