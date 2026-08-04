-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AutonomousVehicleSafety.SafetyCaseValidation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SafetyCaseValidation_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [AutonomousVehicleSafety].[SafetyCaseValidation], backing
--                  ISafetyCaseValidationRepository.AddAsync. Standard error envelope: TRY/CATCH
--                  with transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AutonomousVehicleSafety].[usp_SafetyCaseValidation_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
    @RejectionReason NVARCHAR(1000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [AutonomousVehicleSafety].[SafetyCaseValidation]
            ([SafetyCaseValidationId], [TenantId], [SiteId], [Status], [AutonomousVehicleAssetId], [StandardReference], [DocumentVersion], [RevalidationCadenceDays], [ValidatedByUserId], [ValidatedAtUtc], [ExpiresAtUtc], [RejectionReason], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@SafetyCaseValidationId, @TenantId, @SiteId, @Status, @AutonomousVehicleAssetId, @StandardReference, @DocumentVersion, @RevalidationCadenceDays, @ValidatedByUserId, @ValidatedAtUtc, @ExpiresAtUtc, @RejectionReason, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
