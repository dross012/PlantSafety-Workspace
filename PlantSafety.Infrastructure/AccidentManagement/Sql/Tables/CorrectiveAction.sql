-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         AccidentManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           CorrectiveAction.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Child table backing the Accident.CorrectiveActions collection (CorrectiveAction
--                  entity, CAPA). TenantId is denormalized from the parent Accident for
--                  Row-Level-Security discovery/enforcement. Additive-only evolution.
--  UPDATED:        2026-07-28 (Migration Commander Phase 2, AccidentManagement) -- added a native
--                  RowVersion ROWVERSION NOT NULL column. This table had no concurrency column at
--                  all before this pass. Matches the canonical Phase 1/2 pattern (RowVersion
--                  exposed to callers as ETag via CONVERT(NVARCHAR(34), CAST(RowVersion AS
--                  BINARY(8)), 1) in usp_CorrectiveAction_GetByAccidentId). Idempotent via
--                  COL_LENGTH guard; safe to re-run against a database that already has the column.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CorrectiveAction' AND schema_id = SCHEMA_ID('AccidentManagement'))
BEGIN
    CREATE TABLE [AccidentManagement].[CorrectiveAction]
    (
        CorrectiveActionId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CorrectiveAction PRIMARY KEY,
        AccidentId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_CorrectiveAction_Accident REFERENCES [AccidentManagement].[Accident] (AccidentId),
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        Category            NVARCHAR(30)     NOT NULL,
        Description         NVARCHAR(2000)   NOT NULL,
        ResponsiblePersonId UNIQUEIDENTIFIER NOT NULL,
        DueAtUtc            DATETIME2(3)     NOT NULL,
        Status              NVARCHAR(30)     NOT NULL,
        VerificationSteps   NVARCHAR(2000)   NULL,
        CompletionEvidence  NVARCHAR(2000)   NULL,
        CompletedAtUtc      DATETIME2(3)     NULL,
        VerifiedAtUtc       DATETIME2(3)     NULL,
        VerifiedBy          UNIQUEIDENTIFIER NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL,
        IsDeleted           BIT              NOT NULL CONSTRAINT DF_CorrectiveAction_IsDeleted DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_CorrectiveAction_AccidentId ON [AccidentManagement].[CorrectiveAction] (AccidentId, IsDeleted);
END
GO

-- Additive: native ROWVERSION concurrency column. Idempotent.
IF COL_LENGTH('[AccidentManagement].[CorrectiveAction]', 'RowVersion') IS NULL
BEGIN
    ALTER TABLE [AccidentManagement].[CorrectiveAction] ADD RowVersion ROWVERSION NOT NULL;
END
GO
