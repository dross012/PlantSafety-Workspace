-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RoutineSafetyAuditsIncidentReporting_GetItems.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IRoutineSafetyAuditsIncidentReportingRepository.GetRoutineSafetyAuditsIncidentReportingItemsAsync (Application read-model).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_RoutineSafetyAuditsIncidentReporting_GetItems]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [RoutineSafetyAuditsIncidentReportingId],
        [TenantId],
        [SiteId],
        [AuditTitle],
        [ReportingCategory] AS [ReportingCategory],
        [FacilityArea],
        [Status] AS [AuditStatus],
        [CreatedAtUtc],
        [CreatedBy]
    FROM [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ISNULL([IsDeleted], 0) = 0
      AND CreatedAtUtc >= @FromUtc
      AND CreatedAtUtc <= @ToUtc
    ORDER BY CreatedAtUtc DESC;
END
GO
