-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_IncidentTrackingPerformanceEvaluation_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [ContractorSafety].[IncidentTrackingPerformanceEvaluation], backing
--                  IIncidentTrackingPerformanceEvaluationRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_IncidentTrackingPerformanceEvaluation_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
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
        INSERT INTO [ContractorSafety].[IncidentTrackingPerformanceEvaluation]
            ([IncidentTrackingPerformanceEvaluationId], [TenantId], [SiteId], [Status], [EvaluationTitle], [IncidentCategory], [ContractorName], [EvaluatedByUserId], [InvestigatedAtUtc], [RootCauseSummary], [CapaCount], [PerformanceScore], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@IncidentTrackingPerformanceEvaluationId, @TenantId, @SiteId, @Status, @EvaluationTitle, @IncidentCategory, @ContractorName, @EvaluatedByUserId, @InvestigatedAtUtc, @RootCauseSummary, @CapaCount, @PerformanceScore, @ClosedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
