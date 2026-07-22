-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_IncidentTrackingPerformanceEvaluation_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted IncidentTrackingPerformanceEvaluation rows, backing
--                  IIncidentTrackingPerformanceEvaluationRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_IncidentTrackingPerformanceEvaluation_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [IncidentTrackingPerformanceEvaluationId], [TenantId], [SiteId], [Status], [EvaluationTitle], [IncidentCategory], [ContractorName], [EvaluatedByUserId], [InvestigatedAtUtc], [RootCauseSummary], [CapaCount], [PerformanceScore], [ClosedAtUtc], [CreatedAtUtc], [CreatedBy]
    FROM [ContractorSafety].[IncidentTrackingPerformanceEvaluation]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
