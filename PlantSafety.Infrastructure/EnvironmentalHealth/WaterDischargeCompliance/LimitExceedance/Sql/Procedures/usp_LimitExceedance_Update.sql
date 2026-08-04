-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_LimitExceedance_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure backing ILimitExceedanceRepository.UpdateAsync (response initiation/closure). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_LimitExceedance_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @LimitExceedanceId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @ResponseInitiatedAtUtc DATETIME2(3) = NULL,
    @ResponseInitiatedByUserId UNIQUEIDENTIFIER = NULL,
    @ResponseNotes NVARCHAR(1000) = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @ClosedByUserId UNIQUEIDENTIFIER = NULL,
    @ResolutionNotes NVARCHAR(1000) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EnvironmentalHealth].[LimitExceedance]
        SET
            [Status] = @Status,
            [ResponseInitiatedAtUtc] = @ResponseInitiatedAtUtc,
            [ResponseInitiatedByUserId] = @ResponseInitiatedByUserId,
            [ResponseNotes] = @ResponseNotes,
            [ClosedAtUtc] = @ClosedAtUtc,
            [ClosedByUserId] = @ClosedByUserId,
            [ResolutionNotes] = @ResolutionNotes,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [LimitExceedanceId] = @LimitExceedanceId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO