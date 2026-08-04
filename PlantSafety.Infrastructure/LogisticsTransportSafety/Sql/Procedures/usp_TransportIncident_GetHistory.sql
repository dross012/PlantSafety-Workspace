-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportIncidents
--  File:           usp_TransportIncident_GetHistory.sql
--  Created:        2026-08-04
--  Description:    Read-only procedure backing ITransportIncidentRepository (Application
--                  read-model).GetTransportIncidentHistoryAsync, returning TransportIncident
--                  records for a tenant (optionally scoped to a site) that occurred within
--                  [@FromUtc, @ToUtc]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_GetHistory]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @FromUtc DATETIME2(3),
    @ToUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [TransportIncidentId], [TenantId], [SiteId], [VehicleId], [DriverUserId], [IncidentType], [Severity],
        [Status] AS [IncidentStatus], [Location], [OccurredAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[TransportIncidents]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [OccurredAtUtc] >= @FromUtc
      AND [OccurredAtUtc] <= @ToUtc
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [OccurredAtUtc] DESC;
END
GO
