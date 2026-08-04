-- ============================================================================
--  Module: HazardousWasteManagement.TsdfFacilities | Layer: Infrastructure - SQL Scripts
--  File: usp_TsdfFacilities_GetById.sql | Created: 2026-08-04
--  Description: Returns a single TsdfFacilities row by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_TsdfFacilities_GetById]
(
    @TsdfFacilityId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [TsdfFacilityId], [TenantId], [SiteId], [Status], [FacilityName], [EpaFacilityId], [PermitExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[TsdfFacilities]
    WHERE [TsdfFacilityId] = @TsdfFacilityId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
