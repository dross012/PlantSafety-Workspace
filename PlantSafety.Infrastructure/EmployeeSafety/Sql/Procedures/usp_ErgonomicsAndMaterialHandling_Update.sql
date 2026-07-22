-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ErgonomicsAndMaterialHandling_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [EmployeeSafety].[ErgonomicsAndMaterialHandling], backing
--                  IErgonomicsAndMaterialHandlingRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EmployeeSafety].[usp_ErgonomicsAndMaterialHandling_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @ErgonomicsAndMaterialHandlingId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AssessmentTitle NVARCHAR(400),
    @HazardCategory NVARCHAR(100),
    @WorkstationLocation NVARCHAR(400),
    @AssessedByUserId UNIQUEIDENTIFIER,
    @AssessedAtUtc DATETIME2(3) = NULL,
    @RiskFactorCount INT,
    @RiskScore DECIMAL(9,2) = NULL,
    @ControlsDescription NVARCHAR(2000) = NULL,
    @FollowUpDueAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EmployeeSafety].[ErgonomicsAndMaterialHandling]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [AssessmentTitle] = @AssessmentTitle,
        [HazardCategory] = @HazardCategory,
        [WorkstationLocation] = @WorkstationLocation,
        [AssessedByUserId] = @AssessedByUserId,
        [AssessedAtUtc] = @AssessedAtUtc,
        [RiskFactorCount] = @RiskFactorCount,
        [RiskScore] = @RiskScore,
        [ControlsDescription] = @ControlsDescription,
        [FollowUpDueAtUtc] = @FollowUpDueAtUtc,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [ErgonomicsAndMaterialHandlingId] = @ErgonomicsAndMaterialHandlingId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
