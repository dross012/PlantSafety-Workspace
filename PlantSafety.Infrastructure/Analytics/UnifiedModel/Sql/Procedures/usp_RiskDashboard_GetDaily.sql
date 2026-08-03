-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_RiskDashboard_GetDaily.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Aggregates FactRiskAssessment into daily Risk BI dashboard KPI rows.
--                  Additive-only evolution.
--  UPDATED:        2026-08-02 (SQL & Database Agent, Phase 5 hardening pass) -- added the
--                  canonical TRY/CATCH + centralized ErrorDB logging + best-effort AuditDB
--                  viewer-access logging, matching every other Phase 5 procedure and the
--                  canonical stored-procedure template in 00-canonical-templates.md. This
--                  read-only dashboard proc previously had none of these, which was a
--                  compliance and observability gap versus every sibling *Dashboard_GetDaily
--                  procedure. Query itself is unchanged (deterministic, no GETDATE()/RAND()).
--                  See IX_FactRiskAssessment_Tenant_CapturedAtUtc (added in
--                  030-dashboard-covering-indexes.sql) for the covering index backing the
--                  @FromUtc/@ToUtc + @SiteId filter this proc actually uses.
--  UPDATED:        2026-08-02 (Phase 6 query-plan-hint pass) -- added OPTION (RECOMPILE) per
--                  ..\query-plan-hint-convention.md: the optional @SiteId filter combined with
--                  the GROUP BY aggregate makes this proc's memory-grant sizing sensitive to a
--                  bad sniffed-parameter cardinality estimate; low/moderate BI-dashboard call
--                  frequency makes the per-call recompile cost acceptable. Does not change
--                  which index is used (SiteKey is INCLUDE-only, not a key column).
-- ============================================================================
USE [PlantSafety.CoreDB];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [AnalyticsUnifiedModel].[usp_RiskDashboard_GetDaily]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        IF @TenantId IS NULL
            THROW 50001, 'TenantId is required.', 1;
        IF @FromUtc IS NULL OR @ToUtc IS NULL
            THROW 50002, 'FromUtc and ToUtc are required.', 1;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'FactRiskAssessment', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(), 1, CONCAT(N'TenantId=', @TenantId, N'; FromUtc=', @FromUtc, N'; ToUtc=', @ToUtc), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;

        SELECT
            CAST(f.CapturedAtUtc AS DATE)                                    AS BusinessDate,
            f.TenantId                                                       AS TenantId,
            @SiteId                                                          AS SiteId,
            COUNT(*)                                                         AS TotalAssessments,
            SUM(CASE WHEN f.RiskLevelCode = 'Critical' THEN 1 ELSE 0 END)    AS CriticalCount,
            SUM(CASE WHEN f.RiskLevelCode = 'High' THEN 1 ELSE 0 END)        AS HighCount,
            SUM(CASE WHEN f.RiskLevelCode = 'Medium' THEN 1 ELSE 0 END)      AS MediumCount,
            SUM(CASE WHEN f.RiskLevelCode = 'Low' THEN 1 ELSE 0 END)         AS LowCount,
            AVG(CAST(f.CompositeScore AS FLOAT))                             AS AverageCompositeScore,
            SUM(CASE WHEN f.IsResidualScore = 1 THEN 1 ELSE 0 END)           AS ResidualAssessmentCount
        FROM [AnalyticsUnifiedModel].[FactRiskAssessment] f
        WHERE f.TenantId = @TenantId
          AND (@SiteId IS NULL OR f.SiteKey = @SiteId)
          AND f.CapturedAtUtc >= @FromUtc
          AND f.CapturedAtUtc <= @ToUtc
        GROUP BY CAST(f.CapturedAtUtc AS DATE), f.TenantId
        ORDER BY BusinessDate
        OPTION (RECOMPILE);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; SiteId=', @SiteId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
