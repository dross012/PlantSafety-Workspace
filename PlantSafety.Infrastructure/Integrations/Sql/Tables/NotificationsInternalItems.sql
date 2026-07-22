-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Notifications.Internal
--  Layer:          Infrastructure – SQL Scripts
--  File:           NotificationsInternalItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Notifications.Internal read-model, backing
--                  IInternalRepository.GetInternalNotificationsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotificationsInternalItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[NotificationsInternalItems] (
        NotificationId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        RecipientUserId  UNIQUEIDENTIFIER NOT NULL,
        Message          NVARCHAR(MAX)    NOT NULL,
        Category         NVARCHAR(100)    NULL,
        Status           NVARCHAR(50)     NOT NULL,
        DeliveredAtUtc   DATETIME2(3)     NOT NULL,
        TriggeredBy      UNIQUEIDENTIFIER NOT NULL,
        IsDeleted        BIT              NOT NULL CONSTRAINT DF_NotificationsInternalItems_IsDeleted DEFAULT (0),
        CreatedUtc       DATETIME2(3)     NOT NULL CONSTRAINT DF_NotificationsInternalItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy        UNIQUEIDENTIFIER NULL,
        UpdatedUtc       DATETIME2(3)     NULL,
        UpdatedBy        UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_NotificationsInternalItems_TenantId_SiteId] ON [Integrations].[NotificationsInternalItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_NotificationsInternalItems_DeliveredAtUtc] ON [Integrations].[NotificationsInternalItems] (DeliveredAtUtc);
    CREATE NONCLUSTERED INDEX [IX_NotificationsInternalItems_RecipientUserId] ON [Integrations].[NotificationsInternalItems] (RecipientUserId);
END
GO
