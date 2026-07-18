USE [PlantSafety.PredictiveDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 2 Durability/Concurrency Remediation
-- Script:        20-PredictiveDB-rowversion.sql
-- Purpose:       Add ConcurrencyToken ROWVERSION NOT NULL to every update-capable
--                (mutable aggregate-root) table in PlantSafety.PredictiveDB.
-- Selection:     Tables with an UpdatedUtc/UpdatedBy pair (mutable signal per
--                Schema Sentinel rule: create-table.md / align-schema.md).
--                Append-only tables (HistoryEntry, LogEntry, Archive, Error,
--                ReplayRecord, Snapshot-of-fact tables, etc.) are intentionally
--                skipped -- see companion report.
-- Idempotent:    Guarded by COL_LENGTH() check; safe to re-run.
-- =============================================

IF COL_LENGTH('[PredictiveSafety].[SafetyPrediction]', 'ConcurrencyToken') IS NULL
BEGIN
    ALTER TABLE [PredictiveSafety].[SafetyPrediction] ADD ConcurrencyToken ROWVERSION NOT NULL;
END;
GO
