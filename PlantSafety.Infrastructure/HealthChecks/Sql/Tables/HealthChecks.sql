-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HealthChecks
--  Layer:          Infrastructure – SQL Scripts
--  File:           HealthChecks.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the HealthChecks read-model, backing
--                  IHealthChecksRepository.GetChecksAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HealthChecks')
BEGIN
    EXEC('CREATE SCHEMA [HealthChecks]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HealthChecks' AND schema_id = SCHEMA_ID('HealthChecks'))
BEGIN
    CREATE TABLE [HealthChecks].[HealthChecks] (
        CheckId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        CheckName     NVARCHAR(200)    NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        CheckedAtUtc  DATETIME2(3)     NOT NULL,
        CheckedBy     UNIQUEIDENTIFIER NOT NULL,
        IsDeleted     BIT              NOT NULL CONSTRAINT DF_HealthChecks_IsDeleted DEFAULT (0),
        CreatedAtUtc  DATETIME2(3)     NOT NULL CONSTRAINT DF_HealthChecks_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy     UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc DATETIME2(3)     NULL,
        ModifiedBy    UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_HealthChecks_TenantId_SiteId] ON [HealthChecks].[HealthChecks] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_HealthChecks_CheckedAtUtc] ON [HealthChecks].[HealthChecks] (CheckedAtUtc);
END
GO
