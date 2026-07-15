-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Predictive.FatigueRiskModeling
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_FatigueRiskModeling_List.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Analytics].[FatigueRiskModeling],
--                  backing IFatigueRiskModelingRepository.GetFatigueRiskAssessmentsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_FatigueRiskModeling_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RiskAssessmentId],
        [TenantId],
        [SiteId],
        [EmployeeId],
        [FatigueRiskScore],
        [RiskLevel],
        [Status],
        [AssessedAtUtc]
FROM [Analytics].[FatigueRiskModeling]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AssessedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AssessedAtUtc] DESC;
GO
