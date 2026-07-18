USE [PlantSafety.DiagnosticsDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 2 Durability/Concurrency Remediation
-- Script:        20-DiagnosticsDB-rowversion.sql
-- Purpose:       Add ConcurrencyToken ROWVERSION NOT NULL to every update-capable
--                (mutable aggregate-root) table in PlantSafety.DiagnosticsDB.
-- Selection:     Tables with an UpdatedUtc/UpdatedBy pair (mutable signal per
--                Schema Sentinel rule: create-table.md / align-schema.md).
--                Append-only tables (HistoryEntry, LogEntry, Archive, Error,
--                ReplayRecord, Snapshot-of-fact tables, etc.) are intentionally
--                skipped -- see companion report.
-- Idempotent:    Guarded by COL_LENGTH() check; safe to re-run.
-- =============================================

IF COL_LENGTH('[Diagnostics].[DiagnosticAttachment]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Diagnostics].[DiagnosticAttachment] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Diagnostics].[DiagnosticEventDimension]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Diagnostics].[DiagnosticEventDimension] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Diagnostics].[DiagnosticSnapshot]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Diagnostics].[DiagnosticSnapshot] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Diagnostics].[EngineRun]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Diagnostics].[EngineRun] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Diagnostics].[ProviderRegistry]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Diagnostics].[ProviderRegistry] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Diagnostics].[RuleRegistry]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Diagnostics].[RuleRegistry] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Diagnostics].[TenantEngineRun]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Diagnostics].[TenantEngineRun] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Diagnostics].[TenantRuleOverride]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Diagnostics].[TenantRuleOverride] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO
