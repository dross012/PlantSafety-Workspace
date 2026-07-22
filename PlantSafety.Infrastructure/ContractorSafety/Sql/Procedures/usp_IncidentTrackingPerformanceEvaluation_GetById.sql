-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_IncidentTrackingPerformanceEvaluation_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single IncidentTrackingPerformanceEvaluation row by identifier, backing
--                  IIncidentTrackingPerformanceEvaluationRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_IncidentTrackingPerformanceEvaluation_GetById]
(
    @IncidentTrackingPerformanceEvaluationId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [IncidentTrackingPerformanceEvaluationId], [TenantId], [SiteId], [Status], [EvaluationTitle], [IncidentCategory], [ContractorName], [EvaluatedByUserId], [InvestigatedAtUtc], [RootCauseSummary], [CapaCount], [PerformanceScore], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[IncidentTrackingPerformanceEvaluation]
    WHERE [IncidentTrackingPerformanceEvaluationId] = @IncidentTrackingPerformanceEvaluationId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
