-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           ProcessedEvents.sql
--  Created:        2026-06-17
--  Updated:        2026-07-21 (Claude Code -- platform-wide outbox bug fix)
--  Author:         David (Ted) + Copilot
--  SchemaVersion:  2026.1
--  Description:    Idempotency log. Records all successfully consumed integration events.
--                  Column shape matches PlantSafety.Infrastructure.Messaging.Consumers.IdempotentConsumerBase
--                  (EventId, ProcessedAt) and usp_ProcessedEvents_Purge (ProcessedAt).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
BEGIN
    EXEC('CREATE SCHEMA [System]');
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'ProcessedEvents')
BEGIN
    CREATE TABLE [System].[ProcessedEvents] (
        EventId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProcessedEvents PRIMARY KEY,
        ProcessedAt DATETIME2(3)     NOT NULL CONSTRAINT DF_ProcessedEvents_ProcessedAt DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_ProcessedEvents_ProcessedAt
        ON [System].[ProcessedEvents] (ProcessedAt);
END
GO
