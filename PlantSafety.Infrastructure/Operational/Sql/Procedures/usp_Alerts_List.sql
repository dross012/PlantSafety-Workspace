-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Alerts
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Alerts_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[AlertsReadModel],
--                  backing IAlertsRepository.GetAlertsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Alerts_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [AlertId],
        [TenantId],
        [SiteId],
        [Message],
        [Severity],
        [Status],
        [TriggeredAtUtc],
        [AcknowledgedBy]
FROM [Operational].[AlertsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [TriggeredAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [TriggeredAtUtc] DESC;
GO
