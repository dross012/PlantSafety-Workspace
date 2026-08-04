-- ============================================================================
--  Module: HazardousWasteManagement.Transporters | Layer: Infrastructure - SQL Scripts
--  File: usp_Transporters_GetAll.sql | Created: 2026-08-04
--  Description: Returns all non-deleted Transporters rows. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Transporters_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [TransporterId], [TenantId], [SiteId], [Status], [CompanyName], [EpaTransporterId], [LicenseExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[Transporters]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
