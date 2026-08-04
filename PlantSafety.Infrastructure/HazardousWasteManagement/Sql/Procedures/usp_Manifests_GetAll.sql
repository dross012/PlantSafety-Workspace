-- ============================================================================
--  Module: HazardousWasteManagement.Manifests | Layer: Infrastructure - SQL Scripts
--  File: usp_Manifests_GetAll.sql | Created: 2026-08-04
--  Description: Returns all non-deleted Manifests rows. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [HazardousWasteManifestId], [TenantId], [SiteId], [Status], [ManifestNumber], [GeneratorEpaId], [WasteStreamId], [TransporterId], [TsdfFacilityId],
           [GeneratedAtUtc], [TransporterSignedAtUtc], [TsdfReceivedAtUtc], [ClosedAtUtc], [IsImmutable], [DiscrepancyReason], [RetentionExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[Manifests]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
