-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Intelligence.PredictiveSafety
--  Layer:          Infrastructure – SQL Scripts
--  File:           PredictiveSafetyAlert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the new Phase 4 PredictiveSafetyAlert aggregate root, backing
--                  IPredictiveSafetyAlertRepository (Domain write-side). Deliberately a distinct
--                  table from the pre-existing [Intelligence].[PredictiveSafety] read-model table
--                  (which backs the untouched date-range
--                  GetPredictiveSafetyQuery/usp_PredictiveSafety_GetPredictions read path) -- this
--                  table carries the full lifecycle state (Active/Acknowledged/Resolved/Archived)
--                  plus a native RowVersion for optimistic-concurrency ETags. Mirrors
--                  [ContinuousImprovement].[RCA]'s Phase 4 additive-column precedent, but as a
--                  wholly new table per the task's "pick distinct proc/table names for your own
--                  new write paths" instruction.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Intelligence')
BEGIN
    EXEC('CREATE SCHEMA [Intelligence]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PredictiveSafetyAlert' AND schema_id = SCHEMA_ID('Intelligence'))
BEGIN
    CREATE TABLE [Intelligence].[PredictiveSafetyAlert] (
        PredictiveSafetyAlertId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        RiskCategory            NVARCHAR(100)    NOT NULL,
        PredictedRiskScore      FLOAT            NOT NULL,
        ModelVersion            NVARCHAR(50)     NULL,
        Status                  NVARCHAR(50)     NOT NULL,
        PredictedAtUtc          DATETIME2(3)     NOT NULL,
        GeneratedBy             UNIQUEIDENTIFIER NOT NULL,
        AcknowledgedBy          UNIQUEIDENTIFIER NULL,
        AcknowledgedAtUtc       DATETIME2(3)     NULL,
        ResolvedBy              UNIQUEIDENTIFIER NULL,
        ResolutionNotes         NVARCHAR(MAX)    NULL,
        ResolvedAtUtc           DATETIME2(3)     NULL,
        ArchivedAtUtc           DATETIME2(3)     NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_PredictiveSafetyAlert_IsDeleted DEFAULT (0),
        CreatedAtUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_PredictiveSafetyAlert_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy               UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc           DATETIME2(3)     NULL,
        ModifiedBy              UNIQUEIDENTIFIER NULL,
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PredictiveSafetyAlert_TenantId_SiteId] ON [Intelligence].[PredictiveSafetyAlert] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PredictiveSafetyAlert_PredictedAtUtc] ON [Intelligence].[PredictiveSafetyAlert] (PredictedAtUtc);
END
GO
