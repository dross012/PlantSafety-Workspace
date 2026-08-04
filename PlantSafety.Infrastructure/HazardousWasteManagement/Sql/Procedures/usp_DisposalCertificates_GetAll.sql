-- ============================================================================
--  Module: HazardousWasteManagement.DisposalCertificates | Layer: Infrastructure - SQL Scripts
--  File: usp_DisposalCertificates_GetAll.sql | Created: 2026-08-04
--  Description: Returns all non-deleted DisposalCertificates rows. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DisposalCertificateId], [TenantId], [SiteId], [Status], [ManifestId], [TsdfFacilityId], [DisposalMethod], [DisposalDateUtc], [CertificateReceivedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [HazardousWasteManagement].[DisposalCertificates]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
