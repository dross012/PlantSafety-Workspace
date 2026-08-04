-- ============================================================================
--  Module: HazardousWasteManagement.Manifests | Layer: Infrastructure - SQL Scripts
--  File: usp_Manifests_GetById.sql | Created: 2026-08-04
--  Description: Returns a single Manifests row by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_GetById]
(
    @HazardousWasteManifestId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [HazardousWasteManifestId], [TenantId], [SiteId], [Status], [ManifestNumber], [GeneratorEpaId], [WasteStreamId], [TransporterId], [TsdfFacilityId],
           [GeneratedAtUtc], [TransporterSignedAtUtc], [TsdfReceivedAtUtc], [ClosedAtUtc], [IsImmutable], [DiscrepancyReason], [RetentionExpiresAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[Manifests]
    WHERE [HazardousWasteManifestId] = @HazardousWasteManifestId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
