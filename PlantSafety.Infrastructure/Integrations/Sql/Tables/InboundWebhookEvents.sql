-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Commands
--  Layer:          Infrastructure – SQL Scripts
--  File:           InboundWebhookEvents.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    ProcessedEvents-style idempotency table backing
--                  IInboundWebhookRepository.RecordEventAsync. Keyed by the external
--                  provider's EventId (per tenant) so retried webhook deliveries of the
--                  same event are recognized and rejected without reprocessing side
--                  effects, matching this solution's established idempotent-consumer
--                  convention.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integrations')
BEGIN
    EXEC('CREATE SCHEMA [Integrations]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InboundWebhookEvents' AND schema_id = SCHEMA_ID('Integrations'))
BEGIN
    CREATE TABLE [Integrations].[InboundWebhookEvents] (
        Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_InboundWebhookEvents_Id DEFAULT (NEWID()) PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        EventId       NVARCHAR(200)    NOT NULL,
        EventType     NVARCHAR(200)    NOT NULL,
        Source        NVARCHAR(200)    NOT NULL,
        OccurredAtUtc DATETIMEOFFSET(3) NOT NULL,
        RawPayload    NVARCHAR(MAX)    NOT NULL,
        ReceivedAtUtc DATETIME2(3)     NOT NULL CONSTRAINT DF_InboundWebhookEvents_ReceivedAtUtc DEFAULT (SYSUTCDATETIME()),
        IsDeleted     BIT              NOT NULL CONSTRAINT DF_InboundWebhookEvents_IsDeleted DEFAULT (0)
    );

    -- One-row-per-(tenant,eventId) uniqueness enforces idempotent recording: a second insert
    -- attempt for the same tenant/event id will violate this constraint, which the owning
    -- procedure catches and treats as "already recorded" (accepted, no duplicate side effects).
    CREATE UNIQUE NONCLUSTERED INDEX [UX_InboundWebhookEvents_TenantId_EventId]
        ON [Integrations].[InboundWebhookEvents] (TenantId, EventId);

    CREATE NONCLUSTERED INDEX [IX_InboundWebhookEvents_ReceivedAtUtc] ON [Integrations].[InboundWebhookEvents] (ReceivedAtUtc);
END
GO
