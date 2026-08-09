-- ============================================================================
--  Project:        AI.Reasoning
--  Module:         Phase1-ReasoningLog
--  File:           00-create-schema.sql
--  Created:        2026-08-09
--  Description:    Creates the [AIReasoning] schema -- an audit-trail log of
--                  every real SafetyReasoningEngine.Reason(SafetyReasoningContext,
--                  rules) call (Phase E.1), currently a pure deterministic
--                  function with zero persistence. Per this task's explicit
--                  instruction for every Path E engine: a session/log table
--                  (ReasoningSessionId is the C# type's own real session
--                  identity -- SafetyReasoningContext.ReasoningSessionId),
--                  not a full normalized relational model of every nested
--                  record -- Conclusions (IReadOnlyList<string>) is
--                  serialized as JSON.
--  DATABASE PLACEMENT: [PlantSafety.CoreDB] -- see Shared.FacilityDomain's
--  Phase1-Lookup DATABASE PLACEMENT ASSUMPTION note; extends to every AI.*
--  bounded context in Path E.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AIReasoning')
BEGIN
    EXEC('CREATE SCHEMA [AIReasoning] AUTHORIZATION dbo;');
END
GO
