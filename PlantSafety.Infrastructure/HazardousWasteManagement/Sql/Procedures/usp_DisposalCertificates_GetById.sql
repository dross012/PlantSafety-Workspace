-- ============================================================================
--  Module: HazardousWasteManagement.DisposalCertificates | Layer: Infrastructure - SQL Scripts
--  File: usp_DisposalCertificates_GetById.sql | Created: 2026-08-04
--  Description: Returns a single DisposalCertificates row by identifier. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_GetById]
(
    @DisposalCertificateId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DisposalCertificateId], [TenantId], [SiteId], [Status], [ManifestId], [TsdfFacilityId], [DisposalMethod], [DisposalDateUtc], [CertificateReceivedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[DisposalCertificates]
    WHERE [DisposalCertificateId] = @DisposalCertificateId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
