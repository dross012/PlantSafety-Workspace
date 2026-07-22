-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Analyzers
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Analyzers_GetRuns.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IAnalyzersRepository.GetRunsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_Analyzers_GetRuns]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RunId,
        TenantId,
        SiteId,
        AnalyzerName,
        TargetSystem,
        FindingsCount,
        Status,
        RunAtUtc,
        TriggeredBy
    FROM [Integrations].[AnalyzersItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND RunAtUtc >= @FromUtc
      AND RunAtUtc <= @ToUtc
    ORDER BY RunAtUtc DESC;
END
GO
