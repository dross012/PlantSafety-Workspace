-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.CultureAssessment.TrainingCompletionRate
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_TrainingCompletionRate_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing ITrainingCompletionRateRepository.GetItemsAsync. Thin,
--                  metric-scoped projection over the shared
--                  [SafetyCulture].[CultureMetricRecord] table, filtered to the
--                  'TrainingCompletionRate' metric (Training Completion Rate).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_TrainingCompletionRate_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
SELECT
    [ItemId], [CultureAssessmentId], [TenantId], [SiteId], [Title], [Status], [RecordedAtUtc]
FROM [SafetyCulture].[CultureMetricRecord]
WHERE [MetricType] = N'TrainingCompletionRate'
  AND [TenantId]     = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND [RecordedAtUtc] BETWEEN @FromUtc AND @ToUtc
  AND ISNULL([IsDeleted], 0) = 0
ORDER BY [RecordedAtUtc] DESC;
GO
