-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Auditing
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Auditing_GetLogs.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IAuditingRepository.GetLogsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_Auditing_GetLogs]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        LogId,
        TenantId,
        SiteId,
        ExternalSystem,
        Direction,
        Endpoint,
        Status,
        OccurredAtUtc,
        InitiatedBy
    FROM [Integrations].[AuditingItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND OccurredAtUtc >= @FromUtc
      AND OccurredAtUtc <= @ToUtc
    ORDER BY OccurredAtUtc DESC;
END
GO
