-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.SafetyCaseValidation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyCaseValidation_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [AutonomousVehicleSafety].[SafetyCaseValidation],
--                  backing ISafetyCaseValidationRepository.UpdateAsync. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_SafetyCaseValidation_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @SafetyCaseValidationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AutonomousVehicleAssetId UNIQUEIDENTIFIER,
    @StandardReference NVARCHAR(100),
    @DocumentVersion NVARCHAR(50),
    @RevalidationCadenceDays INT,
    @ValidatedByUserId UNIQUEIDENTIFIER = NULL,
    @ValidatedAtUtc DATETIME2(3) = NULL,
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @RejectionReason NVARCHAR(1000) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [AutonomousVehicleSafety].[SafetyCaseValidation]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [AutonomousVehicleAssetId] = @AutonomousVehicleAssetId,
        [StandardReference] = @StandardReference,
        [DocumentVersion] = @DocumentVersion,
        [RevalidationCadenceDays] = @RevalidationCadenceDays,
        [ValidatedByUserId] = @ValidatedByUserId,
        [ValidatedAtUtc] = @ValidatedAtUtc,
        [ExpiresAtUtc] = @ExpiresAtUtc,
        [RejectionReason] = @RejectionReason,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [SafetyCaseValidationId] = @SafetyCaseValidationId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
