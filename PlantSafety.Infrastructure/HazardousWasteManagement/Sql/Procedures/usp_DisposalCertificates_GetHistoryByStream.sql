-- ============================================================================
--  Module: HazardousWasteManagement.DisposalCertificates | Layer: Infrastructure - SQL Scripts
--  File: usp_DisposalCertificates_GetHistoryByStream.sql | Created: 2026-08-04
--  Description: Read-only procedure backing IDisposalCertificateRepository.
--               GetDisposalHistoryByStreamAsync. Joins Manifests to resolve WasteStreamId
--               (DisposalCertificates references ManifestId, not WasteStreamId, directly).
--               Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_GetHistoryByStream]
    @TenantId UNIQUEIDENTIFIER,
    @WasteStreamId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        dc.[DisposalCertificateId], dc.[TenantId], dc.[SiteId], m.[WasteStreamId], dc.[ManifestId], dc.[TsdfFacilityId],
        dc.[DisposalMethod], dc.[DisposalDateUtc], dc.[CertificateReceivedAtUtc], dc.[Status]
    FROM [HazardousWasteManagement].[DisposalCertificates] dc
    INNER JOIN [HazardousWasteManagement].[Manifests] m ON m.[HazardousWasteManifestId] = dc.[ManifestId]
    WHERE dc.TenantId = @TenantId
      AND m.[WasteStreamId] = @WasteStreamId
      AND ISNULL(dc.[IsDeleted], 0) = 0
    ORDER BY dc.[CertificateReceivedAtUtc] DESC;
END
GO
