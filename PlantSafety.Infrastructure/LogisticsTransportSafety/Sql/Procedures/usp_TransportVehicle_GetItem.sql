-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportVehicles
--  File:           usp_TransportVehicle_GetItem.sql
--  Created:        2026-08-04
--  Description:    Read-only procedure backing
--                  ITransportVehicleRepository (Application read-model).GetTransportVehicleByIdAsync,
--                  tenant-scoped for defense in depth. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportVehicle_GetItem]
    @TenantId UNIQUEIDENTIFIER,
    @TransportVehicleId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [TransportVehicleId], [TenantId], [SiteId], [VehicleTitle], [VehicleType], [VehicleIdentifier],
        [Status] AS [VehicleStatus], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[TransportVehicles]
    WHERE [TenantId] = @TenantId
      AND [TransportVehicleId] = @TransportVehicleId
      AND ISNULL([IsDeleted], 0) = 0;
END
GO
