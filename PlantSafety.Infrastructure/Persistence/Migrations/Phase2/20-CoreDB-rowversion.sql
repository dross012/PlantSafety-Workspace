USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 2 Durability/Concurrency Remediation
-- Script:        20-CoreDB-rowversion.sql
-- Purpose:       Add ConcurrencyToken ROWVERSION NOT NULL to every update-capable
--                (mutable aggregate-root) table in PlantSafety.CoreDB.
-- Selection:     Tables with an UpdatedUtc/UpdatedBy pair (mutable signal per
--                Schema Sentinel rule: create-table.md / align-schema.md).
--                Append-only tables (HistoryEntry, LogEntry, Archive, Error,
--                ReplayRecord, Snapshot-of-fact tables, etc.) are intentionally
--                skipped -- see companion report.
-- Idempotent:    Guarded by COL_LENGTH() check; safe to re-run.
-- =============================================

IF COL_LENGTH('[AiGovernance].[AiGovernance]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [AiGovernance].[AiGovernance] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[AiSafetyIntelligence].[AiSafetyIntelligence]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [AiSafetyIntelligence].[AiSafetyIntelligence] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Analytics].[Culture]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Analytics].[Culture] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Analytics].[Dashboard]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Analytics].[Dashboard] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Analytics].[Engagement]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Analytics].[Engagement] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Analytics].[PredictiveAnalytics]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Analytics].[PredictiveAnalytics] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Analytics].[ScoreCards]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Analytics].[ScoreCards] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Compliance].[Auditing]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Compliance].[Auditing] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Compliance].[Certification]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Compliance].[Certification] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Compliance].[Compliance]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Compliance].[Compliance] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Compliance].[ManagementOfChange]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Compliance].[ManagementOfChange] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Compliance].[Permits]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Compliance].[Permits] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Compliance].[Policy]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Compliance].[Policy] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Compliance].[Procedures]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Compliance].[Procedures] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Compliance].[RegulatoryCalendar]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Compliance].[RegulatoryCalendar] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[ContractorVisitor].[ContractorVisitor]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [ContractorVisitor].[ContractorVisitor] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[DigitalTwin].[DigitalTwin]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [DigitalTwin].[DigitalTwin] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[EnterpriseFabric].[EnterpriseFabric]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [EnterpriseFabric].[EnterpriseFabric] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[EnvironmentalHealth].[EnvironmentalHealth]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [EnvironmentalHealth].[EnvironmentalHealth] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Governance].[Governance]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Governance].[Governance] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[HumanFactors].[HumanFactors]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [HumanFactors].[HumanFactors] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Operational].[Alert]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Operational].[Alert] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Operational].[Checklists]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Operational].[Checklists] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Operational].[Contractor]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Operational].[Contractor] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Operational].[PPEManagement]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Operational].[PPEManagement] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Operational].[RiskManagement]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Operational].[RiskManagement] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Operational].[Schedules]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Operational].[Schedules] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Operational].[Training]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Operational].[Training] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[OperationalSafety].[OperationalSafety]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [OperationalSafety].[OperationalSafety] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[ProcessManagement].[ProcessDefinition]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [ProcessManagement].[ProcessDefinition] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[SafetyEvents].[EmergencyManagement]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [SafetyEvents].[EmergencyManagement] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[SafetyEvents].[IncidentManagement]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [SafetyEvents].[IncidentManagement] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[SafetyEvents].[Observation]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [SafetyEvents].[Observation] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Sustainability].[Sustainability]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Sustainability].[Sustainability] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[WorkerWellBeing].[WorkerWellBeing]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [WorkerWellBeing].[WorkerWellBeing] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO
