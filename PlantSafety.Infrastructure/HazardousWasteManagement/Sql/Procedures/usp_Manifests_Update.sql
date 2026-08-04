-- ============================================================================
--  Module: HazardousWasteManagement.Manifests | Layer: Infrastructure - SQL Scripts
--  File: usp_Manifests_Update.sql | Created: 2026-08-04
--  Description: Full-row update procedure for [HazardousWasteManagement].[Manifests]. Once
--               IsImmutable = 1 this procedure still executes (it is the sole write path used
--               by domain lifecycle transitions themselves, e.g. TSDF receipt, closure,
--               discrepancy flag) -- immutability is enforced at the aggregate/application
--               level against ad hoc field edits, not against these lifecycle transitions.
--               Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Manifests_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @HazardousWasteManifestId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(100),
    @TransporterId UNIQUEIDENTIFIER = NULL,
    @TsdfFacilityId UNIQUEIDENTIFIER = NULL,
    @TransporterSignedAtUtc DATETIME2(3) = NULL,
    @TsdfReceivedAtUtc DATETIME2(3) = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @IsImmutable BIT,
    @DiscrepancyReason NVARCHAR(2000) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [HazardousWasteManagement].[Manifests]
        SET
            [Status] = @Status,
            [TransporterId] = @TransporterId,
            [TsdfFacilityId] = @TsdfFacilityId,
            [TransporterSignedAtUtc] = @TransporterSignedAtUtc,
            [TsdfReceivedAtUtc] = @TsdfReceivedAtUtc,
            [ClosedAtUtc] = @ClosedAtUtc,
            [IsImmutable] = @IsImmutable,
            [DiscrepancyReason] = @DiscrepancyReason,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [HazardousWasteManifestId] = @HazardousWasteManifestId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
