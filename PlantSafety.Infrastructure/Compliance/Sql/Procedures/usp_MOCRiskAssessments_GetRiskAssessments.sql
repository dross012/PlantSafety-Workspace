-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange.RiskAssessments
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_MOCRiskAssessments_GetRiskAssessments.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IRiskAssessmentsRepository.GetRiskAssessmentsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Compliance].[usp_MOCRiskAssessments_GetRiskAssessments]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        RiskAssessmentId,
        ChangeRequestId,
        TenantId,
        SiteId,
        RiskLevel,
        Status,
        AssessedAtUtc,
        AssessedBy
    FROM [Compliance].[MOCRiskAssessments]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND AssessedAtUtc >= @FromUtc
      AND AssessedAtUtc <= @ToUtc
    ORDER BY AssessedAtUtc DESC;
END
GO
