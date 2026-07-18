USE [PlantSafety.IntegrationDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 2 Durability/Concurrency Remediation
-- Script:        20-IntegrationDB-rowversion.sql
-- Purpose:       Add ConcurrencyToken ROWVERSION NOT NULL to every update-capable
--                (mutable aggregate-root) table in PlantSafety.IntegrationDB.
-- Selection:     Tables with an UpdatedUtc/UpdatedBy pair (mutable signal per
--                Schema Sentinel rule: create-table.md / align-schema.md).
--                Append-only tables (HistoryEntry, LogEntry, Archive, Error,
--                ReplayRecord, Snapshot-of-fact tables, etc.) are intentionally
--                skipped -- see companion report.
-- Idempotent:    Guarded by COL_LENGTH() check; safe to re-run.
-- =============================================

IF COL_LENGTH('[Integration].[ContractField]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Integration].[ContractField] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Integration].[ContractSchema]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Integration].[ContractSchema] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Integration].[ContractVersion]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Integration].[ContractVersion] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[Integration].[RoutingTableEntry]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [Integration].[RoutingTableEntry] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO
