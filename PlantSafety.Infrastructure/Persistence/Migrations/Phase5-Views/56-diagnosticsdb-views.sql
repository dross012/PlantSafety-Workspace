USE [PlantSafety.DiagnosticsDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 5 Views & Summaries
-- Script:        56-diagnosticsdb-views.sql
-- Author:        View & Summary Builder (unattended agent run)
-- Purpose:       Deterministic active-record views, one cross-table reporting view, and
--                a refresh procedure for the pre-existing Diagnostics.DiagnosticDailySummary
--                materialized table (created in an earlier phase without a refresh proc).
--
-- SECTION 1 -- Active-record views (11)
--   vw_<Entity>_Active for every soft-deletable Diagnostics table except:
--     - DiagnosticAttachment (queried by parent DiagnosticEventId, not "active" filter --
--       same reasoning as CoreDB's HistoryEntry tables)
--     - DiagnosticEventArchive (archive/cold-tier copy of DiagnosticEvent, queried by
--       date range / archival job, not an "active" filter)
--     - DiagnosticDailySummary (the summary table itself; already materialized, refreshed
--       below, no "active" filter concept applies to a pre-aggregated rollup)
--
-- SECTION 2 -- Cross-table reporting view (1)
--   Diagnostics.vw_DiagnosticEventFactWithDimension joins the existing star-schema
--   Fact/Dimension pair (FK_Diagnostics_DiagnosticEventFact_Dimension) -- this is the
--   genuine, already-modeled fact/dimension relationship, not an invented one.
--
-- SECTION 3 -- Summary refresh procedure (1)
--   Diagnostics.sp_DiagnosticDailySummary_Refresh -- full rebuild from
--   Diagnostics.DiagnosticEvent, grouped by OccurredAtUtc date key / TenantId /
--   Severity / Category, matching the existing table's exact column shape
--   (SummaryDateKey, TenantId, Severity, Category, EventCount).
--
-- Deliberately NOT created:
--   - No active view for DiagnosticAttachment/DiagnosticEventArchive (see above).
--   - No new summary tables: DiagnosticDailySummary + DiagnosticEventFact/Dimension
--     already cover the module's aggregation needs; a second summary table over the
--     same DiagnosticEvent source would duplicate DiagnosticDailySummary's purpose.
--
-- Idempotent:    CREATE OR ALTER views/procedure.
-- =============================================

-- ===================== SECTION 1: Active-record views =====================

CREATE OR ALTER VIEW Diagnostics.vw_DiagnosticEvent_Active
AS
SELECT
    e.DiagnosticEventId, e.TenantId, e.CorrelationId, e.Severity, e.Category, e.Module, e.Operation, e.OperationStatus, e.Message, e.Detail, e.MetadataJson, e.EventHash, e.OccurredAtUtc, e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Diagnostics.DiagnosticEvent AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_DiagnosticEventDimension_Active
AS
SELECT
    e.DiagnosticEventDimensionId, e.TenantId, e.Severity, e.Category, e.Module, e.Operation, e.OperationStatus, e.EffectiveFromUtc, e.EffectiveToUtc, e.IsCurrent, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Diagnostics.DiagnosticEventDimension AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_DiagnosticEventFact_Active
AS
SELECT
    e.DiagnosticEventFactId, e.DiagnosticEventId, e.TenantId, e.DiagnosticEventDimensionId, e.DateKey, e.Severity, e.DurationMs, e.EventCount, e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Diagnostics.DiagnosticEventFact AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_DiagnosticMetadata_Active
AS
SELECT
    e.DiagnosticMetadataId, e.DiagnosticEventId, e.TenantId, e.MetadataKey, e.MetadataValue, e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Diagnostics.DiagnosticMetadata AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_DiagnosticSnapshot_Active
AS
SELECT
    e.DiagnosticSnapshotId, e.TenantId, e.SnapshotName, e.StateJson, e.CapturedAtUtc, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Diagnostics.DiagnosticSnapshot AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_EngineRun_Active
AS
SELECT
    e.EngineRunId, e.StartedAtUtc, e.CompletedAtUtc, e.IsSuccess, e.TotalRulesEvaluated, e.PassCount, e.FailCount, e.SkipCount, e.ErrorCount, e.EngineVersion, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Diagnostics.EngineRun AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_ProviderRegistry_Active
AS
SELECT
    e.ProviderRegistryId, e.ProviderName, e.ProviderType, e.ConnectionInfoJson, e.IsEnabled, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Diagnostics.ProviderRegistry AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_RuleRegistry_Active
AS
SELECT
    e.RuleRegistryId, e.RuleCode, e.Title, e.Description, e.DefaultSeverity, e.Category, e.IsEnabled, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Diagnostics.RuleRegistry AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_TenantDiagnosticEvent_Active
AS
SELECT
    e.TenantDiagnosticEventId, e.TenantId, e.CorrelationId, e.Severity, e.Category, e.Module, e.Operation, e.OperationStatus, e.Message, e.Detail, e.MetadataJson, e.OccurredAtUtc, e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Diagnostics.TenantDiagnosticEvent AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_TenantEngineRun_Active
AS
SELECT
    e.TenantEngineRunId, e.TenantId, e.StartedAtUtc, e.CompletedAtUtc, e.IsSuccess, e.TotalRulesEvaluated, e.PassCount, e.FailCount, e.SkipCount, e.ErrorCount, e.EngineVersion, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Diagnostics.TenantEngineRun AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Diagnostics.vw_TenantRuleOverride_Active
AS
SELECT
    e.TenantRuleOverrideId, e.TenantId, e.RuleRegistryId, e.OverrideSeverity, e.IsSuppressed, e.Notes, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Diagnostics.TenantRuleOverride AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- ===================== SECTION 2: Cross-table reporting view =====================

-- =============================================
-- View:          Diagnostics.vw_DiagnosticEventFactWithDimension
-- Purpose:       Active DiagnosticEventFact rows joined to their DiagnosticEventDimension
--                row via the real FK_Diagnostics_DiagnosticEventFact_Dimension
--                relationship -- surfaces the descriptive attributes (Module, Operation,
--                OperationStatus, effective-dating) alongside the fact-table measures
--                (DurationMs, EventCount) for star-schema reporting.
-- =============================================
CREATE OR ALTER VIEW Diagnostics.vw_DiagnosticEventFactWithDimension
AS
SELECT
    f.DiagnosticEventFactId, f.DiagnosticEventId, f.TenantId, f.DateKey,
    f.Severity AS FactSeverity, f.DurationMs, f.EventCount,
    d.DiagnosticEventDimensionId, d.Severity AS DimensionSeverity, d.Category,
    d.Module, d.Operation, d.OperationStatus,
    d.EffectiveFromUtc, d.EffectiveToUtc, d.IsCurrent,
    f.CreatedUtc, f.CreatedBy
FROM Diagnostics.DiagnosticEventFact AS f
INNER JOIN Diagnostics.DiagnosticEventDimension AS d
    ON f.DiagnosticEventDimensionId = d.DiagnosticEventDimensionId
WHERE ISNULL(f.IsDeleted, 0) = 0
  AND ISNULL(d.IsDeleted, 0) = 0;
GO

-- ===================== SECTION 3: Summary refresh procedure =====================

-- =============================================
-- Procedure:     Diagnostics.sp_DiagnosticDailySummary_Refresh
-- Description:   Full-rebuild refresh of the pre-existing Diagnostics.DiagnosticDailySummary
--                table from Diagnostics.DiagnosticEvent, grouped by occurred-date key,
--                TenantId, Severity, Category -- matching the table's exact declared shape.
-- =============================================
CREATE OR ALTER PROCEDURE Diagnostics.sp_DiagnosticDailySummary_Refresh
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM Diagnostics.DiagnosticDailySummary;

        INSERT INTO Diagnostics.DiagnosticDailySummary
            (SummaryDateKey, TenantId, Severity, Category, EventCount, CreatedUtc, CreatedBy)
        SELECT
            CONVERT(int, CONVERT(char(8), e.OccurredAtUtc, 112)) AS SummaryDateKey,
            e.TenantId,
            e.Severity,
            e.Category,
            COUNT(*) AS EventCount,
            SYSUTCDATETIME(),
            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)
        FROM Diagnostics.DiagnosticEvent AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        GROUP BY
            CONVERT(int, CONVERT(char(8), e.OccurredAtUtc, 112)),
            e.TenantId, e.Severity, e.Category;

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'DiagnosticDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
                0, N'Full rebuild via sp_DiagnosticDailySummary_Refresh',
                0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block the refresh
        END CATCH;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorContext nvarchar(200) = CONCAT(N'Proc=Diagnostics.sp_DiagnosticDailySummary_Refresh; User=', SYSTEM_USER);
        DECLARE @ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID)
        );

        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
            TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
            BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
        )
        VALUES (
            N'DiagnosticDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
            NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
        );

        THROW;
    END CATCH
END
GO
