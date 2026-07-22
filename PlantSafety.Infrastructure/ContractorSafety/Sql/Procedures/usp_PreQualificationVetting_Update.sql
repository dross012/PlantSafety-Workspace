-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PreQualificationVetting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PreQualificationVetting_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [ContractorSafety].[PreQualificationVetting], backing
--                  IPreQualificationVettingRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_PreQualificationVetting_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @PreQualificationVettingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @VettingTitle NVARCHAR(400),
    @ContractorName NVARCHAR(400),
    @EmrScore DECIMAL(9,2) = NULL,
    @TrirScore DECIMAL(9,2) = NULL,
    @OshaTrainingCompliant BIT,
    @DeficiencyCount INT,
    @InsuranceExpiresAtUtc DATETIME2(3) = NULL,
    @ReviewedByUserId UNIQUEIDENTIFIER,
    @VettedAtUtc DATETIME2(3) = NULL,
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @RejectionReason NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [ContractorSafety].[PreQualificationVetting]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [VettingTitle] = @VettingTitle,
        [ContractorName] = @ContractorName,
        [EmrScore] = @EmrScore,
        [TrirScore] = @TrirScore,
        [OshaTrainingCompliant] = @OshaTrainingCompliant,
        [DeficiencyCount] = @DeficiencyCount,
        [InsuranceExpiresAtUtc] = @InsuranceExpiresAtUtc,
        [ReviewedByUserId] = @ReviewedByUserId,
        [VettedAtUtc] = @VettedAtUtc,
        [ExpiresAtUtc] = @ExpiresAtUtc,
        [RejectionReason] = @RejectionReason,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [PreQualificationVettingId] = @PreQualificationVettingId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
