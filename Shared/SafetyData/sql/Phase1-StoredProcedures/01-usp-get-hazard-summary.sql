-- =============================================
-- Shared.SafetyData
-- Script:        01-usp-get-hazard-summary.sql
-- Purpose:       Creates dbo.usp_GetHazardSummary against [PlantSafety.CoreDB], the
--                single canonical, parameterized, read-only query path over
--                [RiskMgmt].[Risk] (joined to [RiskMgmt].[ControlMeasure]) for
--                Shared.SafetyData.Infrastructure's SafetyDataDapperRepository.
--
-- TABLE PROVENANCE: both source tables have real, tracked CREATE TABLE scripts,
-- unlike Shared.AuditLogging's dbo.AuditLog:
--   - PlantSafety.Infrastructure\RiskManagement\Sql\Tables\Risk.sql
--     ([RiskMgmt].[Risk], tenant-scoped, in PlantSafety.CoreDB)
--   - PlantSafety.Infrastructure\RiskManagement\Sql\Tables\ControlMeasure.sql
--     ([RiskMgmt].[ControlMeasure], FK to Risk.RiskId, tenant-scoped)
-- Column names below are copied verbatim from those scripts and from
-- PlantSafety.Infrastructure's own read stored procedure
-- RiskManagement\Sql\Procedures\usp_Risk_GetAllActive.sql (confirms the exact
-- SELECT list PlantSafety itself already uses for this table) and
-- \usp_ControlMeasure_GetByRiskId.sql. No column name is invented here.
--
-- LOCATION JUDGMENT CALL: this script lives under Shared\SafetyData\sql rather
-- than as a new PlantSafety.Infrastructure RiskManagement\Sql\Procedures script,
-- and the procedure is created in dbo (not [RiskMgmt]) -- mirroring
-- Shared.AuditLogging's dbo.usp_SearchAuditLog placement judgment call. This
-- procedure is Shared.SafetyData's own read path, distinct from (and read-only
-- alongside) PlantSafety's own [RiskMgmt].[usp_Risk_GetAllActive] /
-- [usp_Risk_GetById], which remain PlantSafety's write-side aggregate
-- reconstruction path and are untouched by this module.
--
-- RESULT SET SHAPE: THREE result sets, not two (Shared.AuditLogging's
-- usp_SearchAuditLog returns two -- count, then rows). The third result set is
-- added here because HazardSummaryEntry.ControlMeasures is a
-- one-to-many child collection per hazard; returning it as a third,
-- ControlMeasure-shaped result set (grouped client-side by RiskId in
-- SafetyDataDapperRepository) avoids duplicating every Risk row once per
-- control measure, which a single joined result set would require.
--   (1) single-row, single-column total count of matching Risk rows
--   (2) the requested page of Risk rows, newest first (by CreatedAtUtc)
--   (3) every ControlMeasure row (IsDeleted = 0) belonging to a RiskId present
--       in result set (2) -- unpaginated, since a hazard's control-measure list
--       is expected to be small (Hierarchy-of-Controls typically has <= 5
--       levels applied per hazard)
--
-- SAFETY: SELECT only. This procedure must never gain an INSERT/UPDATE/DELETE
-- statement -- RiskMgmt.Risk / RiskMgmt.ControlMeasure are owned and mutated
-- exclusively by PlantSafety's Risk aggregate via RiskDapperRepository.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetHazardSummary
    @TenantId    uniqueidentifier,
    @SiteId      uniqueidentifier = NULL,
    @FromUtc     datetime2        = NULL,
    @ToUtc       datetime2        = NULL,
    @PageNumber  int              = 1,
    @PageSize    int              = 50
AS
BEGIN
    SET NOCOUNT ON;

    IF @TenantId IS NULL
        THROW 50001, N'TenantId is required.', 1;
    IF @PageNumber IS NULL OR @PageNumber < 1
        THROW 50001, N'PageNumber must be a positive integer.', 1;
    IF @PageSize IS NULL OR @PageSize < 1
        THROW 50001, N'PageSize must be a positive integer.', 1;

    ------------------------------------------------------------
    -- Result set 1: total matching row count
    ------------------------------------------------------------
    SELECT COUNT(*)
    FROM [RiskMgmt].[Risk]
    WHERE TenantId = @TenantId
      AND IsDeleted = 0
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND (@FromUtc IS NULL OR CreatedAtUtc >= @FromUtc)
      AND (@ToUtc   IS NULL OR CreatedAtUtc <= @ToUtc);

    ------------------------------------------------------------
    -- Result set 2: the requested page, newest first
    ------------------------------------------------------------
    ;WITH PagedRisk AS
    (
        SELECT
            RiskId, TenantId, SiteId, HazardSource, Category, RiskTitle, CreatedAtUtc
        FROM [RiskMgmt].[Risk]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@FromUtc IS NULL OR CreatedAtUtc >= @FromUtc)
          AND (@ToUtc   IS NULL OR CreatedAtUtc <= @ToUtc)
        ORDER BY CreatedAtUtc DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY
    )
    SELECT RiskId, CreatedAtUtc, HazardSource, Category, RiskTitle
    FROM PagedRisk;

    ------------------------------------------------------------
    -- Result set 3: control measures for every RiskId in result set 2
    ------------------------------------------------------------
    SELECT cm.RiskId, cm.Description
    FROM [RiskMgmt].[ControlMeasure] AS cm
    WHERE cm.IsDeleted = 0
      AND cm.RiskId IN
      (
          SELECT RiskId
          FROM [RiskMgmt].[Risk]
          WHERE TenantId = @TenantId
            AND IsDeleted = 0
            AND (@SiteId IS NULL OR SiteId = @SiteId)
            AND (@FromUtc IS NULL OR CreatedAtUtc >= @FromUtc)
            AND (@ToUtc   IS NULL OR CreatedAtUtc <= @ToUtc)
          ORDER BY CreatedAtUtc DESC
          OFFSET (@PageNumber - 1) * @PageSize ROWS
          FETCH NEXT @PageSize ROWS ONLY
      )
    ORDER BY cm.RiskId, cm.ImplementedAtUtc;
END
GO
