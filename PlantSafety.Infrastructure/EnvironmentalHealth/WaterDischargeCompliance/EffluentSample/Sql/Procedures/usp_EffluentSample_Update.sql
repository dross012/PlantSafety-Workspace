-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.EffluentSample
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EffluentSample_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Status-only update procedure backing IEffluentSampleRepository.UpdateAsync (marks Corrected/Voided). All other sample fields are immutable once recorded. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_EffluentSample_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @EffluentSampleId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EnvironmentalHealth].[EffluentSample]
        SET
            [Status] = @Status,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [EffluentSampleId] = @EffluentSampleId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO