-- ============================================================================
--  Module: HazardousWasteManagement.TsdfFacilities | Layer: Infrastructure - SQL Scripts
--  File: usp_TsdfFacilities_GetAll.sql | Created: 2026-08-04
--  Description: Returns all non-deleted TsdfFacilities rows. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_TsdfFacilities_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [TsdfFacilityId], [TenantId], [SiteId], [Status], [FacilityName], [EpaFacilityId], [PermitExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[TsdfFacilities]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
