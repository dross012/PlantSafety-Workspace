-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportVehicles
--  File:           usp_TransportVehicle_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single TransportVehicle row by identifier, backing
--                  ITransportVehicleRepository.GetByIdAsync (Domain, id is globally unique so no
--                  tenant filter is required for correctness -- matches
--                  usp_CommunityConcern_GetById precedent). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_GetById]
    @TransportVehicleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [TransportVehicleId], [TenantId], [SiteId], [Status], [VehicleTitle], [VehicleType], [VehicleIdentifier],
        [LastInspectionAtUtc], [NextInspectionDueAtUtc], [LastInspectionScore], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[TransportVehicles]
    WHERE [TransportVehicleId] = @TransportVehicleId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
