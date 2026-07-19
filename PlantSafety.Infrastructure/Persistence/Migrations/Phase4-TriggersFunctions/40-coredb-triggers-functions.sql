USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 4 Triggers & Functions
-- Script:        40-coredb-triggers-functions.sql
-- Author:        Trigger & Function Engineer (unattended agent run)
-- Purpose:       Deterministic AFTER triggers + scalar/inline-TVF functions for
--                PlantSafety.CoreDB, grounded in the Phase 2 ConcurrencyToken
--                inventory and the RiskManagement.cs domain formula.
--
-- SECTION 1 -- Update-audit triggers
--   For every mutable table carrying ConcurrencyToken + UpdatedUtc/UpdatedBy
--   (30 tables; append-only *HistoryEntry tables intentionally excluded, they
--   carry no audit-column pair and are never updated), an AFTER UPDATE trigger
--   forcibly re-stamps UpdatedUtc = SYSUTCDATETIME() on every update, closing
--   the real UPDATE-time gap that a column DEFAULT cannot cover (DEFAULT only
--   fires on INSERT). UpdatedBy is intentionally left untouched -- see the
--   comment inside each trigger body. ConcurrencyToken itself needs no trigger;
--   SQL Server maintains ROWVERSION automatically on every UPDATE.
--
-- SECTION 2 -- Soft-delete triggers: DELIBERATELY NOT CREATED
--   The enterprise_data_database_architecture_master.md rule set (mirrored in
--   the sql-database-skill catalog) states triggers must be "AFTER only
--   (never INSTEAD OF)" and restricted to "audit/error/summary-refresh
--   orchestration only, never business logic". Converting a hard DELETE into
--   a soft-delete UPDATE (IsDeleted = 1) requires an INSTEAD OF DELETE
--   trigger, which this architecture explicitly forbids, and the conversion
--   itself is business logic, not audit orchestration. Soft-delete enforcement
--   in this codebase is therefore an application/SP-layer responsibility
--   (never issue hard DELETE from the API/SP layer), not a trigger
--   responsibility. No INSTEAD OF DELETE triggers are created here.
--
-- SECTION 3 -- Functions
--   dbo.fn_IsWithinTenant           -- scalar, deterministic tenant-scoping helper
--                                      (every CoreDB table carries TenantId; this
--                                      is genuinely reusable across all 16 schemas).
--   Operational.fn_RiskManagement_RiskScore
--                                   -- scalar, replicates the exact
--                                      LikelihoodRating * SeverityRating formula
--                                      (each factor clamped 1-5) from
--                                      PlantSafety.Domain.Operational.RiskManagement
--                                      .Aggregates.RiskManagement.cs (RiskScore
--                                      computed property), exposed for SQL-side
--                                      reporting/SP use without duplicating the
--                                      business rule inconsistently.
--   Inline TVFs (fn_<Entity>_Active) for the safety-critical, highest-traffic
--   CoreDB tables (RiskManagement, IncidentManagement, EmergencyManagement,
--   Observation, Alert) -- tenant-scoped, non-deleted, explicit column list,
--   no business logic beyond the RiskManagement RiskScore projection.
--   The remaining 25 CoreDB tables are NOT given an fn_<Entity>_Active TVF --
--   no distinct reusable logic beyond "WHERE TenantId=@t AND IsDeleted=0" was
--   found to justify one over the existing vw_<Entity>_<Purpose> view pattern;
--   see recommend-optional-functions notes in the final report for the
--   candidate list if the View & Summary Builder / Procedure Architect agents
--   want them added later.
--
-- Idempotent:    CREATE OR ALTER throughout (SQL Server 2025 / compat >=150).
-- =============================================

-- ===================== SECTION 1: Update-audit triggers =====================

CREATE OR ALTER TRIGGER [AiGovernance].[trg_AiGovernance_UpdateAudit]
ON [AiGovernance].[AiGovernance]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [AiGovernance].[AiGovernance] AS t
    INNER JOIN inserted AS i
        ON t.[AiGovernanceId] = i.[AiGovernanceId];
END
GO

CREATE OR ALTER TRIGGER [AiSafetyIntelligence].[trg_AiSafetyIntelligence_UpdateAudit]
ON [AiSafetyIntelligence].[AiSafetyIntelligence]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [AiSafetyIntelligence].[AiSafetyIntelligence] AS t
    INNER JOIN inserted AS i
        ON t.[AiSafetyIntelligenceId] = i.[AiSafetyIntelligenceId];
END
GO

CREATE OR ALTER TRIGGER [Analytics].[trg_Culture_UpdateAudit]
ON [Analytics].[Culture]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Analytics].[Culture] AS t
    INNER JOIN inserted AS i
        ON t.[CultureId] = i.[CultureId];
END
GO

CREATE OR ALTER TRIGGER [Analytics].[trg_Dashboard_UpdateAudit]
ON [Analytics].[Dashboard]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Analytics].[Dashboard] AS t
    INNER JOIN inserted AS i
        ON t.[DashboardId] = i.[DashboardId];
END
GO

CREATE OR ALTER TRIGGER [Analytics].[trg_Engagement_UpdateAudit]
ON [Analytics].[Engagement]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Analytics].[Engagement] AS t
    INNER JOIN inserted AS i
        ON t.[EngagementId] = i.[EngagementId];
END
GO

CREATE OR ALTER TRIGGER [Analytics].[trg_PredictiveAnalytics_UpdateAudit]
ON [Analytics].[PredictiveAnalytics]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Analytics].[PredictiveAnalytics] AS t
    INNER JOIN inserted AS i
        ON t.[PredictiveAnalyticsId] = i.[PredictiveAnalyticsId];
END
GO

CREATE OR ALTER TRIGGER [Analytics].[trg_ScoreCards_UpdateAudit]
ON [Analytics].[ScoreCards]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Analytics].[ScoreCards] AS t
    INNER JOIN inserted AS i
        ON t.[ScoreCardsId] = i.[ScoreCardsId];
END
GO

CREATE OR ALTER TRIGGER [Compliance].[trg_Auditing_UpdateAudit]
ON [Compliance].[Auditing]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Compliance].[Auditing] AS t
    INNER JOIN inserted AS i
        ON t.[AuditingId] = i.[AuditingId];
END
GO

CREATE OR ALTER TRIGGER [Compliance].[trg_Certification_UpdateAudit]
ON [Compliance].[Certification]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Compliance].[Certification] AS t
    INNER JOIN inserted AS i
        ON t.[CertificationId] = i.[CertificationId];
END
GO

CREATE OR ALTER TRIGGER [Compliance].[trg_Compliance_UpdateAudit]
ON [Compliance].[Compliance]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Compliance].[Compliance] AS t
    INNER JOIN inserted AS i
        ON t.[ComplianceId] = i.[ComplianceId];
END
GO

CREATE OR ALTER TRIGGER [Compliance].[trg_ManagementOfChange_UpdateAudit]
ON [Compliance].[ManagementOfChange]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Compliance].[ManagementOfChange] AS t
    INNER JOIN inserted AS i
        ON t.[ManagementOfChangeId] = i.[ManagementOfChangeId];
END
GO

CREATE OR ALTER TRIGGER [Compliance].[trg_Permits_UpdateAudit]
ON [Compliance].[Permits]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Compliance].[Permits] AS t
    INNER JOIN inserted AS i
        ON t.[PermitsId] = i.[PermitsId];
END
GO

CREATE OR ALTER TRIGGER [Compliance].[trg_Policy_UpdateAudit]
ON [Compliance].[Policy]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Compliance].[Policy] AS t
    INNER JOIN inserted AS i
        ON t.[PolicyId] = i.[PolicyId];
END
GO

CREATE OR ALTER TRIGGER [Compliance].[trg_Procedures_UpdateAudit]
ON [Compliance].[Procedures]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Compliance].[Procedures] AS t
    INNER JOIN inserted AS i
        ON t.[ProceduresId] = i.[ProceduresId];
END
GO

CREATE OR ALTER TRIGGER [Compliance].[trg_RegulatoryCalendar_UpdateAudit]
ON [Compliance].[RegulatoryCalendar]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Compliance].[RegulatoryCalendar] AS t
    INNER JOIN inserted AS i
        ON t.[RegulatoryCalendarId] = i.[RegulatoryCalendarId];
END
GO

CREATE OR ALTER TRIGGER [ContractorVisitor].[trg_ContractorVisitor_UpdateAudit]
ON [ContractorVisitor].[ContractorVisitor]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [ContractorVisitor].[ContractorVisitor] AS t
    INNER JOIN inserted AS i
        ON t.[ContractorVisitorId] = i.[ContractorVisitorId];
END
GO

CREATE OR ALTER TRIGGER [DigitalTwin].[trg_DigitalTwin_UpdateAudit]
ON [DigitalTwin].[DigitalTwin]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [DigitalTwin].[DigitalTwin] AS t
    INNER JOIN inserted AS i
        ON t.[DigitalTwinId] = i.[DigitalTwinId];
END
GO

CREATE OR ALTER TRIGGER [EnterpriseFabric].[trg_EnterpriseFabric_UpdateAudit]
ON [EnterpriseFabric].[EnterpriseFabric]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [EnterpriseFabric].[EnterpriseFabric] AS t
    INNER JOIN inserted AS i
        ON t.[EnterpriseFabricId] = i.[EnterpriseFabricId];
END
GO

CREATE OR ALTER TRIGGER [EnvironmentalHealth].[trg_EnvironmentalHealth_UpdateAudit]
ON [EnvironmentalHealth].[EnvironmentalHealth]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [EnvironmentalHealth].[EnvironmentalHealth] AS t
    INNER JOIN inserted AS i
        ON t.[EnvironmentalHealthId] = i.[EnvironmentalHealthId];
END
GO

CREATE OR ALTER TRIGGER [Governance].[trg_Governance_UpdateAudit]
ON [Governance].[Governance]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Governance].[Governance] AS t
    INNER JOIN inserted AS i
        ON t.[GovernanceId] = i.[GovernanceId];
END
GO

CREATE OR ALTER TRIGGER [HumanFactors].[trg_HumanFactors_UpdateAudit]
ON [HumanFactors].[HumanFactors]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [HumanFactors].[HumanFactors] AS t
    INNER JOIN inserted AS i
        ON t.[HumanFactorsId] = i.[HumanFactorsId];
END
GO

CREATE OR ALTER TRIGGER [Operational].[trg_Alert_UpdateAudit]
ON [Operational].[Alert]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Operational].[Alert] AS t
    INNER JOIN inserted AS i
        ON t.[AlertId] = i.[AlertId];
END
GO

CREATE OR ALTER TRIGGER [Operational].[trg_Checklists_UpdateAudit]
ON [Operational].[Checklists]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Operational].[Checklists] AS t
    INNER JOIN inserted AS i
        ON t.[ChecklistsId] = i.[ChecklistsId];
END
GO

CREATE OR ALTER TRIGGER [Operational].[trg_Contractor_UpdateAudit]
ON [Operational].[Contractor]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Operational].[Contractor] AS t
    INNER JOIN inserted AS i
        ON t.[ContractorId] = i.[ContractorId];
END
GO

CREATE OR ALTER TRIGGER [Operational].[trg_PPEManagement_UpdateAudit]
ON [Operational].[PPEManagement]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Operational].[PPEManagement] AS t
    INNER JOIN inserted AS i
        ON t.[PPEManagementId] = i.[PPEManagementId];
END
GO

CREATE OR ALTER TRIGGER [Operational].[trg_RiskManagement_UpdateAudit]
ON [Operational].[RiskManagement]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Operational].[RiskManagement] AS t
    INNER JOIN inserted AS i
        ON t.[RiskManagementId] = i.[RiskManagementId];
END
GO

CREATE OR ALTER TRIGGER [Operational].[trg_Schedules_UpdateAudit]
ON [Operational].[Schedules]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Operational].[Schedules] AS t
    INNER JOIN inserted AS i
        ON t.[SchedulesId] = i.[SchedulesId];
END
GO

CREATE OR ALTER TRIGGER [Operational].[trg_Training_UpdateAudit]
ON [Operational].[Training]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Operational].[Training] AS t
    INNER JOIN inserted AS i
        ON t.[TrainingId] = i.[TrainingId];
END
GO

CREATE OR ALTER TRIGGER [OperationalSafety].[trg_OperationalSafety_UpdateAudit]
ON [OperationalSafety].[OperationalSafety]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [OperationalSafety].[OperationalSafety] AS t
    INNER JOIN inserted AS i
        ON t.[OperationalSafetyId] = i.[OperationalSafetyId];
END
GO

CREATE OR ALTER TRIGGER [ProcessManagement].[trg_ProcessDefinition_UpdateAudit]
ON [ProcessManagement].[ProcessDefinition]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [ProcessManagement].[ProcessDefinition] AS t
    INNER JOIN inserted AS i
        ON t.[ProcessDefinitionId] = i.[ProcessDefinitionId];
END
GO

CREATE OR ALTER TRIGGER [SafetyEvents].[trg_EmergencyManagement_UpdateAudit]
ON [SafetyEvents].[EmergencyManagement]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [SafetyEvents].[EmergencyManagement] AS t
    INNER JOIN inserted AS i
        ON t.[EmergencyManagementId] = i.[EmergencyManagementId];
END
GO

CREATE OR ALTER TRIGGER [SafetyEvents].[trg_IncidentManagement_UpdateAudit]
ON [SafetyEvents].[IncidentManagement]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [SafetyEvents].[IncidentManagement] AS t
    INNER JOIN inserted AS i
        ON t.[IncidentManagementId] = i.[IncidentManagementId];
END
GO

CREATE OR ALTER TRIGGER [SafetyEvents].[trg_Observation_UpdateAudit]
ON [SafetyEvents].[Observation]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [SafetyEvents].[Observation] AS t
    INNER JOIN inserted AS i
        ON t.[ObservationId] = i.[ObservationId];
END
GO

CREATE OR ALTER TRIGGER [Sustainability].[trg_Sustainability_UpdateAudit]
ON [Sustainability].[Sustainability]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [Sustainability].[Sustainability] AS t
    INNER JOIN inserted AS i
        ON t.[SustainabilityId] = i.[SustainabilityId];
END
GO

CREATE OR ALTER TRIGGER [WorkerWellBeing].[trg_WorkerWellBeing_UpdateAudit]
ON [WorkerWellBeing].[WorkerWellBeing]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    -- Force UpdatedUtc to the true SQL Server UTC clock on every UPDATE, regardless
    -- of what the caller supplied. UpdatedBy is intentionally left untouched here --
    -- SYSTEM_USER reflects the DB login (often a shared service account), not the
    -- true application actor, so overwriting it would reduce audit accuracy. The
    -- application/SP layer remains the source of truth for UpdatedBy.
    UPDATE t
        SET t.UpdatedUtc = SYSUTCDATETIME()
    FROM [WorkerWellBeing].[WorkerWellBeing] AS t
    INNER JOIN inserted AS i
        ON t.[WorkerWellBeingId] = i.[WorkerWellBeingId];
END
GO


-- ===================== SECTION 3: Functions =====================

-- =============================================
-- Function:      dbo.fn_IsWithinTenant
-- Purpose:       Deterministic tenant-scoping predicate helper. Every CoreDB
--                table across all 16 schemas carries TenantId; reusable in
--                SP/view WHERE clauses instead of duplicating the NULL-safe
--                comparison expression everywhere.
-- Determinism:   Pure comparison, no table refs, no nondeterministic calls.
-- =============================================
CREATE OR ALTER FUNCTION dbo.fn_IsWithinTenant
(
    @TenantId    uniqueidentifier,
    @RowTenantId uniqueidentifier
)
RETURNS bit
WITH SCHEMABINDING
AS
BEGIN
    RETURN CASE WHEN @TenantId IS NOT NULL AND @RowTenantId IS NOT NULL
                  AND @TenantId = @RowTenantId
                THEN CAST(1 AS bit)
                ELSE CAST(0 AS bit)
           END;
END
GO

-- =============================================
-- Function:      Operational.fn_RiskManagement_RiskScore
-- Purpose:       Replicates PlantSafety.Domain.Operational.RiskManagement
--                .Aggregates.RiskManagement.RiskScore exactly:
--                  RiskScore => LikelihoodRating * SeverityRating
--                with each factor clamped to the domain's [1,5] range
--                (Math.Clamp(likelihoodRating, 1, 5) / Math.Clamp(severityRating, 1, 5)
--                applied in RiskManagement.Create), so SQL-side reporting/SP
--                consumers compute the identical value the Domain layer would.
-- Determinism:   Pure arithmetic, no table refs, no nondeterministic calls.
-- =============================================
CREATE OR ALTER FUNCTION Operational.fn_RiskManagement_RiskScore
(
    @LikelihoodRating int,
    @SeverityRating   int
)
RETURNS int
WITH SCHEMABINDING
AS
BEGIN
    DECLARE @Likelihood int = CASE WHEN @LikelihoodRating < 1 THEN 1
                                    WHEN @LikelihoodRating > 5 THEN 5
                                    ELSE @LikelihoodRating END;
    DECLARE @Severity int = CASE WHEN @SeverityRating < 1 THEN 1
                                  WHEN @SeverityRating > 5 THEN 5
                                  ELSE @SeverityRating END;
    RETURN @Likelihood * @Severity;
END
GO

-- =============================================
-- Function:      Operational.fn_RiskManagement_Active
-- Purpose:       Inline TVF: non-deleted RiskManagement records for a tenant,
--                with RiskScore projected via fn_RiskManagement_RiskScore so
--                callers never have to duplicate the multiplication.
-- Determinism:   Single deterministic SELECT, explicit column list, no
--                cross-module reads (Operational schema only).
-- =============================================
CREATE OR ALTER FUNCTION Operational.fn_RiskManagement_Active
(
    @TenantId uniqueidentifier
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        e.RiskManagementId, e.TenantId, e.SiteId, e.Status,
        e.HazardDescription, e.RiskCategory,
        e.LikelihoodRating, e.SeverityRating,
        Operational.fn_RiskManagement_RiskScore(e.LikelihoodRating, e.SeverityRating) AS RiskScore,
        e.ControlMeasures, e.ResponsiblePartyId, e.ReviewDueAtUtc,
        e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted
    FROM Operational.RiskManagement AS e
    WHERE e.TenantId = @TenantId
      AND ISNULL(e.IsDeleted, 0) = 0
);
GO

-- =============================================
-- Function:      SafetyEvents.fn_IncidentManagement_Active
-- Purpose:       Inline TVF: non-deleted IncidentManagement records for a
--                tenant. Explicit column list, no business logic.
-- =============================================
CREATE OR ALTER FUNCTION SafetyEvents.fn_IncidentManagement_Active
(
    @TenantId uniqueidentifier
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        e.IncidentManagementId, e.TenantId, e.SiteId, e.Status,
        e.Title, e.Description, e.Severity, e.Location,
        e.IncidentOccurredAtUtc, e.InjuryType,
        e.ReportedByWorkerId, e.AssignedInvestigatorId,
        e.RootCause, e.CorrectiveActions, e.ClosedAtUtc,
        e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted
    FROM SafetyEvents.IncidentManagement AS e
    WHERE e.TenantId = @TenantId
      AND ISNULL(e.IsDeleted, 0) = 0
);
GO

-- =============================================
-- Function:      SafetyEvents.fn_EmergencyManagement_Active
-- Purpose:       Inline TVF: non-deleted EmergencyManagement records for a
--                tenant. Explicit column list, no business logic.
-- =============================================
CREATE OR ALTER FUNCTION SafetyEvents.fn_EmergencyManagement_Active
(
    @TenantId uniqueidentifier
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        e.EmergencyManagementId, e.TenantId, e.SiteId, e.Status,
        e.EmergencyType, e.Severity, e.Location, e.CasualtyCount,
        e.ResponseTeamNotified, e.DeclaredAtUtc, e.StabilizedAtUtc, e.ResolvedAtUtc,
        e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted
    FROM SafetyEvents.EmergencyManagement AS e
    WHERE e.TenantId = @TenantId
      AND ISNULL(e.IsDeleted, 0) = 0
);
GO

-- =============================================
-- Function:      SafetyEvents.fn_Observation_Active
-- Purpose:       Inline TVF: non-deleted Observation records for a tenant.
--                Explicit column list, no business logic.
-- =============================================
CREATE OR ALTER FUNCTION SafetyEvents.fn_Observation_Active
(
    @TenantId uniqueidentifier
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        e.ObservationId, e.TenantId, e.SiteId, e.Status,
        e.ObserverId, e.ObservationType, e.HazardCategory, e.Location,
        e.Description, e.CorrectiveActionRequired, e.FollowUpDueAtUtc,
        e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted
    FROM SafetyEvents.Observation AS e
    WHERE e.TenantId = @TenantId
      AND ISNULL(e.IsDeleted, 0) = 0
);
GO

-- =============================================
-- Function:      Operational.fn_Alert_Active
-- Purpose:       Inline TVF: non-deleted Alert records for a tenant.
--                Explicit column list, no business logic.
-- =============================================
CREATE OR ALTER FUNCTION Operational.fn_Alert_Active
(
    @TenantId uniqueidentifier
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        e.AlertId, e.TenantId, e.AlertType, e.Severity, e.Status,
        e.Title, e.Description, e.SourceModule,
        e.AcknowledgedAtUtc, e.InProgressAtUtc, e.EscalatedAtUtc,
        e.ResolvedAtUtc, e.ClosedAtUtc, e.ArchivedAtUtc,
        e.AcknowledgedBy, e.InProgressBy, e.EscalatedBy, e.ResolvedBy,
        e.ClosedBy, e.ArchivedBy, e.EscalationReason, e.ResolutionSummary,
        e.Notes, e.SlaExpiration,
        e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted
    FROM Operational.Alert AS e
    WHERE e.TenantId = @TenantId
      AND ISNULL(e.IsDeleted, 0) = 0
);
GO
