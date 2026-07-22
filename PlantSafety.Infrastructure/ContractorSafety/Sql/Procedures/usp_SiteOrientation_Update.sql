-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.SiteOrientation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SiteOrientation_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [ContractorSafety].[SiteOrientation], backing
--                  ISiteOrientationRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_SiteOrientation_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @SiteOrientationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @OrientationTitle NVARCHAR(400),
    @ContractorName NVARCHAR(400),
    @ConductedByUserId UNIQUEIDENTIFIER,
    @DeliveredAtUtc DATETIME2(3) = NULL,
    @AttendeeCount INT,
    @ComprehensionScore DECIMAL(9,2) = NULL,
    @PassingScoreThreshold DECIMAL(9,2),
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [ContractorSafety].[SiteOrientation]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [OrientationTitle] = @OrientationTitle,
        [ContractorName] = @ContractorName,
        [ConductedByUserId] = @ConductedByUserId,
        [DeliveredAtUtc] = @DeliveredAtUtc,
        [AttendeeCount] = @AttendeeCount,
        [ComprehensionScore] = @ComprehensionScore,
        [PassingScoreThreshold] = @PassingScoreThreshold,
        [ExpiresAtUtc] = @ExpiresAtUtc,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [SiteOrientationId] = @SiteOrientationId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
