-- ============================================================================
--  Module: HazardousWasteManagement.DisposalCertificates | Layer: Infrastructure - SQL Scripts
--  File: usp_DisposalCertificates_Create.sql | Created: 2026-08-04
--  Description: Insert procedure for [HazardousWasteManagement].[DisposalCertificates]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @DisposalCertificateId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ManifestId UNIQUEIDENTIFIER,
    @TsdfFacilityId UNIQUEIDENTIFIER,
    @DisposalMethod NVARCHAR(100),
    @DisposalDateUtc DATETIME2(3),
    @CertificateReceivedAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [HazardousWasteManagement].[DisposalCertificates]
            ([DisposalCertificateId], [TenantId], [SiteId], [Status], [ManifestId], [TsdfFacilityId], [DisposalMethod], [DisposalDateUtc], [CertificateReceivedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@DisposalCertificateId, @TenantId, @SiteId, @Status, @ManifestId, @TsdfFacilityId, @DisposalMethod, @DisposalDateUtc, @CertificateReceivedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
