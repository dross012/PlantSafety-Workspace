-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportIncidents
--  File:           usp_TransportIncident_GetAll.sql
--  Created:        2026-08-04
--  Description:    Returns all non-deleted TransportIncident rows, backing
--                  ITransportIncidentRepository.ListAsync (the Domain specification predicate is
--                  applied in-memory in the Infrastructure repository against this full result
--                  set). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [TransportIncidentId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [DriverUserId],
        [IncidentType], [Severity], [OccurredAtUtc], [Location], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[TransportIncidents]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
