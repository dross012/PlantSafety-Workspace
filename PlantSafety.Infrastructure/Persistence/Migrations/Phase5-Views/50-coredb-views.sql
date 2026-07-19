USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 5 Views & Summaries
-- Script:        50-coredb-views.sql
-- Author:        View & Summary Builder (unattended agent run)
-- Purpose:       Deterministic active-record views, "with latest history" cross-table
--                reporting views, and materialized summary tables + refresh procedures
--                for PlantSafety.CoreDB, grounded in the real Phase1-4 schema (58 tables,
--                16 schemas, 23 base<->HistoryEntry FK pairs) and the FY2027
--                enterprise_data_database_architecture_master.md canonical templates.
--
-- SECTION 1 -- Active-record views (35)
--   One vw_<Entity>_Active view per non-HistoryEntry table (35 base tables across all
--   16 schemas), explicit column list matching the table's real declared columns,
--   WHERE ISNULL(IsDeleted,0)=0. The 23 *HistoryEntry tables are intentionally excluded
--   -- they are append-only audit-trail tables queried by parent FK (e.g. WHERE
--   RiskManagementId = @id), never by an "active" filter.
--
-- SECTION 2 -- Cross-table "WithLatestHistory" views (23)
--   For each of the 23 base<->HistoryEntry FK pairs, a vw_<Entity>_WithLatestHistory
--   view OUTER APPLYs the single most-recent HistoryEntry row (by ChangedAtUtc DESC)
--   onto the active base record -- genuinely useful for "current status + last
--   transition" reporting/UI screens, mirrors the SafetyEvents.vw_IncidentSummary
--   pattern named in scope. Single-schema joins only (no cross-module reads).
--
-- SECTION 3 -- Materialized summary tables + refresh procedures (3)
--   SafetyEvents.IncidentManagementSummary, Operational.RiskManagementSummary,
--   Operational.AlertSummary -- see inline rationale comment. Full-rebuild refresh
--   pattern, TRY/CATCH + XACT_ABORT, best-effort audit logging, mandatory error
--   logging into PlantSafety.AuditDB.dbo.AuditLog / PlantSafety.ErrorDB.dbo.ErrorLog
--   (bracketed 3-part names -- the embedded '.' in the DB name requires [brackets],
--   confirmed live: unbracketed PlantSafety.AuditDB.dbo.AuditLog is misparsed by SQL
--   Server as a 4-part linked-server name and fails with error 7202).
--
-- Deliberately NOT created for CoreDB:
--   - Tenant-scoped parameterized views: SQL Server views cannot accept parameters.
--     The established tenant-scoping pattern in this codebase is the parameterized
--     inline TVF (Operational.fn_RiskManagement_Active, SafetyEvents.fn_IncidentManagement_Active,
--     SafetyEvents.fn_EmergencyManagement_Active, SafetyEvents.fn_Observation_Active,
--     Operational.fn_Alert_Active), already built by the Trigger & Function Engineer
--     in Phase 4 for the 5 highest-traffic tables. No competing view-based
--     TenantId-filter pattern is invented here.
--   - Summary tables for the other 32 base tables: no genuine time-series/aggregation
--     access pattern beyond "WHERE TenantId=@t AND IsDeleted=0" was found for them
--     (already served by their vw_<Entity>_Active view); see the final report's
--     recommend-optional-summaries notes for candidates the Analytics schema tables
--     (Culture/Dashboard/Engagement/PredictiveAnalytics/ScoreCards) already ARE
--     themselves summary/rollup-shaped tables, so a further summary-of-summary was
--     judged unwarranted.
--
-- Idempotent:    CREATE OR ALTER views/procedures; IF NOT EXISTS guards for tables.
-- =============================================

-- ===================== SECTION 1: Active-record views =====================

CREATE OR ALTER VIEW AiGovernance.vw_AiGovernance_Active
AS
SELECT
    e.AiGovernanceId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM AiGovernance.AiGovernance AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW AiSafetyIntelligence.vw_AiSafetyIntelligence_Active
AS
SELECT
    e.AiSafetyIntelligenceId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM AiSafetyIntelligence.AiSafetyIntelligence AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Analytics.vw_Culture_Active
AS
SELECT
    e.CultureId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.SurveyPeriod, e.CultureIndexScore, e.EngagementScore, e.ParticipationRate, e.TopRiskTheme
FROM Analytics.Culture AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Analytics.vw_Dashboard_Active
AS
SELECT
    e.DashboardId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.DashboardName, e.DashboardType, e.OwnerId, e.RefreshIntervalSeconds, e.WidgetCount, e.IsPublished, e.LastRefreshedAtUtc
FROM Analytics.Dashboard AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Analytics.vw_Engagement_Active
AS
SELECT
    e.EngagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.WorkerId, e.EngagementScore, e.SurveyPeriod, e.ParticipationChannel, e.ResponseCount, e.LastEngagedAtUtc
FROM Analytics.Engagement AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Analytics.vw_PredictiveAnalytics_Active
AS
SELECT
    e.PredictiveAnalyticsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ModelName, e.PredictionType, e.TargetEntityId, e.PredictedRiskScore, e.ConfidenceLevel, e.PredictionHorizonDays, e.GeneratedAtUtc
FROM Analytics.PredictiveAnalytics AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Analytics.vw_ScoreCards_Active
AS
SELECT
    e.ScoreCardsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ScoreCardName, e.ReportingPeriod, e.KpiCategory, e.PerformanceIndex, e.GeneratedForSiteId, e.GeneratedAtUtc
FROM Analytics.ScoreCards AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Compliance.vw_Auditing_Active
AS
SELECT
    e.AuditingId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.AuditTitle, e.AuditType, e.Scope, e.AuditorId, e.ScheduledAtUtc, e.CompletedAtUtc, e.FindingsCount, e.Score, e.ComplianceRating
FROM Compliance.Auditing AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Compliance.vw_Certification_Active
AS
SELECT
    e.CertificationId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.HolderWorkerId, e.CertificationName, e.CertificationBody, e.CertificationNumber, e.IssuedAtUtc, e.ExpiresAtUtc, e.RenewalRequired
FROM Compliance.Certification AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Compliance.vw_Compliance_Active
AS
SELECT
    e.ComplianceId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.RegulationReference, e.RuleDescription, e.ComplianceScore, e.NonConformanceCount, e.ResponsiblePartyId, e.LastEvaluatedAtUtc
FROM Compliance.Compliance AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Compliance.vw_ManagementOfChange_Active
AS
SELECT
    e.ManagementOfChangeId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ChangeTitle, e.ChangeType, e.Justification, e.RiskLevel, e.RequestedByUserId, e.ApprovedByUserId, e.ApprovalStatus, e.ImplementationAtUtc, e.ReviewDueAtUtc
FROM Compliance.ManagementOfChange AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Compliance.vw_Permits_Active
AS
SELECT
    e.PermitsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.PermitType, e.PermitNumber, e.WorkDescription, e.Location, e.HazardsIdentified, e.RequestedByUserId, e.ApprovedByUserId, e.IssuedAtUtc, e.ExpiresAtUtc
FROM Compliance.Permits AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Compliance.vw_Policy_Active
AS
SELECT
    e.PolicyId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.PolicyTitle, e.PolicyVersion, e.Category, e.OwnerId, e.EffectiveAtUtc, e.AcknowledgmentRequired, e.AcknowledgmentCount
FROM Compliance.Policy AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Compliance.vw_Procedures_Active
AS
SELECT
    e.ProceduresId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ProcedureTitle, e.ProcedureNumber, e.Category, e.RevisionNumber, e.OwnerId, e.EffectiveAtUtc, e.ReviewDueAtUtc
FROM Compliance.Procedures AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Compliance.vw_RegulatoryCalendar_Active
AS
SELECT
    e.RegulatoryCalendarId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.EventTitle, e.RegulatoryBody, e.JurisdictionCode, e.ComplianceCategory, e.ResponsiblePartyId, e.DueAtUtc, e.ReminderLeadDays
FROM Compliance.RegulatoryCalendar AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW ContractorVisitor.vw_ContractorVisitor_Active
AS
SELECT
    e.ContractorVisitorId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM ContractorVisitor.ContractorVisitor AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW DigitalTwin.vw_DigitalTwin_Active
AS
SELECT
    e.DigitalTwinId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM DigitalTwin.DigitalTwin AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW EnterpriseFabric.vw_EnterpriseFabric_Active
AS
SELECT
    e.EnterpriseFabricId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM EnterpriseFabric.EnterpriseFabric AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW EnvironmentalHealth.vw_EnvironmentalHealth_Active
AS
SELECT
    e.EnvironmentalHealthId, e.TenantId, e.SiteId, e.MetricType, e.MeasurementValue, e.Unit, e.Threshold, e.Status, e.RecordedAtUtc, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM EnvironmentalHealth.EnvironmentalHealth AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Governance.vw_Governance_Active
AS
SELECT
    e.GovernanceId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM Governance.Governance AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW HumanFactors.vw_HumanFactors_Active
AS
SELECT
    e.HumanFactorsId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM HumanFactors.HumanFactors AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Operational.vw_Alert_Active
AS
SELECT
    e.AlertId, e.AlertType, e.Severity, e.Status, e.Title, e.Description, e.SourceModule, e.AcknowledgedAtUtc, e.InProgressAtUtc, e.EscalatedAtUtc, e.ResolvedAtUtc, e.ClosedAtUtc, e.ArchivedAtUtc, e.AcknowledgedBy, e.InProgressBy, e.EscalatedBy, e.ResolvedBy, e.ClosedBy, e.ArchivedBy, e.EscalationReason, e.ResolutionSummary, e.Notes, e.SlaExpiration, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM Operational.Alert AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Operational.vw_Checklists_Active
AS
SELECT
    e.ChecklistsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ChecklistTitle, e.ChecklistType, e.Location, e.TotalItems, e.CompletedItems, e.ExceptionCount, e.CompletedByUserId, e.CompletedAtUtc
FROM Operational.Checklists AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Operational.vw_Contractor_Active
AS
SELECT
    e.ContractorId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ContractorCompany, e.ContractorName, e.QualificationStatus, e.ReadinessScore, e.PrimaryContactId, e.InsuranceExpiresAtUtc, e.OnboardedAtUtc
FROM Operational.Contractor AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Operational.vw_PPEManagement_Active
AS
SELECT
    e.PPEManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.WorkerId, e.PpeType, e.PpeItem, e.ComplianceStatus, e.IssuedAtUtc, e.ReturnedAtUtc, e.InspectionDueAtUtc
FROM Operational.PPEManagement AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Operational.vw_RiskManagement_Active
AS
SELECT
    e.RiskManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.HazardDescription, e.RiskCategory, e.LikelihoodRating, e.SeverityRating, e.ControlMeasures, e.ResponsiblePartyId, e.ReviewDueAtUtc
FROM Operational.RiskManagement AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Operational.vw_Schedules_Active
AS
SELECT
    e.SchedulesId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.TaskName, e.RecurrencePattern, e.AssignedToUserId, e.DueAtUtc, e.SlaWindowMinutes, e.CompletedAtUtc
FROM Operational.Schedules AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Operational.vw_Training_Active
AS
SELECT
    e.TrainingId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.CourseName, e.CourseCode, e.WorkerId, e.ComplianceCategory, e.AssignedAtUtc, e.DueAtUtc, e.CompletedAtUtc, e.Score
FROM Operational.Training AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW OperationalSafety.vw_OperationalSafety_Active
AS
SELECT
    e.OperationalSafetyId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM OperationalSafety.OperationalSafety AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW ProcessManagement.vw_ProcessDefinition_Active
AS
SELECT
    e.ProcessDefinitionId, e.TenantId, e.SiteId, e.ProcessName, e.Category, e.Status, e.Version, e.OwnerId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM ProcessManagement.ProcessDefinition AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW SafetyEvents.vw_EmergencyManagement_Active
AS
SELECT
    e.EmergencyManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.EmergencyType, e.Severity, e.Location, e.CasualtyCount, e.ResponseTeamNotified, e.DeclaredAtUtc, e.StabilizedAtUtc, e.ResolvedAtUtc
FROM SafetyEvents.EmergencyManagement AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW SafetyEvents.vw_IncidentManagement_Active
AS
SELECT
    e.IncidentManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.Title, e.Description, e.Severity, e.Location, e.IncidentOccurredAtUtc, e.InjuryType, e.ReportedByWorkerId, e.AssignedInvestigatorId, e.RootCause, e.CorrectiveActions, e.ClosedAtUtc
FROM SafetyEvents.IncidentManagement AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW SafetyEvents.vw_Observation_Active
AS
SELECT
    e.ObservationId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ObserverId, e.ObservationType, e.HazardCategory, e.Location, e.Description, e.CorrectiveActionRequired, e.FollowUpDueAtUtc
FROM SafetyEvents.Observation AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Sustainability.vw_Sustainability_Active
AS
SELECT
    e.SustainabilityId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
FROM Sustainability.Sustainability AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW WorkerWellBeing.vw_WorkerWellBeing_Active
AS
SELECT
    e.WorkerWellBeingId, e.TenantId, e.WorkerId, e.WellBeingRiskLevel, e.Status, e.RecordedAtUtc, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM WorkerWellBeing.WorkerWellBeing AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- ===================== SECTION 2: Cross-table WithLatestHistory views =====================

-- =============================================
-- View:          Analytics.vw_Culture_WithLatestHistory
-- Purpose:       Active Culture record joined to its single most-recent
--                CultureHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Analytics.vw_Culture_WithLatestHistory
AS
SELECT
    e.CultureId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Analytics.Culture AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Analytics.CultureHistoryEntry AS h
    WHERE h.CultureId = e.CultureId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Analytics.vw_Dashboard_WithLatestHistory
-- Purpose:       Active Dashboard record joined to its single most-recent
--                DashboardHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Analytics.vw_Dashboard_WithLatestHistory
AS
SELECT
    e.DashboardId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Analytics.Dashboard AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Analytics.DashboardHistoryEntry AS h
    WHERE h.DashboardId = e.DashboardId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Analytics.vw_Engagement_WithLatestHistory
-- Purpose:       Active Engagement record joined to its single most-recent
--                EngagementHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Analytics.vw_Engagement_WithLatestHistory
AS
SELECT
    e.EngagementId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Analytics.Engagement AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Analytics.EngagementHistoryEntry AS h
    WHERE h.EngagementId = e.EngagementId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Analytics.vw_PredictiveAnalytics_WithLatestHistory
-- Purpose:       Active PredictiveAnalytics record joined to its single most-recent
--                PredictiveAnalyticsHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Analytics.vw_PredictiveAnalytics_WithLatestHistory
AS
SELECT
    e.PredictiveAnalyticsId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Analytics.PredictiveAnalytics AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Analytics.PredictiveAnalyticsHistoryEntry AS h
    WHERE h.PredictiveAnalyticsId = e.PredictiveAnalyticsId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Analytics.vw_ScoreCards_WithLatestHistory
-- Purpose:       Active ScoreCards record joined to its single most-recent
--                ScoreCardsHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Analytics.vw_ScoreCards_WithLatestHistory
AS
SELECT
    e.ScoreCardsId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Analytics.ScoreCards AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Analytics.ScoreCardsHistoryEntry AS h
    WHERE h.ScoreCardsId = e.ScoreCardsId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Compliance.vw_Auditing_WithLatestHistory
-- Purpose:       Active Auditing record joined to its single most-recent
--                AuditingHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Compliance.vw_Auditing_WithLatestHistory
AS
SELECT
    e.AuditingId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Compliance.Auditing AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Compliance.AuditingHistoryEntry AS h
    WHERE h.AuditingId = e.AuditingId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Compliance.vw_Certification_WithLatestHistory
-- Purpose:       Active Certification record joined to its single most-recent
--                CertificationHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Compliance.vw_Certification_WithLatestHistory
AS
SELECT
    e.CertificationId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Compliance.Certification AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Compliance.CertificationHistoryEntry AS h
    WHERE h.CertificationId = e.CertificationId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Compliance.vw_Compliance_WithLatestHistory
-- Purpose:       Active Compliance record joined to its single most-recent
--                ComplianceHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Compliance.vw_Compliance_WithLatestHistory
AS
SELECT
    e.ComplianceId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Compliance.Compliance AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Compliance.ComplianceHistoryEntry AS h
    WHERE h.ComplianceId = e.ComplianceId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Compliance.vw_ManagementOfChange_WithLatestHistory
-- Purpose:       Active ManagementOfChange record joined to its single most-recent
--                ManagementOfChangeHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Compliance.vw_ManagementOfChange_WithLatestHistory
AS
SELECT
    e.ManagementOfChangeId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Compliance.ManagementOfChange AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Compliance.ManagementOfChangeHistoryEntry AS h
    WHERE h.ManagementOfChangeId = e.ManagementOfChangeId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Compliance.vw_Permits_WithLatestHistory
-- Purpose:       Active Permits record joined to its single most-recent
--                PermitsHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Compliance.vw_Permits_WithLatestHistory
AS
SELECT
    e.PermitsId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Compliance.Permits AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Compliance.PermitsHistoryEntry AS h
    WHERE h.PermitsId = e.PermitsId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Compliance.vw_Policy_WithLatestHistory
-- Purpose:       Active Policy record joined to its single most-recent
--                PolicyHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Compliance.vw_Policy_WithLatestHistory
AS
SELECT
    e.PolicyId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Compliance.Policy AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Compliance.PolicyHistoryEntry AS h
    WHERE h.PolicyId = e.PolicyId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Compliance.vw_Procedures_WithLatestHistory
-- Purpose:       Active Procedures record joined to its single most-recent
--                ProceduresHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Compliance.vw_Procedures_WithLatestHistory
AS
SELECT
    e.ProceduresId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Compliance.Procedures AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Compliance.ProceduresHistoryEntry AS h
    WHERE h.ProceduresId = e.ProceduresId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Compliance.vw_RegulatoryCalendar_WithLatestHistory
-- Purpose:       Active RegulatoryCalendar record joined to its single most-recent
--                RegulatoryCalendarHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Compliance.vw_RegulatoryCalendar_WithLatestHistory
AS
SELECT
    e.RegulatoryCalendarId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Compliance.RegulatoryCalendar AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Compliance.RegulatoryCalendarHistoryEntry AS h
    WHERE h.RegulatoryCalendarId = e.RegulatoryCalendarId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Operational.vw_Alert_WithLatestHistory
-- Purpose:       Active Alert record joined to its single most-recent
--                AlertHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Operational.vw_Alert_WithLatestHistory
AS
SELECT
    e.AlertId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Operational.Alert AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Operational.AlertHistoryEntry AS h
    WHERE h.AlertId = e.AlertId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Operational.vw_Checklists_WithLatestHistory
-- Purpose:       Active Checklists record joined to its single most-recent
--                ChecklistsHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Operational.vw_Checklists_WithLatestHistory
AS
SELECT
    e.ChecklistsId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Operational.Checklists AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Operational.ChecklistsHistoryEntry AS h
    WHERE h.ChecklistsId = e.ChecklistsId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Operational.vw_Contractor_WithLatestHistory
-- Purpose:       Active Contractor record joined to its single most-recent
--                ContractorHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Operational.vw_Contractor_WithLatestHistory
AS
SELECT
    e.ContractorId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Operational.Contractor AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Operational.ContractorHistoryEntry AS h
    WHERE h.ContractorId = e.ContractorId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Operational.vw_PPEManagement_WithLatestHistory
-- Purpose:       Active PPEManagement record joined to its single most-recent
--                PPEManagementHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Operational.vw_PPEManagement_WithLatestHistory
AS
SELECT
    e.PPEManagementId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Operational.PPEManagement AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Operational.PPEManagementHistoryEntry AS h
    WHERE h.PPEManagementId = e.PPEManagementId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Operational.vw_RiskManagement_WithLatestHistory
-- Purpose:       Active RiskManagement record joined to its single most-recent
--                RiskManagementHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Operational.vw_RiskManagement_WithLatestHistory
AS
SELECT
    e.RiskManagementId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Operational.RiskManagement AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Operational.RiskManagementHistoryEntry AS h
    WHERE h.RiskManagementId = e.RiskManagementId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Operational.vw_Schedules_WithLatestHistory
-- Purpose:       Active Schedules record joined to its single most-recent
--                SchedulesHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Operational.vw_Schedules_WithLatestHistory
AS
SELECT
    e.SchedulesId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Operational.Schedules AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Operational.SchedulesHistoryEntry AS h
    WHERE h.SchedulesId = e.SchedulesId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Operational.vw_Training_WithLatestHistory
-- Purpose:       Active Training record joined to its single most-recent
--                TrainingHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW Operational.vw_Training_WithLatestHistory
AS
SELECT
    e.TrainingId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM Operational.Training AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM Operational.TrainingHistoryEntry AS h
    WHERE h.TrainingId = e.TrainingId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          SafetyEvents.vw_EmergencyManagement_WithLatestHistory
-- Purpose:       Active EmergencyManagement record joined to its single most-recent
--                EmergencyManagementHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW SafetyEvents.vw_EmergencyManagement_WithLatestHistory
AS
SELECT
    e.EmergencyManagementId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM SafetyEvents.EmergencyManagement AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM SafetyEvents.EmergencyManagementHistoryEntry AS h
    WHERE h.EmergencyManagementId = e.EmergencyManagementId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          SafetyEvents.vw_IncidentManagement_WithLatestHistory
-- Purpose:       Active IncidentManagement record joined to its single most-recent
--                IncidentManagementHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW SafetyEvents.vw_IncidentManagement_WithLatestHistory
AS
SELECT
    e.IncidentManagementId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM SafetyEvents.IncidentManagement AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM SafetyEvents.IncidentManagementHistoryEntry AS h
    WHERE h.IncidentManagementId = e.IncidentManagementId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          SafetyEvents.vw_Observation_WithLatestHistory
-- Purpose:       Active Observation record joined to its single most-recent
--                ObservationHistoryEntry row (latest status transition), via
--                CROSS APPLY TOP(1)...ORDER BY ChangedAtUtc DESC. Explicit
--                column list, soft-delete filtered on the base entity,
--                module-scoped (single schema, real FK-backed join).
-- =============================================
CREATE OR ALTER VIEW SafetyEvents.vw_Observation_WithLatestHistory
AS
SELECT
    e.ObservationId, e.Status AS CurrentStatus, e.TenantId, e.CreatedUtc, e.CreatedBy,
    e.UpdatedUtc, e.UpdatedBy, e.IsDeleted,
    h.HistoryId AS LatestHistoryId, h.PreviousStatus AS LatestPreviousStatus,
    h.NewStatus AS LatestNewStatus, h.ChangedAtUtc AS LatestChangedAtUtc,
    h.ChangedBy AS LatestChangedBy, h.Notes AS LatestChangeNotes
FROM SafetyEvents.Observation AS e
OUTER APPLY (
    SELECT TOP (1) h.HistoryId, h.PreviousStatus, h.NewStatus, h.ChangedAtUtc, h.ChangedBy, h.Notes
    FROM SafetyEvents.ObservationHistoryEntry AS h
    WHERE h.ObservationId = e.ObservationId
    ORDER BY h.ChangedAtUtc DESC, h.HistoryId DESC
) AS h
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- ===================== SECTION 3: Materialized summary tables + refresh procedures =====================
-- Rationale: of the 35 base CoreDB tables, three carry a genuine time-series/status-rollup
-- reporting need that a plain view cannot serve efficiently once volume grows (dashboards
-- aggregating by day/site/severity/status across many rows): SafetyEvents.IncidentManagement
-- (incident counts by severity/status -- explicitly named as the canonical example in scope),
-- Operational.RiskManagement (risk register counts by risk band, using the existing
-- Operational.fn_RiskManagement_RiskScore function so the banding matches the Domain layer
-- exactly), and Operational.Alert (operational alert-volume trend by type/severity/status).
-- No other CoreDB table showed an aggregation-worthy access pattern distinct from its
-- vw_<Entity>_Active view -- see the final report for the full recommend-optional-summaries list.

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = 'SafetyEvents' AND t.name = 'IncidentManagementSummary')
BEGIN
    CREATE TABLE SafetyEvents.IncidentManagementSummary
    (
        IncidentManagementSummaryId uniqueidentifier NOT NULL CONSTRAINT DF_IncidentManagementSummary_Id DEFAULT NEWSEQUENTIALID(),
        SummaryDateKey              int              NOT NULL,
        TenantId                    uniqueidentifier NOT NULL,
        SiteId                      uniqueidentifier NULL,
        Severity                    nvarchar(50)     NOT NULL,
        Status                      nvarchar(50)     NOT NULL,
        IncidentCount                int              NOT NULL,
        CreatedUtc                  datetime2(7)     NOT NULL CONSTRAINT DF_IncidentManagementSummary_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy                   uniqueidentifier NOT NULL,
        IsDeleted                   bit              NOT NULL CONSTRAINT DF_IncidentManagementSummary_IsDeleted DEFAULT (0),
        CONSTRAINT PK_IncidentManagementSummary PRIMARY KEY CLUSTERED (IncidentManagementSummaryId)
    );
    -- Not UNIQUE: SQL Server disallows expressions (ISNULL) in an index key list, and
    -- SiteId is nullable (unique indexes only tolerate a single NULL per key otherwise).
    -- The refresh procedure's full DELETE+INSERT already guarantees no duplicate groups;
    -- this index exists purely for query performance on the natural lookup columns.
    CREATE NONCLUSTERED INDEX IX_IncidentManagementSummary_Key
        ON SafetyEvents.IncidentManagementSummary (SummaryDateKey, TenantId, SiteId, Severity, Status);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = 'Operational' AND t.name = 'RiskManagementSummary')
BEGIN
    CREATE TABLE Operational.RiskManagementSummary
    (
        RiskManagementSummaryId uniqueidentifier NOT NULL CONSTRAINT DF_RiskManagementSummary_Id DEFAULT NEWSEQUENTIALID(),
        SummaryDateKey           int              NOT NULL,
        TenantId                 uniqueidentifier NOT NULL,
        SiteId                   uniqueidentifier NULL,
        RiskCategory             nvarchar(100)    NOT NULL,
        RiskBand                 nvarchar(20)     NOT NULL, -- Low(1-5) / Medium(6-14) / High(15-25), derived from fn_RiskManagement_RiskScore
        RiskCount                 int              NOT NULL,
        CreatedUtc               datetime2(7)     NOT NULL CONSTRAINT DF_RiskManagementSummary_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy                uniqueidentifier NOT NULL,
        IsDeleted                bit              NOT NULL CONSTRAINT DF_RiskManagementSummary_IsDeleted DEFAULT (0),
        CONSTRAINT PK_RiskManagementSummary PRIMARY KEY CLUSTERED (RiskManagementSummaryId)
    );
    -- Not UNIQUE: SQL Server disallows expressions (ISNULL) in an index key list, and
    -- SiteId is nullable. The refresh procedure's full DELETE+INSERT already guarantees
    -- no duplicate groups; this index exists purely for query performance.
    CREATE NONCLUSTERED INDEX IX_RiskManagementSummary_Key
        ON Operational.RiskManagementSummary (SummaryDateKey, TenantId, SiteId, RiskCategory, RiskBand);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = 'Operational' AND t.name = 'AlertSummary')
BEGIN
    CREATE TABLE Operational.AlertSummary
    (
        AlertSummaryId uniqueidentifier NOT NULL CONSTRAINT DF_AlertSummary_Id DEFAULT NEWSEQUENTIALID(),
        SummaryDateKey  int              NOT NULL,
        TenantId        uniqueidentifier NOT NULL,
        AlertType       nvarchar(100)    NOT NULL,
        Severity        nvarchar(50)     NOT NULL,
        Status          nvarchar(50)     NOT NULL,
        AlertCount       int              NOT NULL,
        CreatedUtc      datetime2(7)     NOT NULL CONSTRAINT DF_AlertSummary_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy       uniqueidentifier NOT NULL,
        IsDeleted       bit              NOT NULL CONSTRAINT DF_AlertSummary_IsDeleted DEFAULT (0),
        CONSTRAINT PK_AlertSummary PRIMARY KEY CLUSTERED (AlertSummaryId)
    );
    -- Not UNIQUE, for consistency with the sibling summary tables above; the refresh
    -- procedure's full DELETE+INSERT guarantees no duplicate groups.
    CREATE NONCLUSTERED INDEX IX_AlertSummary_Key
        ON Operational.AlertSummary (SummaryDateKey, TenantId, AlertType, Severity, Status);
END
GO

-- =============================================
-- Procedure:     SafetyEvents.sp_IncidentManagementSummary_Refresh
-- Description:   Full-rebuild refresh of SafetyEvents.IncidentManagementSummary from
--                SafetyEvents.IncidentManagement, grouped by created-date key, tenant,
--                site, severity and status. Full rebuild pattern (small/medium table,
--                simplest to reason about deterministically; matches the 3 documented
--                refresh patterns in the master doc).
-- =============================================
CREATE OR ALTER PROCEDURE SafetyEvents.sp_IncidentManagementSummary_Refresh
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM SafetyEvents.IncidentManagementSummary;

        INSERT INTO SafetyEvents.IncidentManagementSummary
            (SummaryDateKey, TenantId, SiteId, Severity, Status, IncidentCount, CreatedUtc, CreatedBy)
        SELECT
            CONVERT(int, CONVERT(char(8), e.CreatedUtc, 112)) AS SummaryDateKey,
            e.TenantId,
            e.SiteId,
            e.Severity,
            e.Status,
            COUNT(*) AS IncidentCount,
            SYSUTCDATETIME(),
            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)
        FROM SafetyEvents.IncidentManagement AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        GROUP BY
            CONVERT(int, CONVERT(char(8), e.CreatedUtc, 112)),
            e.TenantId, e.SiteId, e.Severity, e.Status;

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IncidentManagementSummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
                0, N'Full rebuild via sp_IncidentManagementSummary_Refresh',
                0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block the refresh
        END CATCH;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorContext nvarchar(200) = CONCAT(N'Proc=SafetyEvents.sp_IncidentManagementSummary_Refresh; User=', SYSTEM_USER);
        DECLARE @ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID)
        );

        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
            TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
            BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
        )
        VALUES (
            N'IncidentManagementSummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
            NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
        );

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     Operational.sp_RiskManagementSummary_Refresh
-- Description:   Full-rebuild refresh of Operational.RiskManagementSummary from
--                Operational.RiskManagement, banding each row's risk score (via the
--                existing Operational.fn_RiskManagement_RiskScore function) into
--                Low(1-5)/Medium(6-14)/High(15-25) before grouping.
-- =============================================
CREATE OR ALTER PROCEDURE Operational.sp_RiskManagementSummary_Refresh
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM Operational.RiskManagementSummary;

        INSERT INTO Operational.RiskManagementSummary
            (SummaryDateKey, TenantId, SiteId, RiskCategory, RiskBand, RiskCount, CreatedUtc, CreatedBy)
        SELECT
            CONVERT(int, CONVERT(char(8), e.CreatedUtc, 112)) AS SummaryDateKey,
            e.TenantId,
            e.SiteId,
            e.RiskCategory,
            CASE
                WHEN Operational.fn_RiskManagement_RiskScore(e.LikelihoodRating, e.SeverityRating) <= 5  THEN N'Low'
                WHEN Operational.fn_RiskManagement_RiskScore(e.LikelihoodRating, e.SeverityRating) <= 14 THEN N'Medium'
                ELSE N'High'
            END AS RiskBand,
            COUNT(*) AS RiskCount,
            SYSUTCDATETIME(),
            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)
        FROM Operational.RiskManagement AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        GROUP BY
            CONVERT(int, CONVERT(char(8), e.CreatedUtc, 112)),
            e.TenantId, e.SiteId, e.RiskCategory,
            CASE
                WHEN Operational.fn_RiskManagement_RiskScore(e.LikelihoodRating, e.SeverityRating) <= 5  THEN N'Low'
                WHEN Operational.fn_RiskManagement_RiskScore(e.LikelihoodRating, e.SeverityRating) <= 14 THEN N'Medium'
                ELSE N'High'
            END;

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RiskManagementSummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
                0, N'Full rebuild via sp_RiskManagementSummary_Refresh',
                0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorContext nvarchar(200) = CONCAT(N'Proc=Operational.sp_RiskManagementSummary_Refresh; User=', SYSTEM_USER);
        DECLARE @ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID)
        );

        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
            TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
            BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
        )
        VALUES (
            N'RiskManagementSummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
            NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
        );

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     Operational.sp_AlertSummary_Refresh
-- Description:   Full-rebuild refresh of Operational.AlertSummary from Operational.Alert,
--                grouped by created-date key, tenant, alert type, severity and status.
-- =============================================
CREATE OR ALTER PROCEDURE Operational.sp_AlertSummary_Refresh
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM Operational.AlertSummary;

        INSERT INTO Operational.AlertSummary
            (SummaryDateKey, TenantId, AlertType, Severity, Status, AlertCount, CreatedUtc, CreatedBy)
        SELECT
            CONVERT(int, CONVERT(char(8), e.CreatedUtc, 112)) AS SummaryDateKey,
            e.TenantId,
            e.AlertType,
            e.Severity,
            e.Status,
            COUNT(*) AS AlertCount,
            SYSUTCDATETIME(),
            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)
        FROM Operational.Alert AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        GROUP BY
            CONVERT(int, CONVERT(char(8), e.CreatedUtc, 112)),
            e.TenantId, e.AlertType, e.Severity, e.Status;

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AlertSummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
                0, N'Full rebuild via sp_AlertSummary_Refresh',
                0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorContext nvarchar(200) = CONCAT(N'Proc=Operational.sp_AlertSummary_Refresh; User=', SYSTEM_USER);
        DECLARE @ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID)
        );

        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
            TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
            BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
        )
        VALUES (
            N'AlertSummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
            NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
        );

        THROW;
    END CATCH
END
GO
