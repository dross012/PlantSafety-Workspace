-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportIncidents
--  File:           usp_TransportIncident_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single TransportIncident row by identifier, backing
--                  ITransportIncidentRepository.GetByIdAsync (Domain, id is globally unique so no
--                  tenant filter is required for correctness). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_GetById]
    @TransportIncidentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [TransportIncidentId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [DriverUserId],
        [IncidentType], [Severity], [OccurredAtUtc], [Location], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[TransportIncidents]
    WHERE [TransportIncidentId] = @TransportIncidentId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
