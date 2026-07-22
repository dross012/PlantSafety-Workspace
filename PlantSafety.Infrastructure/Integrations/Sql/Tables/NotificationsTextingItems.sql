-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Notifications.Texting
--  Layer:          Infrastructure – SQL Scripts
--  File:           NotificationsTextingItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Integrations.Notifications.Texting read-model, backing
--                  ITextingRepository.GetTextingDeliveriesAsync (Notifications.Texting namespace).
--                  Distinct from [Integrations].[TextingItems], which backs the separate
--                  root-level Integrations.Texting.ITextingRepository interface.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotificationsTextingItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[NotificationsTextingItems] (
        SmsDeliveryId          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        SiteId                 UNIQUEIDENTIFIER NULL,
        RecipientPhoneNumber   NVARCHAR(30)     NOT NULL,
        MessageBody            NVARCHAR(1000)   NOT NULL,
        TriggerEvent           NVARCHAR(200)    NULL,
        Status                 NVARCHAR(50)     NOT NULL,
        SentAtUtc              DATETIME2(3)     NOT NULL,
        SentBy                 UNIQUEIDENTIFIER NOT NULL,
        IsDeleted              BIT              NOT NULL CONSTRAINT DF_NotificationsTextingItems_IsDeleted DEFAULT (0),
        CreatedUtc             DATETIME2(3)     NOT NULL CONSTRAINT DF_NotificationsTextingItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy              UNIQUEIDENTIFIER NULL,
        UpdatedUtc             DATETIME2(3)     NULL,
        UpdatedBy              UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_NotificationsTextingItems_TenantId_SiteId] ON [Integrations].[NotificationsTextingItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_NotificationsTextingItems_SentAtUtc] ON [Integrations].[NotificationsTextingItems] (SentAtUtc);
END
GO
