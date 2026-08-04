-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargeMonitoringReport_Update.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure backing IDischargeMonitoringReportRepository.UpdateAsync (submission, acceptance, rejection). RetentionExpiresAtUtc is set on submission per the 5+ year regulatory-submission retention rule. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeMonitoringReport_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @DischargeMonitoringReportId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @SubmittedAtUtc DATETIME2(3) = NULL,
    @SubmittedByUserId UNIQUEIDENTIFIER = NULL,
    @SamplesWithinLimitCount INT,
    @TotalSamplesCount INT,
    @RetentionExpiresAtUtc DATETIME2(3) = NULL,
    @RejectionReason NVARCHAR(500) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [EnvironmentalHealth].[DischargeMonitoringReport]
        SET
            [Status] = @Status,
            [SubmittedAtUtc] = @SubmittedAtUtc,
            [SubmittedByUserId] = @SubmittedByUserId,
            [SamplesWithinLimitCount] = @SamplesWithinLimitCount,
            [TotalSamplesCount] = @TotalSamplesCount,
            [RetentionExpiresAtUtc] = @RetentionExpiresAtUtc,
            [RejectionReason] = @RejectionReason,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [DischargeMonitoringReportId] = @DischargeMonitoringReportId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO