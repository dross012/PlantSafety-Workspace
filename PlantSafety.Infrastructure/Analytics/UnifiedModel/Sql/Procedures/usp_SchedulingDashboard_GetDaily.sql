-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_SchedulingDashboard_GetDaily.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Aggregates FactSchedulingEvent into daily Scheduling BI dashboard KPI rows.
--                  Additive-only evolution.
--  UPDATED:        2026-08-02 (SQL & Database Agent, Phase 5 hardening pass) -- added the
--                  canonical TRY/CATCH + centralized ErrorDB logging + best-effort AuditDB
--                  viewer-access logging, matching every other Phase 5 procedure and the
--                  canonical stored-procedure template in 00-canonical-templates.md. Query
--                  itself is unchanged (deterministic). See
--                  IX_FactSchedulingEvent_Tenant_ScheduledAtUtc
--                  (030-dashboard-covering-indexes.sql) for the covering index backing the
--                  @FromUtc/@ToUtc + @SiteId filter.
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

CREATE OR ALTER PROCEDURE [AnalyticsUnifiedModel].[usp_SchedulingDashboard_GetDaily]
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
            VALUES (N'FactSchedulingEvent', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(), 1, CONCAT(N'TenantId=', @TenantId, N'; FromUtc=', @FromUtc, N'; ToUtc=', @ToUtc), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;

        SELECT
            CAST(f.ScheduledAtUtc AS DATE)                                         AS BusinessDate,
            f.TenantId                                                             AS TenantId,
            @SiteId                                                                AS SiteId,
            COUNT(*)                                                               AS TotalScheduledEvents,
            SUM(CASE WHEN f.WasOnTime = 1 THEN 1 ELSE 0 END)                       AS OnTimeCompletedCount,
            SUM(CASE WHEN f.CompletedAtUtc IS NOT NULL AND f.WasOnTime = 0 THEN 1 ELSE 0 END) AS LateCompletedCount,
            CAST(SUM(CASE WHEN f.WasOnTime = 1 THEN 1 ELSE 0 END) AS FLOAT)
                / NULLIF(SUM(CASE WHEN f.CompletedAtUtc IS NOT NULL THEN 1 ELSE 0 END), 0)     AS OnTimeRate,
            AVG(CAST(f.VarianceMinutes AS FLOAT))                                  AS AverageVarianceMinutes,
            SUM(CASE WHEN f.PriorityCode = 'P1' AND f.CompletedAtUtc IS NULL THEN 1 ELSE 0 END) AS P1OutstandingCount
        FROM [AnalyticsUnifiedModel].[FactSchedulingEvent] f
        WHERE f.TenantId = @TenantId
          AND (@SiteId IS NULL OR f.SiteKey = @SiteId)
          AND f.ScheduledAtUtc >= @FromUtc
          AND f.ScheduledAtUtc <= @ToUtc
        GROUP BY CAST(f.ScheduledAtUtc AS DATE), f.TenantId
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
