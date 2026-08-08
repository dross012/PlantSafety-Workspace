-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        19-fn-orchestration-categoryforeventtype.sql
-- Purpose:       Creates Orchestration.fn_Orchestration_CategoryForEventType
--                against [PlantSafety.CoreDB] -- a deterministic scalar
--                function mirroring
--                OrchestrationEventCategoryCatalog.ForEventType(eventType)
--                (Shared.SafetyData.Abstractions.Models.Industries.
--                Orchestration\OrchestrationEventCategory.cs): classifies an
--                EventType string by suffix into 'Created' / 'Archived' /
--                'Other'.
--
-- VALUE JUSTIFICATION: this is cheap (single CASE expression, no table
-- access), reusable across every INSERT/upsert procedure in this folder
-- that needs to independently re-derive Category from EventType for
-- defense-in-depth (rather than trusting only the caller-supplied
-- @Category parameter), and it is a pure, deterministic string-suffix rule
-- with no other business logic -- a faithful, trivial-to-verify mirror of
-- the C# catalog, not a duplication of anything nontrivial that belongs in
-- C#.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER FUNCTION [Orchestration].[fn_Orchestration_CategoryForEventType]
(
    @EventType nvarchar(200)
)
RETURNS nvarchar(20)
WITH SCHEMABINDING
AS
BEGIN
    RETURN (
        CASE
            WHEN @EventType LIKE '%Created'  ESCAPE '\' THEN N'Created'
            WHEN @EventType LIKE '%Archived' ESCAPE '\' THEN N'Archived'
            ELSE N'Other'
        END
    );
END
GO
