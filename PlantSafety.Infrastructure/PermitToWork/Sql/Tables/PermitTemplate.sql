-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PermitTemplate.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PermitTemplate aggregate root (IPermitTemplateRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PermitToWork')
    EXEC('CREATE SCHEMA PermitToWork AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitTemplate' AND schema_id = SCHEMA_ID('PermitToWork'))
BEGIN
    CREATE TABLE [PermitToWork].[PermitTemplate]
    (
        PermitTemplateId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PermitTemplate PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        PermitTypeCode NVARCHAR(400) NOT NULL,
        RequiredControls NVARCHAR(400) NOT NULL,
        StandardValidityHours INT NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PermitTemplate_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PermitTemplate_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PermitTemplate_TenantId_Site ON [PermitToWork].[PermitTemplate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitTemplate_Status ON [PermitToWork].[PermitTemplate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PermitTemplate_CreatedAtUtc ON [PermitToWork].[PermitTemplate] (CreatedAtUtc);
END
GO
