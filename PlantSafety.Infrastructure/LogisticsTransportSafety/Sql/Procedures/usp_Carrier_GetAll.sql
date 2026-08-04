-- ============================================================================
--  Module:         LogisticsTransportSafety.Carriers
--  File:           usp_Carrier_GetAll.sql
--  Created:        2026-08-04
--  Description:    Returns all non-deleted Carrier rows, backing ICarrierRepository.ListAsync
--                  (the Domain specification predicate is applied in-memory in the Infrastructure
--                  repository against this full result set). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_Carrier_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [CarrierId], [TenantId], [SiteId], [Status], [CarrierName], [CarrierType], [InsuranceExpiryAtUtc],
        [SafetyRatingScore], [QualifiedAtUtc], [SuspensionReason], [CreatedAtUtc], [CreatedBy]
    FROM [LogisticsTransportSafety].[Carriers]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
