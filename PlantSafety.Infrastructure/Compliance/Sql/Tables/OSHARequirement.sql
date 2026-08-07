-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.OSHAStandards
--  Layer:          Infrastructure - SQL Scripts
--  File:           OSHARequirement.sql
--  Created:        2026-08-06
--  SchemaVersion:  2026.1
--  Description:    Requirement rows derived directly from Compliance.OSHAStandard section
--                  text (sentences containing "must"/"shall"/"required"/"prohibited" etc.).
--                  Not fabricated -- Notes always carries the raw source sentence. Global
--                  reference data, same scoping rules as OSHAStandard (no TenantId).
--                  AppliesToModule is left NULL on import; PlantSafety modules map
--                  themselves to requirements later via Fact Providers / cross-domain
--                  rules, not at import time. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OSHARequirement' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[OSHARequirement] (
        [OSHARequirementId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OSHARequirement PRIMARY KEY,
        [OSHAStandardId]    UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_OSHARequirement_OSHAStandard REFERENCES [Compliance].[OSHAStandard] ([OSHAStandardId]),
        [RequirementType]   NVARCHAR(100)    NULL,
        [AppliesToModule]   NVARCHAR(100)    NULL,
        [Severity]          NVARCHAR(50)     NULL,
        [Notes]             NVARCHAR(MAX)    NOT NULL,
        [CreatedAtUtc]      DATETIME2(3)     NOT NULL CONSTRAINT DF_OSHARequirement_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        RowVersion          ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_OSHARequirement_OSHAStandardId] ON [Compliance].[OSHARequirement] ([OSHAStandardId]);
    CREATE NONCLUSTERED INDEX [IX_OSHARequirement_AppliesToModule] ON [Compliance].[OSHARequirement] ([AppliesToModule]);
END
GO
