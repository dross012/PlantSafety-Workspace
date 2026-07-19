-- =============================================
-- Migration:     72-data-classification.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Description:   SQL Server native sensitivity classification
--                (ADD SENSITIVITY CLASSIFICATION, SQL 2019+) for genuinely
--                sensitive personal-data / health-data columns.
--                Governance remediation item #2.
--
-- Scope covered: the 5 fields named in the prior audit, PLUS additional
-- fields this agent independently identified during a full re-scan of all
-- 8 in-scope databases (see report). Columns considered but deliberately
-- NOT classified are listed at the bottom with rationale.
--
-- Taxonomy used: LABEL = 'Confidential - GDPR' for personal-data narrative
-- fields tied to an identifiable worker/incident; INFORMATION_TYPE =
-- 'Health' for physically/medically health-related fields (injury type,
-- well-being risk level), 'Other' for general personal-data narrative text
-- that isn't specifically health data (incident description/root cause/
-- corrective actions/notes, which frequently reference named individuals
-- and circumstances). RANK = HIGH for all, given these are incident/injury
-- narrative fields with real re-identification risk.
--
-- Idempotency: DROP SENSITIVITY CLASSIFICATION IF EXISTS guard before each
-- ADD, so this script is safe to re-run.
-- =============================================
SET NOCOUNT ON;
GO

USE [PlantSafety.CoreDB];
GO

------------------------------------------------------------
-- Originally-flagged fields (prior audit)
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('SafetyEvents.IncidentManagement') AND minor_id = COLUMNPROPERTY(OBJECT_ID('SafetyEvents.IncidentManagement'), 'Description', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM SafetyEvents.IncidentManagement.Description;
ADD SENSITIVITY CLASSIFICATION TO
    SafetyEvents.IncidentManagement.Description
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Other', RANK = HIGH);
GO

IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('SafetyEvents.IncidentManagement') AND minor_id = COLUMNPROPERTY(OBJECT_ID('SafetyEvents.IncidentManagement'), 'RootCause', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM SafetyEvents.IncidentManagement.RootCause;
ADD SENSITIVITY CLASSIFICATION TO
    SafetyEvents.IncidentManagement.RootCause
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Other', RANK = HIGH);
GO

IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('SafetyEvents.IncidentManagement') AND minor_id = COLUMNPROPERTY(OBJECT_ID('SafetyEvents.IncidentManagement'), 'CorrectiveActions', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM SafetyEvents.IncidentManagement.CorrectiveActions;
ADD SENSITIVITY CLASSIFICATION TO
    SafetyEvents.IncidentManagement.CorrectiveActions
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Other', RANK = HIGH);
GO

IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('SafetyEvents.ObservationHistoryEntry') AND minor_id = COLUMNPROPERTY(OBJECT_ID('SafetyEvents.ObservationHistoryEntry'), 'Notes', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM SafetyEvents.ObservationHistoryEntry.Notes;
ADD SENSITIVITY CLASSIFICATION TO
    SafetyEvents.ObservationHistoryEntry.Notes
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Other', RANK = HIGH);
GO

IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('SafetyEvents.EmergencyManagementHistoryEntry') AND minor_id = COLUMNPROPERTY(OBJECT_ID('SafetyEvents.EmergencyManagementHistoryEntry'), 'Notes', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM SafetyEvents.EmergencyManagementHistoryEntry.Notes;
ADD SENSITIVITY CLASSIFICATION TO
    SafetyEvents.EmergencyManagementHistoryEntry.Notes
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Other', RANK = HIGH);
GO

------------------------------------------------------------
-- Additional fields found during this agent's independent re-scan
------------------------------------------------------------

-- InjuryType is genuine health data (not just narrative) - classify Health.
IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('SafetyEvents.IncidentManagement') AND minor_id = COLUMNPROPERTY(OBJECT_ID('SafetyEvents.IncidentManagement'), 'InjuryType', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM SafetyEvents.IncidentManagement.InjuryType;
ADD SENSITIVITY CLASSIFICATION TO
    SafetyEvents.IncidentManagement.InjuryType
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Health', RANK = HIGH);
GO

-- Observation.Description mirrors ObservationHistoryEntry.Notes - the
-- current-state field was missed by the prior audit which only caught the
-- history/audit-trail table.
IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('SafetyEvents.Observation') AND minor_id = COLUMNPROPERTY(OBJECT_ID('SafetyEvents.Observation'), 'Description', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM SafetyEvents.Observation.Description;
ADD SENSITIVITY CLASSIFICATION TO
    SafetyEvents.Observation.Description
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Other', RANK = HIGH);
GO

-- IncidentManagementHistoryEntry carries its own Notes column plus a
-- point-in-time snapshot of RootCause - both missed by the prior audit
-- (which only classified the live IncidentManagement.RootCause column).
IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('SafetyEvents.IncidentManagementHistoryEntry') AND minor_id = COLUMNPROPERTY(OBJECT_ID('SafetyEvents.IncidentManagementHistoryEntry'), 'Notes', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM SafetyEvents.IncidentManagementHistoryEntry.Notes;
ADD SENSITIVITY CLASSIFICATION TO
    SafetyEvents.IncidentManagementHistoryEntry.Notes
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Other', RANK = HIGH);
GO

IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('SafetyEvents.IncidentManagementHistoryEntry') AND minor_id = COLUMNPROPERTY(OBJECT_ID('SafetyEvents.IncidentManagementHistoryEntry'), 'RootCauseAtTransition', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM SafetyEvents.IncidentManagementHistoryEntry.RootCauseAtTransition;
ADD SENSITIVITY CLASSIFICATION TO
    SafetyEvents.IncidentManagementHistoryEntry.RootCauseAtTransition
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Other', RANK = HIGH);
GO

-- WorkerWellBeing.WellBeingRiskLevel is a per-worker health/well-being risk
-- rating - genuine health data, entirely missed by the prior audit (a
-- whole module/table not previously scanned).
IF EXISTS (SELECT 1 FROM sys.sensitivity_classifications
           WHERE major_id = OBJECT_ID('WorkerWellBeing.WorkerWellBeing') AND minor_id = COLUMNPROPERTY(OBJECT_ID('WorkerWellBeing.WorkerWellBeing'), 'WellBeingRiskLevel', 'ColumnId'))
    DROP SENSITIVITY CLASSIFICATION FROM WorkerWellBeing.WorkerWellBeing.WellBeingRiskLevel;
ADD SENSITIVITY CLASSIFICATION TO
    WorkerWellBeing.WorkerWellBeing.WellBeingRiskLevel
WITH (LABEL = 'Confidential - GDPR', INFORMATION_TYPE = 'Health', RANK = HIGH);
GO

------------------------------------------------------------
-- Columns considered during the re-scan but deliberately NOT classified,
-- with rationale (documented per the No-Guessing Rule - flag rather than
-- silently decide either way):
--   * All `*WorkerId` FK columns (Analytics.Engagement.WorkerId,
--     Compliance.Certification.HolderWorkerId, Operational.PPEManagement.
--     WorkerId, Operational.Training.WorkerId, WorkerWellBeing.
--     WorkerWellBeing.WorkerId, SafetyEvents.IncidentManagement.
--     ReportedByWorkerId) are opaque uniqueidentifier surrogate keys with
--     no human-readable content. SQL Data Classification targets
--     human-readable PII/health content; these are pseudonymous foreign
--     keys. NEEDS HUMAN CONFIRMATION: if GDPR subject-access tooling needs
--     these tagged as "linkage to a data subject" for discovery purposes,
--     that is a policy decision, not a mechanical one - flagging rather
--     than guessing.
--   * Operational.Contractor.ContractorName / PrimaryContactId - business
--     entity name and a contact-person FK, not a worker's own personal
--     data; left unclassified as out of the audited scope (contractor
--     company data, not employee/worker health-and-safety narrative data).
--   * Operational.RiskManagement.HazardDescription - describes a
--     hazard/process/location, not an identifiable person; not personal
--     data under GDPR Art. 4, left unclassified.
--   * General `*.Notes` fields outside SafetyEvents/WorkerWellBeing
--     (Analytics.*, Compliance.*, Operational.Alert/Checklists/Schedules/
--     Training HistoryEntry.Notes, etc.) - these are operational/workflow
--     notes, not incident/injury narrative data. Re-scanned and judged
--     out of the genuinely-sensitive category; NOT classified. If any of
--     these in practice end up containing worker-identifying health
--     narrative text, that would need a follow-up, human-directed pass.
------------------------------------------------------------

------------------------------------------------------------
-- Verification
------------------------------------------------------------
SELECT
    OBJECT_SCHEMA_NAME(c.major_id) + '.' + OBJECT_NAME(c.major_id) AS TableName,
    col.name AS ColumnName,
    c.label,
    c.information_type,
    c.rank_desc
FROM sys.sensitivity_classifications c
JOIN sys.columns col ON col.object_id = c.major_id AND col.column_id = c.minor_id
ORDER BY TableName, ColumnName;
GO

