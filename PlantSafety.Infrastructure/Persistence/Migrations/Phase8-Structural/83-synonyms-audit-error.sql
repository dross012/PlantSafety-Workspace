-- =============================================
-- Phase 8 Structural Additions
-- Script:        83-synonyms-audit-error.sql
-- Purpose:       Add CREATE SYNONYM objects for the two shared,
--                cross-database platform logging tables
--                ([PlantSafety.AuditDB].dbo.AuditLog and
--                [PlantSafety.ErrorDB].dbo.ErrorLog) in each of the 8
--                in-scope module databases, as available infrastructure
--                for FUTURE stored procedure authoring.
--
-- Judgment call (per task instructions): the existing 400+ Phase 6/7
-- procedures already work correctly using the bracket-quoted 3-part name
-- pattern (e.g. [PlantSafety.AuditDB].dbo.AuditLog) documented as an
-- approved, deliberate pattern in 00-canonical-templates.md. This script
-- does NOT rewrite any of them -- that is explicitly out of scope/too
-- risky per the task instructions. What IS added here is a same-named
-- local synonym (dbo.AuditLog / dbo.ErrorLog) in each of the 8 databases
-- that resolves to the real 3-part cross-database reference, verified live
-- to work correctly (SELECT through the synonym returns real AuditLog rows).
--
-- Rationale for building this (rather than skipping it): it gives future
-- proc authors a strictly shorter, single-database-scoped alternative
-- (INSERT INTO dbo.AuditLog (...) instead of the 3-part bracket-quoted
-- form) while leaving the DB name change surface localized to one synonym
-- object per database instead of every future proc body, without touching
-- anything that exists today.
--
-- Deliberately NOT synonym-wrapped: [PlantSafety.AuditDB].dbo.usp_WriteAuditLog,
-- [PlantSafety.ErrorDB].dbo.usp_WriteErrorLog, and the
-- [PlantSafety.ErrorDB].dbo.TR_ErrorLog_NoDelete trigger. These are the
-- documented, approved-exception, "shared cross-database platform-utility"
-- entry points per 00-canonical-templates.md, with a live-verified zero
-- current callers anywhere in the solution. Wrapping already-zero-caller
-- objects in an additional layer of indirection adds pure risk (a second
-- discoverable name for the same object, inviting future confusion about
-- which name is canonical) with no realistic present-day consumer to
-- benefit from it. If/when these procs gain real callers, whether to add a
-- synonym for them is a decision for that point in time, not now.
--
-- Idempotent: guarded by sys.synonyms existence checks. Safe to re-run.
-- All application tables confirmed at 0 rows before and after (synonyms
-- carry no data of their own; AuditLog/ErrorLog already contain historical
-- platform logging rows from prior phases' own operations, which is
-- expected and unrelated to the 0-row application-table baseline).
-- =============================================

-- In-scope databases (documented here for reference; dynamic cross-database
-- DDL from a single batch is avoided in favor of explicit, reviewable
-- per-database USE blocks below, consistent with every other script in this
-- migration series): PlantSafety.CoreDB, ArchiveDB, CloudIdentityDB,
-- PredictiveDB, ReportingDB, IntegrationDB, DiagnosticsDB, ExportImportDB.

------------------------------------------------------------------------------
-- PlantSafety.CoreDB
------------------------------------------------------------------------------
USE [PlantSafety.CoreDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'AuditLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.AuditLog FOR [PlantSafety.AuditDB].dbo.AuditLog;
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'ErrorLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.ErrorLog FOR [PlantSafety.ErrorDB].dbo.ErrorLog;
GO

------------------------------------------------------------------------------
-- PlantSafety.ArchiveDB
------------------------------------------------------------------------------
USE [PlantSafety.ArchiveDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'AuditLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.AuditLog FOR [PlantSafety.AuditDB].dbo.AuditLog;
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'ErrorLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.ErrorLog FOR [PlantSafety.ErrorDB].dbo.ErrorLog;
GO

------------------------------------------------------------------------------
-- PlantSafety.CloudIdentityDB
------------------------------------------------------------------------------
USE [PlantSafety.CloudIdentityDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'AuditLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.AuditLog FOR [PlantSafety.AuditDB].dbo.AuditLog;
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'ErrorLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.ErrorLog FOR [PlantSafety.ErrorDB].dbo.ErrorLog;
GO

------------------------------------------------------------------------------
-- PlantSafety.PredictiveDB
------------------------------------------------------------------------------
USE [PlantSafety.PredictiveDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'AuditLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.AuditLog FOR [PlantSafety.AuditDB].dbo.AuditLog;
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'ErrorLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.ErrorLog FOR [PlantSafety.ErrorDB].dbo.ErrorLog;
GO

------------------------------------------------------------------------------
-- PlantSafety.ReportingDB
------------------------------------------------------------------------------
USE [PlantSafety.ReportingDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'AuditLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.AuditLog FOR [PlantSafety.AuditDB].dbo.AuditLog;
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'ErrorLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.ErrorLog FOR [PlantSafety.ErrorDB].dbo.ErrorLog;
GO

------------------------------------------------------------------------------
-- PlantSafety.IntegrationDB
------------------------------------------------------------------------------
USE [PlantSafety.IntegrationDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'AuditLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.AuditLog FOR [PlantSafety.AuditDB].dbo.AuditLog;
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'ErrorLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.ErrorLog FOR [PlantSafety.ErrorDB].dbo.ErrorLog;
GO

------------------------------------------------------------------------------
-- PlantSafety.DiagnosticsDB
------------------------------------------------------------------------------
USE [PlantSafety.DiagnosticsDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'AuditLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.AuditLog FOR [PlantSafety.AuditDB].dbo.AuditLog;
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'ErrorLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.ErrorLog FOR [PlantSafety.ErrorDB].dbo.ErrorLog;
GO

------------------------------------------------------------------------------
-- PlantSafety.ExportImportDB
------------------------------------------------------------------------------
USE [PlantSafety.ExportImportDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'AuditLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.AuditLog FOR [PlantSafety.AuditDB].dbo.AuditLog;
GO
IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'ErrorLog' AND SCHEMA_NAME(schema_id) = N'dbo')
    CREATE SYNONYM dbo.ErrorLog FOR [PlantSafety.ErrorDB].dbo.ErrorLog;
GO
