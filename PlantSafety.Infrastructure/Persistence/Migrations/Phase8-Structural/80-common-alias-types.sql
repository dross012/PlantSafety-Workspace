-- =============================================
-- Phase 8 Structural Additions
-- Script:        80-common-alias-types.sql
-- Purpose:       Create standardization CREATE TYPE alias types for the
--                genuinely-repeated column shapes confirmed live via
--                sys.columns survey across all 8 in-scope databases on
--                2026-07-19 (PlantSafety.CoreDB, ArchiveDB, CloudIdentityDB,
--                PredictiveDB, ReportingDB, IntegrationDB, DiagnosticsDB,
--                ExportImportDB).
--
-- Grounding (live survey, not invented):
--   - dbo.StrongId  -> uniqueidentifier. Every *Id column in every one of the
--     8 databases already uses uniqueidentifier (e.g. 165 NOT NULL + 32
--     NULL usages in CoreDB alone). This is THE dominant, universal shape.
--   - dbo.Utc       -> datetime2(3). Across all 8 databases, 197 of 250
--     *Utc columns use datetime2(3) vs. 53 using datetime2(7) (a REAL,
--     pre-existing precision inconsistency -- see note below, NOT invented
--     for this script). datetime2(3) is the dominant, majority shape, so it
--     is the one standardized here.
--   - dbo.CorrelationId -> uniqueidentifier. CorrelationId is
--     uniqueidentifier in ArchiveDB.ArchivalJob, PredictiveDB.SafetyPrediction,
--     ReportingDB.ReportMetadata, and IntegrationDB.ReplayRecord /
--     IntegrationLogEntry (5 of 8 live usages). DiagnosticsDB currently uses
--     nvarchar(200) for the same concept on 3 tables -- a REAL, pre-existing
--     type inconsistency, not invented for this script.
--
-- IMPORTANT -- scope discipline:
--   - These types are additive, forward-looking standardization tools for
--     FUTURE table/column authoring. Per the deletion/alteration discipline
--     in this agent's operating rules, this script does NOT retrofit any
--     existing column to use these aliases -- that would require ALTER
--     COLUMN on live (even if currently 0-row) production tables, which is
--     out of scope for a "structural additions" pass and belongs to a
--     dedicated align-schema remediation pass instead.
--   - Base type is declared nullable (NULL) rather than NOT NULL so that
--     the alias can be used for BOTH nullable and non-nullable columns --
--     column-level NOT NULL can still be added explicitly at the point of
--     use, exactly as with the raw base types today. A NOT NULL alias would
--     have made the type unusable for the many legitimately-nullable
--     Updated*/Completed*/Closed*-style Utc columns and FK columns observed
--     live (e.g. CoreDB has 41 nullable + 60 non-nullable datetime2(3) Utc
--     columns).
--   - Two pre-existing drift items were surfaced by this survey and are
--     flagged here for a future Schema Sentinel align-schema pass, not
--     silently fixed in this structural-additions script:
--       1. datetime2(3) vs datetime2(7) precision drift on *Utc columns
--          (197 vs 53 across the 8 databases).
--       2. CorrelationId type drift: uniqueidentifier (5 usages) vs
--          nvarchar(200) (3 usages, all in PlantSafety.DiagnosticsDB).
--
-- SQL Server scoping note: CREATE TYPE is database-scoped (no cross-database
-- user-defined types), so this script creates the same three alias types
-- identically in each of the 8 in-scope databases.
--
-- Idempotent: guarded by sys.types existence checks. Safe to re-run.
-- All application tables confirmed at 0 rows before and after (types carry
-- no data of their own).
-- =============================================

------------------------------------------------------------------------------
-- PlantSafety.CoreDB
------------------------------------------------------------------------------
USE [PlantSafety.CoreDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'StrongId' AND is_user_defined = 1)
    CREATE TYPE dbo.StrongId FROM uniqueidentifier NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'Utc' AND is_user_defined = 1)
    CREATE TYPE dbo.Utc FROM datetime2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'CorrelationId' AND is_user_defined = 1)
    CREATE TYPE dbo.CorrelationId FROM uniqueidentifier NULL;
GO

------------------------------------------------------------------------------
-- PlantSafety.ArchiveDB
------------------------------------------------------------------------------
USE [PlantSafety.ArchiveDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'StrongId' AND is_user_defined = 1)
    CREATE TYPE dbo.StrongId FROM uniqueidentifier NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'Utc' AND is_user_defined = 1)
    CREATE TYPE dbo.Utc FROM datetime2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'CorrelationId' AND is_user_defined = 1)
    CREATE TYPE dbo.CorrelationId FROM uniqueidentifier NULL;
GO

------------------------------------------------------------------------------
-- PlantSafety.CloudIdentityDB
------------------------------------------------------------------------------
USE [PlantSafety.CloudIdentityDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'StrongId' AND is_user_defined = 1)
    CREATE TYPE dbo.StrongId FROM uniqueidentifier NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'Utc' AND is_user_defined = 1)
    CREATE TYPE dbo.Utc FROM datetime2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'CorrelationId' AND is_user_defined = 1)
    CREATE TYPE dbo.CorrelationId FROM uniqueidentifier NULL;
GO

------------------------------------------------------------------------------
-- PlantSafety.PredictiveDB
------------------------------------------------------------------------------
USE [PlantSafety.PredictiveDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'StrongId' AND is_user_defined = 1)
    CREATE TYPE dbo.StrongId FROM uniqueidentifier NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'Utc' AND is_user_defined = 1)
    CREATE TYPE dbo.Utc FROM datetime2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'CorrelationId' AND is_user_defined = 1)
    CREATE TYPE dbo.CorrelationId FROM uniqueidentifier NULL;
GO

------------------------------------------------------------------------------
-- PlantSafety.ReportingDB
------------------------------------------------------------------------------
USE [PlantSafety.ReportingDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'StrongId' AND is_user_defined = 1)
    CREATE TYPE dbo.StrongId FROM uniqueidentifier NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'Utc' AND is_user_defined = 1)
    CREATE TYPE dbo.Utc FROM datetime2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'CorrelationId' AND is_user_defined = 1)
    CREATE TYPE dbo.CorrelationId FROM uniqueidentifier NULL;
GO

------------------------------------------------------------------------------
-- PlantSafety.IntegrationDB
------------------------------------------------------------------------------
USE [PlantSafety.IntegrationDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'StrongId' AND is_user_defined = 1)
    CREATE TYPE dbo.StrongId FROM uniqueidentifier NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'Utc' AND is_user_defined = 1)
    CREATE TYPE dbo.Utc FROM datetime2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'CorrelationId' AND is_user_defined = 1)
    CREATE TYPE dbo.CorrelationId FROM uniqueidentifier NULL;
GO

------------------------------------------------------------------------------
-- PlantSafety.DiagnosticsDB
------------------------------------------------------------------------------
USE [PlantSafety.DiagnosticsDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'StrongId' AND is_user_defined = 1)
    CREATE TYPE dbo.StrongId FROM uniqueidentifier NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'Utc' AND is_user_defined = 1)
    CREATE TYPE dbo.Utc FROM datetime2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'CorrelationId' AND is_user_defined = 1)
    CREATE TYPE dbo.CorrelationId FROM uniqueidentifier NULL;
GO

------------------------------------------------------------------------------
-- PlantSafety.ExportImportDB
------------------------------------------------------------------------------
USE [PlantSafety.ExportImportDB]
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'StrongId' AND is_user_defined = 1)
    CREATE TYPE dbo.StrongId FROM uniqueidentifier NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'Utc' AND is_user_defined = 1)
    CREATE TYPE dbo.Utc FROM datetime2(3) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'CorrelationId' AND is_user_defined = 1)
    CREATE TYPE dbo.CorrelationId FROM uniqueidentifier NULL;
GO
