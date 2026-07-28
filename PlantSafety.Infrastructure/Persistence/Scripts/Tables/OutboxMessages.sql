-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           OutboxMessages.sql
--  Created:        2026-06-17
--  Updated:        2026-07-21 (Claude Code -- platform-wide outbox bug fix)
--  Author:         David (Ted) + Copilot
--  SchemaVersion:  2026.1
--  Description:    Stores serialised domain events pending publish to Azure Service Bus.
--                  Column shape matches PlantSafety.Infrastructure.Persistence.UnitOfWork.OutboxEnvelope
--                  and the existing usp_Outbox_GetPendingBatch / usp_Outbox_MarkProcessed procedures.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
BEGIN
    EXEC('CREATE SCHEMA [System]');
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'OutboxMessages')
BEGIN
    CREATE TABLE [System].[OutboxMessages] (
        MessageId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OutboxMessages PRIMARY KEY,
        EventType       NVARCHAR(200)    NOT NULL,
        SchemaVersion   NVARCHAR(50)     NOT NULL CONSTRAINT DF_OutboxMessages_SchemaVersion DEFAULT ('2026.1'),
        TenantId        NVARCHAR(100)    NOT NULL,
        ActorId         NVARCHAR(100)    NOT NULL,
        CorrelationId   NVARCHAR(100)    NOT NULL,
        OccurredAt      DATETIME2(3)     NOT NULL,
        Payload         NVARCHAR(MAX)    NOT NULL,
        Processed       BIT              NOT NULL CONSTRAINT DF_OutboxMessages_Processed DEFAULT (0),
        ProcessedAt     DATETIME2(3)     NULL,
        CreatedAt       DATETIME2(3)     NOT NULL CONSTRAINT DF_OutboxMessages_CreatedAt DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_OutboxMessages_Processed_OccurredAt
        ON [System].[OutboxMessages] (Processed, OccurredAt)
        INCLUDE (MessageId);

    CREATE INDEX IX_OutboxMessages_TenantId
        ON [System].[OutboxMessages] (TenantId);
END
GO
