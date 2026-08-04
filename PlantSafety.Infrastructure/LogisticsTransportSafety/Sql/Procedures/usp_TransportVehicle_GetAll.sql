-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportVehicles
--  File:           usp_TransportVehicle_GetAll.sql
--  Created:        2026-08-04
--  Description:    Returns all non-deleted TransportVehicle rows, backing
--                  ITransportVehicleRepository.ListAsync (the Domain specification predicate is
--                  applied in-memory in the Infrastructure repository against this full result
--                  set, matching usp_CommunityConcern_GetAll precedent). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [TransportVehicleId], [TenantId], [SiteId], [Status], [VehicleTitle], [VehicleType], [VehicleIdentifier],
        [LastInspectionAtUtc], [NextInspectionDueAtUtc], [LastInspectionScore], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[TransportVehicles]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
