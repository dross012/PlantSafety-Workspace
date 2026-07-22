-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Notifications.Email
--  Layer:          Infrastructure – SQL Scripts
--  File:           NotificationsEmailItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Notifications.Email read-model, backing
--                  Integrations.Notifications.Email.Repositories.IEmailRepository.GetEmailDeliveriesAsync.
--                  Distinct from [Integrations].[EmailItems], which backs the separate
--                  root-level Integrations.Email.IEmailRepository interface.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotificationsEmailItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[NotificationsEmailItems] (
        EmailDeliveryId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        RecipientAddress  NVARCHAR(320)    NOT NULL,
        Subject           NVARCHAR(400)    NOT NULL,
        TemplateName      NVARCHAR(200)    NULL,
        Status            NVARCHAR(50)     NOT NULL,
        SentAtUtc         DATETIME2(3)     NOT NULL,
        SentBy            UNIQUEIDENTIFIER NOT NULL,
        IsDeleted         BIT              NOT NULL CONSTRAINT DF_NotificationsEmailItems_IsDeleted DEFAULT (0),
        CreatedUtc        DATETIME2(3)     NOT NULL CONSTRAINT DF_NotificationsEmailItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy         UNIQUEIDENTIFIER NULL,
        UpdatedUtc        DATETIME2(3)     NULL,
        UpdatedBy         UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_NotificationsEmailItems_TenantId_SiteId] ON [Integrations].[NotificationsEmailItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_NotificationsEmailItems_SentAtUtc] ON [Integrations].[NotificationsEmailItems] (SentAtUtc);
END
GO
