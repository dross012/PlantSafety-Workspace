-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Analytics
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Analytics_GetRecords.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IAnalyticsRepository.GetRecordsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_Analytics_GetRecords]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RecordId,
        TenantId,
        SiteId,
        SourceSystem,
        MetricName,
        MetricValue,
        Status,
        IngestedAtUtc,
        IngestedBy
    FROM [Integrations].[AnalyticsItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND IngestedAtUtc >= @FromUtc
      AND IngestedAtUtc <= @ToUtc
    ORDER BY IngestedAtUtc DESC;
END
GO
