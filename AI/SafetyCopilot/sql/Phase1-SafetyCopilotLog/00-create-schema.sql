-- ============================================================================
--  Project:        AI.SafetyCopilot
--  Module:         Phase1-SafetyCopilotLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [AISafetyCopilot] schema -- audit-trail log
--                  of every real SafetyCopilotEngine.Guide call (Phase E.8).
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see AI.Reasoning's
--  Phase1-ReasoningLog note.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AISafetyCopilot')
BEGIN
    EXEC('CREATE SCHEMA [AISafetyCopilot] AUTHORIZATION dbo;');
END
GO
