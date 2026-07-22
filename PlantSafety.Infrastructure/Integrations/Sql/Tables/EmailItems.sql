-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Email
--  Layer:          Infrastructure – SQL Scripts
--  File:           EmailItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the root-level Integrations.Email read-model, backing
--                  IEmailRepository.GetEmailsAsync (PlantSafety.Application.Integrations.Email).
--                  Distinct from [Integrations].[NotificationsEmailItems], which backs the
--                  separate Integrations.Notifications.Email.IEmailRepository interface.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmailItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[EmailItems] (
        EmailId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        Recipient     NVARCHAR(320)    NOT NULL,
        Subject       NVARCHAR(400)    NOT NULL,
        Provider      NVARCHAR(100)    NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        SentAtUtc     DATETIME2(3)     NOT NULL,
        SentBy        UNIQUEIDENTIFIER NOT NULL,
        IsDeleted     BIT              NOT NULL CONSTRAINT DF_EmailItems_IsDeleted DEFAULT (0),
        CreatedUtc    DATETIME2(3)     NOT NULL CONSTRAINT DF_EmailItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy     UNIQUEIDENTIFIER NULL,
        UpdatedUtc    DATETIME2(3)     NULL,
        UpdatedBy     UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_EmailItems_TenantId_SiteId] ON [Integrations].[EmailItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_EmailItems_SentAtUtc] ON [Integrations].[EmailItems] (SentAtUtc);
END
GO
