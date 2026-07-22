-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CorrectiveAction_GetByAccidentId.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all CorrectiveAction child rows for a given AccidentId.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_CorrectiveAction_GetByAccidentId]
    @AccidentId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CorrectiveActionId, AccidentId, Category, Description, ResponsiblePersonId, DueAtUtc, Status,
        VerificationSteps, CompletionEvidence, CompletedAtUtc, VerifiedAtUtc, VerifiedBy, CreatedAtUtc
    FROM [AccidentManagement].[CorrectiveAction]
    WHERE AccidentId = @AccidentId AND IsDeleted = 0
    ORDER BY CreatedAtUtc ASC;
END
GO
