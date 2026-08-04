-- ============================================================================
--  Module:         LogisticsTransportSafety.Carriers
--  File:           usp_Carrier_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single Carrier row by identifier, backing
--                  ICarrierRepository.GetByIdAsync (Domain, id is globally unique so no tenant
--                  filter is required for correctness). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_GetById]
    @CarrierId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [CarrierId], [TenantId], [SiteId], [Status], [CarrierName], [CarrierType], [InsuranceExpiryAtUtc],
        [SafetyRatingScore], [QualifiedAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[Carriers]
    WHERE [CarrierId] = @CarrierId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
