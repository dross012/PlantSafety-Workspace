-- ============================================================================
--  Project:        Enterprise.DataLake
--  Module:         Phase1-DataLake
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [DataLake] schema -- the durable, append-only
--                  store this task explicitly calls out as needing "a real
--                  durable table given it's meant to be a data lake", for
--                  Enterprise.DataLake.Abstractions.Records.DataLakeRecord
--                  (Phase D.2), currently held only in an in-memory
--                  ConcurrentQueue.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB]. The task instructs consulting
--  the DataLake manuals "only where the target database is a lake zone" --
--  no separate lake-zone database exists in 00-canonical-templates.md's live
--  inventory (no "DataLake"/"Lake" entry), so this table is provisioned in
--  CoreDB per the same DATABASE PLACEMENT ASSUMPTION as every other new Path
--  D/E schema in this task, structured as a genuinely append-only,
--  partition-ready table (HotData/ColdData filegroup convention documented
--  below) so it can be physically re-homed to a dedicated lake-zone database
--  later without a schema redesign. Flag for a dedicated lake-zone database
--  provisioning decision.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'DataLake')
BEGIN
    EXEC('CREATE SCHEMA [DataLake] AUTHORIZATION dbo;');
END
GO
