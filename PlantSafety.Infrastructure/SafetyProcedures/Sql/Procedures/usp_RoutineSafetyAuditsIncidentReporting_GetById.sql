-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RoutineSafetyAuditsIncidentReporting_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single RoutineSafetyAuditsIncidentReporting row by identifier, backing
--                  IRoutineSafetyAuditsIncidentReportingRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_RoutineSafetyAuditsIncidentReporting_GetById]
(
    @RoutineSafetyAuditsIncidentReportingId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [RoutineSafetyAuditsIncidentReportingId], [TenantId], [SiteId], [Status], [AuditTitle], [ReportingCategory], [FacilityArea], [ConductedByUserId], [ConductedAtUtc], [NextAuditDueAtUtc], [FindingCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting]
    WHERE [RoutineSafetyAuditsIncidentReportingId] = @RoutineSafetyAuditsIncidentReportingId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
