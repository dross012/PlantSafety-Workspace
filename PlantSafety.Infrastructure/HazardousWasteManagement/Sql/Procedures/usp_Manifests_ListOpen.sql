-- ============================================================================
--  Module: HazardousWasteManagement.Manifests | Layer: Infrastructure - SQL Scripts
--  File: usp_Manifests_ListOpen.sql | Created: 2026-08-04
--  Description: Read-only procedure backing IHazardousWasteManifestRepository.
--               ListOpenManifestsAsync (all manifests not yet Closed). Feeds the Hazardous
--               Waste Lifecycle Tracker dashboard and the manifest-follow-up scheduling hook.
--               Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_ListOpen]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT [HazardousWasteManifestId], [TenantId], [SiteId], [Status], [ManifestNumber], [GeneratorEpaId], [WasteStreamId], [TransporterId], [TsdfFacilityId],
           [GeneratedAtUtc], [TransporterSignedAtUtc], [TsdfReceivedAtUtc], [ClosedAtUtc], [IsImmutable], [DiscrepancyReason], [RetentionExpiresAtUtc]
    FROM [HazardousWasteManagement].[Manifests]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND [Status] <> 'Closed'
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [GeneratedAtUtc] ASC;
END
GO
