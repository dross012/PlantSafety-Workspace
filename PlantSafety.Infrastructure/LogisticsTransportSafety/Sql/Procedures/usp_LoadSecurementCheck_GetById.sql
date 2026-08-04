-- ============================================================================
--  Module:         LogisticsTransportSafety.LoadSecurementChecks
--  File:           usp_LoadSecurementCheck_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single LoadSecurementCheck row by identifier, backing
--                  ILoadSecurementCheckRepository.GetByIdAsync (Domain, id is globally unique so
--                  no tenant filter is required for correctness). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_GetById]
    @LoadSecurementCheckId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [LoadSecurementCheckId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [CheckedByUserId],
        [CheckedAtUtc], [DeficiencyCount], [SecurementScore], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[LoadSecurementChecks]
    WHERE [LoadSecurementCheckId] = @LoadSecurementCheckId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
