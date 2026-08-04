-- ============================================================================
--  Module: HazardousWasteManagement.Transporters | Layer: Infrastructure - SQL Scripts
--  File: usp_Transporters_GetById.sql | Created: 2026-08-04
--  Description: Returns a single Transporters row by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Transporters_GetById]
(
    @TransporterId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [TransporterId], [TenantId], [SiteId], [Status], [CompanyName], [EpaTransporterId], [LicenseExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[Transporters]
    WHERE [TransporterId] = @TransporterId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
