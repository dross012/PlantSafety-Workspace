USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 1 -- Structural Defect Fix
-- Script:        15-coredb-history-snapshot-nullability-fix.sql
-- Purpose:       17 numeric/bit "AtTransition" snapshot columns across 14
--                HistoryEntry tables were created NOT NULL (with a DEFAULT)
--                by 10-coredb-flesh-out-stubs-v2.sql. A transition row that
--                does not touch a given field has no legitimate way to
--                record "not captured" vs. "actually 0/false" while the
--                column is NOT NULL. This script makes all 17 columns
--                nullable and drops their DEFAULT constraints so the app
--                layer must explicitly pass NULL or the real value --
--                no silent 0/false default on omission.
--
-- Grounding:     Every column's live exact data type/precision/scale and
--                its DF_ default-constraint name were read directly from
--                sys.columns / sys.default_constraints on
--                (local)\SQLEXPRESS -> PlantSafety.CoreDB immediately
--                before writing this script -- not assumed, not carried
--                over from the origin migration text.
--
-- Safety:        All 44 CoreDB tables (22 base + 22 HistoryEntry) are at
--                0 rows as of this writing, so this is a pure structural
--                fix with no data-migration risk. Idempotent: each block
--                checks live nullability/default existence before acting,
--                so a rerun is a no-op and produces no errors.
-- =============================================

------------------------------------------------------------
-- Analytics.CultureHistoryEntry.CultureIndexScoreAtTransition  decimal(7,4)
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Analytics.CultureHistoryEntry') AND c.name = 'CultureIndexScoreAtTransition'
)
BEGIN
    DECLARE @dc1 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Analytics.CultureHistoryEntry') AND c.name = 'CultureIndexScoreAtTransition'
    );
    EXEC('ALTER TABLE [Analytics].[CultureHistoryEntry] DROP CONSTRAINT [' + @dc1 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Analytics.CultureHistoryEntry') AND c.name = 'CultureIndexScoreAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Analytics].[CultureHistoryEntry] ALTER COLUMN [CultureIndexScoreAtTransition] decimal(7,4) NULL;
GO

------------------------------------------------------------
-- Analytics.DashboardHistoryEntry.IsPublishedAtTransition  bit
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Analytics.DashboardHistoryEntry') AND c.name = 'IsPublishedAtTransition'
)
BEGIN
    DECLARE @dc2 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Analytics.DashboardHistoryEntry') AND c.name = 'IsPublishedAtTransition'
    );
    EXEC('ALTER TABLE [Analytics].[DashboardHistoryEntry] DROP CONSTRAINT [' + @dc2 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Analytics.DashboardHistoryEntry') AND c.name = 'IsPublishedAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Analytics].[DashboardHistoryEntry] ALTER COLUMN [IsPublishedAtTransition] bit NULL;
GO

------------------------------------------------------------
-- Analytics.EngagementHistoryEntry.EngagementScoreAtTransition  decimal(7,4)
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Analytics.EngagementHistoryEntry') AND c.name = 'EngagementScoreAtTransition'
)
BEGIN
    DECLARE @dc3 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Analytics.EngagementHistoryEntry') AND c.name = 'EngagementScoreAtTransition'
    );
    EXEC('ALTER TABLE [Analytics].[EngagementHistoryEntry] DROP CONSTRAINT [' + @dc3 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Analytics.EngagementHistoryEntry') AND c.name = 'EngagementScoreAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Analytics].[EngagementHistoryEntry] ALTER COLUMN [EngagementScoreAtTransition] decimal(7,4) NULL;
GO

------------------------------------------------------------
-- Analytics.PredictiveAnalyticsHistoryEntry.PredictedRiskScoreAtTransition  decimal(7,4)
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Analytics.PredictiveAnalyticsHistoryEntry') AND c.name = 'PredictedRiskScoreAtTransition'
)
BEGIN
    DECLARE @dc4 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Analytics.PredictiveAnalyticsHistoryEntry') AND c.name = 'PredictedRiskScoreAtTransition'
    );
    EXEC('ALTER TABLE [Analytics].[PredictiveAnalyticsHistoryEntry] DROP CONSTRAINT [' + @dc4 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Analytics.PredictiveAnalyticsHistoryEntry') AND c.name = 'PredictedRiskScoreAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Analytics].[PredictiveAnalyticsHistoryEntry] ALTER COLUMN [PredictedRiskScoreAtTransition] decimal(7,4) NULL;
GO

------------------------------------------------------------
-- Analytics.ScoreCardsHistoryEntry.PerformanceIndexAtTransition  decimal(7,4)
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Analytics.ScoreCardsHistoryEntry') AND c.name = 'PerformanceIndexAtTransition'
)
BEGIN
    DECLARE @dc5 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Analytics.ScoreCardsHistoryEntry') AND c.name = 'PerformanceIndexAtTransition'
    );
    EXEC('ALTER TABLE [Analytics].[ScoreCardsHistoryEntry] DROP CONSTRAINT [' + @dc5 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Analytics.ScoreCardsHistoryEntry') AND c.name = 'PerformanceIndexAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Analytics].[ScoreCardsHistoryEntry] ALTER COLUMN [PerformanceIndexAtTransition] decimal(7,4) NULL;
GO

------------------------------------------------------------
-- Compliance.AuditingHistoryEntry.FindingsCountAtTransition  int
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Compliance.AuditingHistoryEntry') AND c.name = 'FindingsCountAtTransition'
)
BEGIN
    DECLARE @dc6 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Compliance.AuditingHistoryEntry') AND c.name = 'FindingsCountAtTransition'
    );
    EXEC('ALTER TABLE [Compliance].[AuditingHistoryEntry] DROP CONSTRAINT [' + @dc6 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Compliance.AuditingHistoryEntry') AND c.name = 'FindingsCountAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Compliance].[AuditingHistoryEntry] ALTER COLUMN [FindingsCountAtTransition] int NULL;
GO

------------------------------------------------------------
-- Compliance.ComplianceHistoryEntry.ComplianceScoreAtTransition  decimal(7,4)
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Compliance.ComplianceHistoryEntry') AND c.name = 'ComplianceScoreAtTransition'
)
BEGIN
    DECLARE @dc7 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Compliance.ComplianceHistoryEntry') AND c.name = 'ComplianceScoreAtTransition'
    );
    EXEC('ALTER TABLE [Compliance].[ComplianceHistoryEntry] DROP CONSTRAINT [' + @dc7 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Compliance.ComplianceHistoryEntry') AND c.name = 'ComplianceScoreAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Compliance].[ComplianceHistoryEntry] ALTER COLUMN [ComplianceScoreAtTransition] decimal(7,4) NULL;
GO

------------------------------------------------------------
-- Compliance.ComplianceHistoryEntry.NonConformanceCountAtTransition  int
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Compliance.ComplianceHistoryEntry') AND c.name = 'NonConformanceCountAtTransition'
)
BEGIN
    DECLARE @dc8 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Compliance.ComplianceHistoryEntry') AND c.name = 'NonConformanceCountAtTransition'
    );
    EXEC('ALTER TABLE [Compliance].[ComplianceHistoryEntry] DROP CONSTRAINT [' + @dc8 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Compliance.ComplianceHistoryEntry') AND c.name = 'NonConformanceCountAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Compliance].[ComplianceHistoryEntry] ALTER COLUMN [NonConformanceCountAtTransition] int NULL;
GO

------------------------------------------------------------
-- Compliance.PolicyHistoryEntry.AcknowledgmentCountAtTransition  int
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Compliance.PolicyHistoryEntry') AND c.name = 'AcknowledgmentCountAtTransition'
)
BEGIN
    DECLARE @dc9 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Compliance.PolicyHistoryEntry') AND c.name = 'AcknowledgmentCountAtTransition'
    );
    EXEC('ALTER TABLE [Compliance].[PolicyHistoryEntry] DROP CONSTRAINT [' + @dc9 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Compliance.PolicyHistoryEntry') AND c.name = 'AcknowledgmentCountAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Compliance].[PolicyHistoryEntry] ALTER COLUMN [AcknowledgmentCountAtTransition] int NULL;
GO

------------------------------------------------------------
-- Compliance.PolicyHistoryEntry.PolicyVersionAtTransition  int
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Compliance.PolicyHistoryEntry') AND c.name = 'PolicyVersionAtTransition'
)
BEGIN
    DECLARE @dc10 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Compliance.PolicyHistoryEntry') AND c.name = 'PolicyVersionAtTransition'
    );
    EXEC('ALTER TABLE [Compliance].[PolicyHistoryEntry] DROP CONSTRAINT [' + @dc10 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Compliance.PolicyHistoryEntry') AND c.name = 'PolicyVersionAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Compliance].[PolicyHistoryEntry] ALTER COLUMN [PolicyVersionAtTransition] int NULL;
GO

------------------------------------------------------------
-- Compliance.ProceduresHistoryEntry.RevisionNumberAtTransition  int
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Compliance.ProceduresHistoryEntry') AND c.name = 'RevisionNumberAtTransition'
)
BEGIN
    DECLARE @dc11 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Compliance.ProceduresHistoryEntry') AND c.name = 'RevisionNumberAtTransition'
    );
    EXEC('ALTER TABLE [Compliance].[ProceduresHistoryEntry] DROP CONSTRAINT [' + @dc11 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Compliance.ProceduresHistoryEntry') AND c.name = 'RevisionNumberAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Compliance].[ProceduresHistoryEntry] ALTER COLUMN [RevisionNumberAtTransition] int NULL;
GO

------------------------------------------------------------
-- Operational.ChecklistsHistoryEntry.CompletedItemsAtTransition  int
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Operational.ChecklistsHistoryEntry') AND c.name = 'CompletedItemsAtTransition'
)
BEGIN
    DECLARE @dc12 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Operational.ChecklistsHistoryEntry') AND c.name = 'CompletedItemsAtTransition'
    );
    EXEC('ALTER TABLE [Operational].[ChecklistsHistoryEntry] DROP CONSTRAINT [' + @dc12 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Operational.ChecklistsHistoryEntry') AND c.name = 'CompletedItemsAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Operational].[ChecklistsHistoryEntry] ALTER COLUMN [CompletedItemsAtTransition] int NULL;
GO

------------------------------------------------------------
-- Operational.ChecklistsHistoryEntry.ExceptionCountAtTransition  int
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Operational.ChecklistsHistoryEntry') AND c.name = 'ExceptionCountAtTransition'
)
BEGIN
    DECLARE @dc13 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Operational.ChecklistsHistoryEntry') AND c.name = 'ExceptionCountAtTransition'
    );
    EXEC('ALTER TABLE [Operational].[ChecklistsHistoryEntry] DROP CONSTRAINT [' + @dc13 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Operational.ChecklistsHistoryEntry') AND c.name = 'ExceptionCountAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Operational].[ChecklistsHistoryEntry] ALTER COLUMN [ExceptionCountAtTransition] int NULL;
GO

------------------------------------------------------------
-- Operational.ContractorHistoryEntry.ReadinessScoreAtTransition  decimal(7,4)
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Operational.ContractorHistoryEntry') AND c.name = 'ReadinessScoreAtTransition'
)
BEGIN
    DECLARE @dc14 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Operational.ContractorHistoryEntry') AND c.name = 'ReadinessScoreAtTransition'
    );
    EXEC('ALTER TABLE [Operational].[ContractorHistoryEntry] DROP CONSTRAINT [' + @dc14 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Operational.ContractorHistoryEntry') AND c.name = 'ReadinessScoreAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Operational].[ContractorHistoryEntry] ALTER COLUMN [ReadinessScoreAtTransition] decimal(7,4) NULL;
GO

------------------------------------------------------------
-- Operational.RiskManagementHistoryEntry.RiskScoreAtTransition  int
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('Operational.RiskManagementHistoryEntry') AND c.name = 'RiskScoreAtTransition'
)
BEGIN
    DECLARE @dc15 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('Operational.RiskManagementHistoryEntry') AND c.name = 'RiskScoreAtTransition'
    );
    EXEC('ALTER TABLE [Operational].[RiskManagementHistoryEntry] DROP CONSTRAINT [' + @dc15 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('Operational.RiskManagementHistoryEntry') AND c.name = 'RiskScoreAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [Operational].[RiskManagementHistoryEntry] ALTER COLUMN [RiskScoreAtTransition] int NULL;
GO

------------------------------------------------------------
-- SafetyEvents.EmergencyManagementHistoryEntry.CasualtyCountAtTransition  int
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('SafetyEvents.EmergencyManagementHistoryEntry') AND c.name = 'CasualtyCountAtTransition'
)
BEGIN
    DECLARE @dc16 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('SafetyEvents.EmergencyManagementHistoryEntry') AND c.name = 'CasualtyCountAtTransition'
    );
    EXEC('ALTER TABLE [SafetyEvents].[EmergencyManagementHistoryEntry] DROP CONSTRAINT [' + @dc16 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('SafetyEvents.EmergencyManagementHistoryEntry') AND c.name = 'CasualtyCountAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [SafetyEvents].[EmergencyManagementHistoryEntry] ALTER COLUMN [CasualtyCountAtTransition] int NULL;
GO

------------------------------------------------------------
-- SafetyEvents.ObservationHistoryEntry.CorrectiveActionRequiredAtTransition  bit
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID('SafetyEvents.ObservationHistoryEntry') AND c.name = 'CorrectiveActionRequiredAtTransition'
)
BEGIN
    DECLARE @dc17 sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID('SafetyEvents.ObservationHistoryEntry') AND c.name = 'CorrectiveActionRequiredAtTransition'
    );
    EXEC('ALTER TABLE [SafetyEvents].[ObservationHistoryEntry] DROP CONSTRAINT [' + @dc17 + ']');
END
GO
IF EXISTS (
    SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.object_id = OBJECT_ID('SafetyEvents.ObservationHistoryEntry') AND c.name = 'CorrectiveActionRequiredAtTransition' AND c.is_nullable = 0
)
    ALTER TABLE [SafetyEvents].[ObservationHistoryEntry] ALTER COLUMN [CorrectiveActionRequiredAtTransition] bit NULL;
GO
