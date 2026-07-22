-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Export
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Export_GetExportRuns.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IExportRepository.GetExportRunsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_Export_GetExportRuns]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ExportRunId,
        TenantId,
        SiteId,
        TargetSystem,
        ExportType,
        RecordCount,
        Status,
        CompletedAtUtc,
        InitiatedBy
    FROM [Integrations].[ExportItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND CompletedAtUtc >= @FromUtc
      AND CompletedAtUtc <= @ToUtc
    ORDER BY CompletedAtUtc DESC;
END
GO
