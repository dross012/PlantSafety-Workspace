-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_IncidentTrackingPerformanceEvaluation_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [ContractorSafety].[IncidentTrackingPerformanceEvaluation], backing
--                  IIncidentTrackingPerformanceEvaluationRepository.UpdateAsync (all lifecycle transitions persist the
--                  aggregate's full current state). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_IncidentTrackingPerformanceEvaluation_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @IncidentTrackingPerformanceEvaluationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @EvaluationTitle NVARCHAR(400),
    @IncidentCategory NVARCHAR(100),
    @ContractorName NVARCHAR(400),
    @EvaluatedByUserId UNIQUEIDENTIFIER,
    @InvestigatedAtUtc DATETIME2(3) = NULL,
    @RootCauseSummary NVARCHAR(2000) = NULL,
    @CapaCount INT,
    @PerformanceScore DECIMAL(9,2) = NULL,
    @ClosedAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [ContractorSafety].[IncidentTrackingPerformanceEvaluation]
        SET
        [SiteId] = @SiteId,
        [Status] = @Status,
        [EvaluationTitle] = @EvaluationTitle,
        [IncidentCategory] = @IncidentCategory,
        [ContractorName] = @ContractorName,
        [EvaluatedByUserId] = @EvaluatedByUserId,
        [InvestigatedAtUtc] = @InvestigatedAtUtc,
        [RootCauseSummary] = @RootCauseSummary,
        [CapaCount] = @CapaCount,
        [PerformanceScore] = @PerformanceScore,
        [ClosedAtUtc] = @ClosedAtUtc,
        [ModifiedAtUtc] = @NowUtc,
        [ModifiedBy] = @ActorId,
        [CorrelationId] = @CorrelationId
        WHERE [IncidentTrackingPerformanceEvaluationId] = @IncidentTrackingPerformanceEvaluationId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
