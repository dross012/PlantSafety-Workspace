-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargeMonitoringReport_GetItems.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only procedure backing IDischargeMonitoringReportRepository.GetDischargeMonitoringReportsByPeriodAsync (Application read-model). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeMonitoringReport_GetItems]
    @TenantId       UNIQUEIDENTIFIER,
    @SiteId         UNIQUEIDENTIFIER = NULL,
    @PeriodStartUtc DATETIME2(3),
    @PeriodEndUtc   DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT [DischargeMonitoringReportId], [TenantId], [SiteId], [DischargePermitId], [ReportingPeriodStartUtc],
           [ReportingPeriodEndUtc], [DueAtUtc], [SubmittedAtUtc], [SamplesWithinLimitCount], [TotalSamplesCount],
           [Status], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[DischargeMonitoringReport]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [ReportingPeriodStartUtc] <= @PeriodEndUtc
      AND [ReportingPeriodEndUtc] >= @PeriodStartUtc
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [ReportingPeriodStartUtc] DESC;
END
GO