-- ============================================================================
--  Module: HazardousWasteManagement.DisposalCertificates | Layer: Infrastructure - SQL Scripts
--  File: usp_DisposalCertificates_Update.sql | Created: 2026-08-04
--  Description: Full-row update procedure for [HazardousWasteManagement].[DisposalCertificates]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_DisposalCertificates_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @DisposalCertificateId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [HazardousWasteManagement].[DisposalCertificates]
        SET
            [Status] = @Status,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [DisposalCertificateId] = @DisposalCertificateId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
