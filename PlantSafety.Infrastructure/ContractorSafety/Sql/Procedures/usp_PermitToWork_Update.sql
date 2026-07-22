-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.PermitToWork
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PermitToWork_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [ContractorSafety].[PermitToWork], backing
--                  IPermitToWorkRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_PermitToWork_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @PermitToWorkId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @PermitTitle NVARCHAR(400),
    @PermitType NVARCHAR(100),
    @WorkLocation NVARCHAR(400),
    @HazardCount INT,
    @RequiresAtmosphericTesting BIT,
    @AtmosphericTestScore DECIMAL(9,2) = NULL,
    @AuthorizedByUserId UNIQUEIDENTIFIER,
    @IssuedAtUtc DATETIME2(3) = NULL,
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @SuspensionReason NVARCHAR(2000) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [ContractorSafety].[PermitToWork]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [PermitTitle] = @PermitTitle,
        [PermitType] = @PermitType,
        [WorkLocation] = @WorkLocation,
        [HazardCount] = @HazardCount,
        [RequiresAtmosphericTesting] = @RequiresAtmosphericTesting,
        [AtmosphericTestScore] = @AtmosphericTestScore,
        [AuthorizedByUserId] = @AuthorizedByUserId,
        [IssuedAtUtc] = @IssuedAtUtc,
        [ExpiresAtUtc] = @ExpiresAtUtc,
        [SuspensionReason] = @SuspensionReason,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [PermitToWorkId] = @PermitToWorkId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
