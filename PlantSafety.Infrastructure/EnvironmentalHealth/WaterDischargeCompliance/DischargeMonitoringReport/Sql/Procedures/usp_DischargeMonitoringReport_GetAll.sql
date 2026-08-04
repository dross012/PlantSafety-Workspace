-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargeMonitoringReport_GetAll.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted DischargeMonitoringReport rows, backing IDischargeMonitoringReportRepository.ListAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargeMonitoringReport_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [DischargeMonitoringReportId], [TenantId], [SiteId], [Status], [DischargePermitId], [ReportingPeriodStartUtc],
           [ReportingPeriodEndUtc], [DueAtUtc], [SubmittedAtUtc], [SubmittedByUserId], [SamplesWithinLimitCount],
           [TotalSamplesCount], [RetentionExpiresAtUtc], [RejectionReason], [CreatedAtUtc], [CreatedBy]
    FROM [EnvironmentalHealth].[DischargeMonitoringReport]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO