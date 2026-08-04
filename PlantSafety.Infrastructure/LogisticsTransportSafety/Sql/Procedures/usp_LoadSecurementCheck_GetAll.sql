-- ============================================================================
--  Module:         LogisticsTransportSafety.LoadSecurementChecks
--  File:           usp_LoadSecurementCheck_GetAll.sql
--  Created:        2026-08-04
--  Description:    Returns all non-deleted LoadSecurementCheck rows, backing
--                  ILoadSecurementCheckRepository.ListAsync (the Domain specification predicate is
--                  applied in-memory in the Infrastructure repository against this full result
--                  set). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_LoadSecurementCheck_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [LoadSecurementCheckId], [TenantId], [SiteId], [Status], [TripId], [VehicleId], [CheckedByUserId],
        [CheckedAtUtc], [DeficiencyCount], [SecurementScore], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[LoadSecurementChecks]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
