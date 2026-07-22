-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_IncidentTrackingPerformanceEvaluation_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IIncidentTrackingPerformanceEvaluationRepository.GetIncidentTrackingPerformanceEvaluationItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_IncidentTrackingPerformanceEvaluation_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [IncidentTrackingPerformanceEvaluationId],
        [TenantId],
        [SiteId],
        [EvaluationTitle],
        [IncidentCategory] AS [IncidentCategory],
        [ContractorName],
        [Status] AS [EligibilityStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [ContractorSafety].[IncidentTrackingPerformanceEvaluation]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO
