USE [PlantSafety.ExportImportDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 2 Durability/Concurrency Remediation
-- Script:        20-ExportImportDB-rowversion.sql
-- Purpose:       Add ConcurrencyToken ROWVERSION NOT NULL to every update-capable
--                (mutable aggregate-root) table in PlantSafety.ExportImportDB.
-- Selection:     Tables with an UpdatedUtc/UpdatedBy pair (mutable signal per
--                Schema Sentinel rule: create-table.md / align-schema.md).
--                Append-only tables (HistoryEntry, LogEntry, Archive, Error,
--                ReplayRecord, Snapshot-of-fact tables, etc.) are intentionally
--                skipped -- see companion report.
-- Idempotent:    Guarded by COL_LENGTH() check; safe to re-run.
-- =============================================

IF COL_LENGTH('[ExportImport].[ExportBatch]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [ExportImport].[ExportBatch] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[ExportImport].[ExportOperation]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [ExportImport].[ExportOperation] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[ExportImport].[ExportRecord]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [ExportImport].[ExportRecord] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[ExportImport].[ImportBatch]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [ExportImport].[ImportBatch] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[ExportImport].[ImportOperation]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [ExportImport].[ImportOperation] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO

IF COL_LENGTH('[ExportImport].[ImportRecord]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [ExportImport].[ImportRecord] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO
