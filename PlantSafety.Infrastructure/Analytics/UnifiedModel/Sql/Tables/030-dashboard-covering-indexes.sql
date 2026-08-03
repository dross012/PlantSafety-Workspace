-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           030-dashboard-covering-indexes.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (SQL & Database Agent, Phase 5 hardening pass)
--  SchemaVersion:  2026.1
--  Description:    Adds the covering indexes actually needed by the six
--                  usp_*Dashboard_GetDaily read-heavy aggregation procedures.
--
--                  Gap found: every fact table already carries an IX_<Fact>_Tenant_Date index
--                  on (TenantId, DateKey) -- the DimDate surrogate key -- but none of the six
--                  dashboard procedures filter or GROUP BY DateKey. They all filter/group by
--                  the fact's own business-event UTC timestamp column instead
--                  (CapturedAtUtc/RaisedAtUtc/DueAtUtc/OccurredAtUtc/ScheduledAtUtc/
--                  OpenedAtUtc), each further filtered by an optional @SiteId. The existing
--                  (TenantId, DateKey) index therefore cannot be used to seek this predicate at
--                  all, forcing a full scan of every fact table on every dashboard call. This
--                  script adds one covering (TenantId, <EventTimestamp>) index per fact table,
--                  INCLUDE-ing exactly the columns each dashboard proc's SELECT/aggregate list
--                  needs, so all six procedures become fully index-covered (seek + no key
--                  lookups). Idempotent (guarded by sys.indexes existence checks);
--                  additive-only, matches the "Additive-only evolution" convention used
--                  throughout this schema.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

-- usp_RiskDashboard_GetDaily: filters/groups CapturedAtUtc, filters SiteKey; aggregates
-- RiskLevelCode, CompositeScore, IsResidualScore.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FactRiskAssessment_Tenant_CapturedAtUtc' AND object_id = OBJECT_ID('[AnalyticsUnifiedModel].[FactRiskAssessment]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FactRiskAssessment_Tenant_CapturedAtUtc
        ON [AnalyticsUnifiedModel].[FactRiskAssessment] (TenantId, CapturedAtUtc)
        INCLUDE (SiteKey, RiskLevelCode, CompositeScore, IsResidualScore);
END
GO

-- usp_AuditDashboard_GetDaily: filters/groups RaisedAtUtc, filters SiteKey; aggregates
-- SeverityCode, ClosedAtUtc, WasRemediatedOnTime, RemediationDays.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FactAuditFinding_Tenant_RaisedAtUtc' AND object_id = OBJECT_ID('[AnalyticsUnifiedModel].[FactAuditFinding]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FactAuditFinding_Tenant_RaisedAtUtc
        ON [AnalyticsUnifiedModel].[FactAuditFinding] (TenantId, RaisedAtUtc)
        INCLUDE (SiteKey, SeverityCode, ClosedAtUtc, WasRemediatedOnTime, RemediationDays);
END
GO

-- usp_MaintenanceDashboard_GetDaily: filters/groups DueAtUtc, filters SiteKey; aggregates
-- MaintenanceType, WasOnSchedule, Status, DowntimeMinutes, PredictedFailureProbability.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FactMaintenanceWorkOrder_Tenant_DueAtUtc' AND object_id = OBJECT_ID('[AnalyticsUnifiedModel].[FactMaintenanceWorkOrder]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FactMaintenanceWorkOrder_Tenant_DueAtUtc
        ON [AnalyticsUnifiedModel].[FactMaintenanceWorkOrder] (TenantId, DueAtUtc)
        INCLUDE (SiteKey, MaintenanceType, WasOnSchedule, Status, DowntimeMinutes, PredictedFailureProbability);
END
GO

-- usp_ProductionDashboard_GetDaily: filters/groups OccurredAtUtc, filters SiteKey; aggregates
-- IsCompliant, DiscrepancyCount.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FactOperationalEvent_Tenant_OccurredAtUtc' AND object_id = OBJECT_ID('[AnalyticsUnifiedModel].[FactOperationalEvent]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FactOperationalEvent_Tenant_OccurredAtUtc
        ON [AnalyticsUnifiedModel].[FactOperationalEvent] (TenantId, OccurredAtUtc)
        INCLUDE (SiteKey, IsCompliant, DiscrepancyCount);
END
GO

-- usp_SchedulingDashboard_GetDaily: filters/groups ScheduledAtUtc, filters SiteKey; aggregates
-- WasOnTime, CompletedAtUtc, VarianceMinutes, PriorityCode.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FactSchedulingEvent_Tenant_ScheduledAtUtc' AND object_id = OBJECT_ID('[AnalyticsUnifiedModel].[FactSchedulingEvent]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FactSchedulingEvent_Tenant_ScheduledAtUtc
        ON [AnalyticsUnifiedModel].[FactSchedulingEvent] (TenantId, ScheduledAtUtc)
        INCLUDE (SiteKey, WasOnTime, CompletedAtUtc, VarianceMinutes, PriorityCode);
END
GO

-- usp_ContinuousImprovementDashboard_GetDaily: filters/groups OpenedAtUtc, filters SiteKey;
-- aggregates ImprovementSubContext, ClosedAtUtc, WasClosedOnTime, CycleTimeDays,
-- EffectivenessVerified.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FactCIAction_Tenant_OpenedAtUtc' AND object_id = OBJECT_ID('[AnalyticsUnifiedModel].[FactContinuousImprovementAction]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FactCIAction_Tenant_OpenedAtUtc
        ON [AnalyticsUnifiedModel].[FactContinuousImprovementAction] (TenantId, OpenedAtUtc)
        INCLUDE (SiteKey, ImprovementSubContext, ClosedAtUtc, WasClosedOnTime, CycleTimeDays, EffectivenessVerified);
END
GO
