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
--  UPDATED:        2026-07-28 -- added ETag, projected from the new RowVersion column the same way
--                  as usp_Accident_GetById.sql.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [AccidentManagement].[usp_CorrectiveAction_GetByAccidentId]
    @AccidentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        CorrectiveActionId, AccidentId, Category, Description, ResponsiblePersonId, DueAtUtc, Status,
        VerificationSteps, CompletionEvidence, CompletedAtUtc, VerifiedAtUtc, VerifiedBy, CreatedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [AccidentManagement].[CorrectiveAction]
    -- App-layer tenant scoping (defense-in-depth alongside RLS, per F1 in
    -- docs\lifecycle\security\rls-audit.md).
    WHERE AccidentId = @AccidentId AND TenantId = @TenantId AND IsDeleted = 0
    ORDER BY CreatedAtUtc ASC;
END
GO
