-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         RiskManagement
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ControlMeasure.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Child table backing the Risk.ControlMeasures collection (ControlMeasure
--                  entity, Hierarchy of Controls). TenantId is denormalized from the parent Risk
--                  for Row-Level-Security discovery/enforcement. Additive-only evolution.
--  UPDATED:        2026-07-28 (Migration Commander Phase 3, RiskManagement) -- added a native
--                  RowVersion ROWVERSION NOT NULL column, matching Risk.sql's RowVersion addition,
--                  exposed to callers as ETag via CONVERT(NVARCHAR(34), CAST(RowVersion AS
--                  BINARY(8)), 1) in usp_ControlMeasure_GetByRiskId. ControlMeasure has no
--                  pre-existing ConcurrencyToken column, so this is a pure addition. Idempotent via
--                  COL_LENGTH guard; safe to re-run against a database that already has the column.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ControlMeasure' AND schema_id = SCHEMA_ID('RiskMgmt'))
BEGIN
    CREATE TABLE [RiskMgmt].[ControlMeasure]
    (
        ControlMeasureId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ControlMeasure PRIMARY KEY,
        RiskId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ControlMeasure_Risk REFERENCES [RiskMgmt].[Risk] (RiskId),
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        HierarchyLevel      NVARCHAR(30)     NOT NULL,
        Description         NVARCHAR(2000)   NOT NULL,
        Justification       NVARCHAR(2000)   NULL,
        ResponsiblePersonId UNIQUEIDENTIFIER NOT NULL,
        ImplementedAtUtc    DATETIME2(3)     NOT NULL,
        IsDeleted           BIT              NOT NULL CONSTRAINT DF_ControlMeasure_IsDeleted DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_ControlMeasure_RiskId ON [RiskMgmt].[ControlMeasure] (RiskId, IsDeleted);
END
GO

-- Additive: native ROWVERSION concurrency column -- see header comment. Idempotent.
IF COL_LENGTH('[RiskMgmt].[ControlMeasure]', 'RowVersion') IS NULL
BEGIN
    ALTER TABLE [RiskMgmt].[ControlMeasure] ADD RowVersion ROWVERSION NOT NULL;
END
GO
