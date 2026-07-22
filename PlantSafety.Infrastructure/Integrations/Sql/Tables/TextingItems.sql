-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Texting
--  Layer:          Infrastructure – SQL Scripts
--  File:           TextingItems.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the root-level Integrations.Texting read-model, backing
--                  ITextingRepository.GetTextMessagesAsync (PlantSafety.Application.Integrations.Texting).
--                  Distinct from [Integrations].[NotificationsTextingItems], which backs the
--                  separate Integrations.Notifications.Texting.ITextingRepository interface.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TextingItems' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[TextingItems] (
        MessageId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                UNIQUEIDENTIFIER NULL,
        RecipientPhoneNumber  NVARCHAR(30)     NOT NULL,
        MessageTemplate       NVARCHAR(200)    NOT NULL,
        GatewayProvider       NVARCHAR(100)    NOT NULL,
        Status                NVARCHAR(50)     NOT NULL,
        SentAtUtc             DATETIME2(3)     NOT NULL,
        InitiatedBy           UNIQUEIDENTIFIER NOT NULL,
        IsDeleted             BIT              NOT NULL CONSTRAINT DF_TextingItems_IsDeleted DEFAULT (0),
        CreatedUtc            DATETIME2(3)     NOT NULL CONSTRAINT DF_TextingItems_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy             UNIQUEIDENTIFIER NULL,
        UpdatedUtc            DATETIME2(3)     NULL,
        UpdatedBy             UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_TextingItems_TenantId_SiteId] ON [Integrations].[TextingItems] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_TextingItems_SentAtUtc] ON [Integrations].[TextingItems] (SentAtUtc);
END
GO
