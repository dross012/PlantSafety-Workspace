-- ============================================================================
--  Module: HazardousWasteManagement.Manifests | Layer: Infrastructure - SQL Scripts
--  File: usp_Manifests_Create.sql | Created: 2026-08-04
--  Description: Insert procedure for [HazardousWasteManagement].[Manifests]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @HazardousWasteManifestId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @ManifestNumber NVARCHAR(50),
    @GeneratorEpaId NVARCHAR(50),
    @WasteStreamId UNIQUEIDENTIFIER,
    @GeneratedAtUtc DATETIME2(3),
    @IsImmutable BIT,
    @RetentionExpiresAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [HazardousWasteManagement].[Manifests]
            ([HazardousWasteManifestId], [TenantId], [SiteId], [Status], [ManifestNumber], [GeneratorEpaId], [WasteStreamId], [GeneratedAtUtc], [IsImmutable], [RetentionExpiresAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@HazardousWasteManifestId, @TenantId, @SiteId, @Status, @ManifestNumber, @GeneratorEpaId, @WasteStreamId, @GeneratedAtUtc, @IsImmutable, @RetentionExpiresAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
