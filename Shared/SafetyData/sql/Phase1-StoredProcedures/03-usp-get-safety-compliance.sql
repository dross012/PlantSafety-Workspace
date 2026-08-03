-- =============================================
-- Shared.SafetyData
-- Script:        03-usp-get-safety-compliance.sql
-- Purpose:       Creates dbo.usp_GetSafetyCompliance against [PlantSafety.CoreDB],
--                a DERIVED open/closed/overdue compliance rollup over
--                [RiskMgmt].[Risk] and [ContinuousImprovement].[CAPA] for
--                Shared.SafetyData.Infrastructure's SafetyDataDapperRepository.
--
-- WHY DERIVED (not a 1:1 table mapping): unlike Hazard Summary
-- ([RiskMgmt].[Risk]) and Corrective Action ([ContinuousImprovement].[CAPA]),
-- there is no PlantSafety aggregate/table named "SafetyCompliance" or
-- equivalent. Per explicit task instruction, this procedure derives a
-- reasonable compliance view (aggregate counts of open/closed/overdue items)
-- from the two real, already-verified tables the sibling procedures already
-- read from. See Shared.SafetyData.Abstractions\Models\SafetyComplianceEntry.cs
-- for the full field-by-field rationale. This is NOT a relocated PlantSafety
-- script -- it is new, module-owned aggregation logic.
--
-- ONE ROW PER CATEGORY:
--   'Hazards'           <- [RiskMgmt].[Risk]
--   'CorrectiveActions' <- [ContinuousImprovement].[CAPA]
--
-- OPEN / CLOSED CLASSIFICATION:
--   - Risk.Status: RiskStatus.Closed (code 'Closed'; see PlantSafety.Domain.
--     RiskManagement.Enums.RiskStatus) is the only terminal state recognized
--     by the Risk aggregate's own lifecycle enum -- 'Archived' (also terminal
--     in the enum) is deliberately still counted "open" here for a compliance
--     rollup, since an archived-but-never-formally-closed hazard should not
--     silently read as compliant; flagged for human review if that judgment
--     is wrong.
--   - CAPA.Status: CAPA.Status is a free-text NVARCHAR(50) column with no
--     fixed enum found anywhere in the scanned PlantSafety C#/SQL (confirmed
--     by repo-wide search -- CorrectiveActionEntry.Status/CAPA.Status values
--     are set by callers, not validated against a Smart Enum). The
--     case-insensitive values 'Closed', 'Completed', and 'Cancelled' are
--     treated as terminal here -- the common English vocabulary for a
--     finished/terminated corrective action -- flagged as a judgment call for
--     human review once PlantSafety's real CAPA status vocabulary is
--     confirmed.
--
-- OVERDUE CLASSIFICATION:
--   - Risk has no due-date column (NextReviewDueUtc exists but represents a
--     periodic *review* due date, not the hazard's own remediation due date,
--     and is nullable/optional) -- OverdueCount is therefore always 0 for the
--     'Hazards' row. Flagged: a future pass could instead treat
--     NextReviewDueUtc < SYSUTCDATETIME() AND ReviewRequired = 1 as "overdue
--     review", but that is a different compliance dimension than "overdue
--     remediation" and was judged out of scope for this rollup to avoid
--     overloading OverdueCount's meaning across the two category rows.
--   - CAPA.DueDateUtc < SYSUTCDATETIME() AND Status NOT IN the terminal list
--     above => Overdue, for the 'CorrectiveActions' row.
--
-- SAFETY: SELECT only (aggregate SELECT/COUNT queries). This procedure must
-- never gain an INSERT/UPDATE/DELETE statement.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetSafetyCompliance
    @TenantId  uniqueidentifier,
    @SiteId    uniqueidentifier = NULL,
    @FromUtc   datetime2        = NULL,
    @ToUtc     datetime2        = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TenantId IS NULL
        THROW 50001, N'TenantId is required.', 1;

    DECLARE @AsOfUtc datetime2 = SYSUTCDATETIME();

    ------------------------------------------------------------
    -- 'Hazards' rollup, from [RiskMgmt].[Risk]
    ------------------------------------------------------------
    ;WITH HazardScope AS
    (
        SELECT Status
        FROM [RiskMgmt].[Risk]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@FromUtc IS NULL OR CreatedAtUtc >= @FromUtc)
          AND (@ToUtc   IS NULL OR CreatedAtUtc <= @ToUtc)
    ),
    HazardRollup AS
    (
        SELECT
            N'Hazards'                                                  AS Category,
            COUNT(*)                                                    AS TotalCount,
            SUM(CASE WHEN Status <> N'Closed' THEN 1 ELSE 0 END)        AS OpenCount,
            SUM(CASE WHEN Status = N'Closed' THEN 1 ELSE 0 END)         AS ClosedCount,
            0                                                           AS OverdueCount
        FROM HazardScope
    )
    SELECT
        Category,
        TotalCount,
        OpenCount,
        ClosedCount,
        OverdueCount,
        CAST(CASE WHEN TotalCount = 0 THEN 100.00
                  ELSE ROUND(100.0 * ClosedCount / TotalCount, 2) END AS decimal(5,2)) AS CompliancePercentage,
        @AsOfUtc AS AsOfUtc
    FROM HazardRollup

    UNION ALL

    ------------------------------------------------------------
    -- 'CorrectiveActions' rollup, from [ContinuousImprovement].[CAPA]
    ------------------------------------------------------------
    SELECT
        Category,
        TotalCount,
        OpenCount,
        ClosedCount,
        OverdueCount,
        CAST(CASE WHEN TotalCount = 0 THEN 100.00
                  ELSE ROUND(100.0 * ClosedCount / TotalCount, 2) END AS decimal(5,2)) AS CompliancePercentage,
        @AsOfUtc AS AsOfUtc
    FROM
    (
        SELECT
            N'CorrectiveActions' AS Category,
            COUNT(*) AS TotalCount,
            SUM(CASE WHEN Status NOT IN (N'Closed', N'Completed', N'Cancelled') THEN 1 ELSE 0 END) AS OpenCount,
            SUM(CASE WHEN Status IN (N'Closed', N'Completed', N'Cancelled') THEN 1 ELSE 0 END) AS ClosedCount,
            SUM(CASE WHEN Status NOT IN (N'Closed', N'Completed', N'Cancelled')
                      AND DueDateUtc < @AsOfUtc THEN 1 ELSE 0 END) AS OverdueCount
        FROM [ContinuousImprovement].[CAPA]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@FromUtc IS NULL OR CreatedAtUtc >= @FromUtc)
          AND (@ToUtc   IS NULL OR CreatedAtUtc <= @ToUtc)
    ) AS CorrectiveActionRollup;
END
GO
