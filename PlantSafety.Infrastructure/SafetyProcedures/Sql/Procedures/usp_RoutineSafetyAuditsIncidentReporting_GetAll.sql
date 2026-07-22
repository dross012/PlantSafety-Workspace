-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RoutineSafetyAuditsIncidentReporting_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted RoutineSafetyAuditsIncidentReporting rows, backing
--                  IRoutineSafetyAuditsIncidentReportingRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_RoutineSafetyAuditsIncidentReporting_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [RoutineSafetyAuditsIncidentReportingId], [TenantId], [SiteId], [Status], [AuditTitle], [ReportingCategory], [FacilityArea], [ConductedByUserId], [ConductedAtUtc], [NextAuditDueAtUtc], [FindingCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
