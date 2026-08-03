-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           DeadLetterEvents.sql
--  Created:        2026-08-02
--  Updated:        2026-08-02 (Phase 6 blocker fix -- MassTransit fault pipeline wiring)
--  Author:         PlantSafety Platform Team (Performance Optimization Agent, Phase 5 §7.2;
--                  Eventing & Messaging Agent, Phase 6)
--  SchemaVersion:  2026.2
--  Description:    Dead-letter analytics table. [System].[OutboxMessages] (pending/published
--                  outbox envelopes) and [System].[ProcessedEvents] (idempotency log of
--                  successfully consumed events) already existed; this codebase had NO table
--                  recording events that permanently failed to publish or be consumed, so a
--                  poisoned/permanently-failing message left no queryable trail once RabbitMQ's
--                  own "_error" queue eventually purged it. Schema mirrors OutboxMessages'
--                  established shape (MessageId/EventType/SchemaVersion/TenantId/Payload/
--                  CreatedAt columns, [System] schema, same PK/index style) for consistency.
--                  Additive-only evolution.
--
--  2026-08-02 UPDATE: this table is now wired to MassTransit's fault pipeline. See
--  DeadLetterFaultObserver (PlantSafety.Infrastructure.Messaging.Consumers.DeadLetter), an
--  IConsumeObserver registered once as a singleton and connected via
--  cfg.ConnectConsumeObserver(...), plus cfg.UseMessageRetry(...) bus-wide in
--  InfrastructureServiceRegistration -- both previously absent from AddMassTransit's
--  configuration. (2026-08-03: reworked from an open-generic IConsumer<Fault<TMessage>>
--  registration, which crashed PlantSafety.Api's strict ValidateOnBuild DI validation at startup.)
--  Columns added below (CorrelationId, CausationId, ActorId, ConsumerName, ReplayedAtUtc,
--  ReplayedByUserId) capture the full diagnostic/replay context the fault persister records and
--  the replay pipeline (ReplayDeadLetterEventCommand) needs. Additive-only evolution -- every new
--  column is nullable so existing rows remain valid.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
BEGIN
    EXEC('CREATE SCHEMA [System]');
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'DeadLetterEvents')
BEGIN
    CREATE TABLE [System].[DeadLetterEvents] (
        DeadLetterId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DeadLetterEvents PRIMARY KEY,
        MessageId       UNIQUEIDENTIFIER NULL,
        EventType       NVARCHAR(200)    NOT NULL,
        SchemaVersion   NVARCHAR(50)     NOT NULL CONSTRAINT DF_DeadLetterEvents_SchemaVersion DEFAULT ('2026.1'),
        TenantId        NVARCHAR(100)    NULL,
        Payload         NVARCHAR(MAX)    NULL,
        FaultReason     NVARCHAR(400)    NOT NULL,
        ExceptionDetail NVARCHAR(MAX)    NULL,
        RetryCount      INT              NOT NULL CONSTRAINT DF_DeadLetterEvents_RetryCount DEFAULT (0),
        FaultedAt       DATETIME2(3)     NOT NULL CONSTRAINT DF_DeadLetterEvents_FaultedAt DEFAULT (SYSUTCDATETIME()),
        CreatedAt       DATETIME2(3)     NOT NULL CONSTRAINT DF_DeadLetterEvents_CreatedAt DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_DeadLetterEvents_EventType_FaultedAt
        ON [System].[DeadLetterEvents] (EventType, FaultedAt)
        INCLUDE (DeadLetterId);

    CREATE INDEX IX_DeadLetterEvents_TenantId
        ON [System].[DeadLetterEvents] (TenantId);
END
GO

-- ---------------------------------------------------------------------------
-- Additive schema hardening (2026-08-02): correlation/causation/actor lineage
-- (Handbook §12.9.1 Correlation ID Propagation), the faulting consumer's identity
-- (needed for DLQ observability / triage), and replay tracking (ReplayedAtUtc /
-- ReplayedByUserId -- Domain Action Identity Rule §1.8.2: replay is a system/operator
-- action, not CRUD authorship, but is tracked here as a simple actor+timestamp pair
-- consistent with this table's existing flat, non-aggregate shape rather than a full
-- PerformedByActor value object, since [System].[DeadLetterEvents] is infrastructure
-- diagnostic state, not a domain aggregate).
-- ---------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('[System].[DeadLetterEvents]') AND name = 'CorrelationId')
BEGIN
    ALTER TABLE [System].[DeadLetterEvents] ADD CorrelationId UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('[System].[DeadLetterEvents]') AND name = 'CausationId')
BEGIN
    ALTER TABLE [System].[DeadLetterEvents] ADD CausationId UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('[System].[DeadLetterEvents]') AND name = 'ActorId')
BEGIN
    ALTER TABLE [System].[DeadLetterEvents] ADD ActorId NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('[System].[DeadLetterEvents]') AND name = 'ConsumerName')
BEGIN
    ALTER TABLE [System].[DeadLetterEvents] ADD ConsumerName NVARCHAR(400) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('[System].[DeadLetterEvents]') AND name = 'ReplayedAtUtc')
BEGIN
    ALTER TABLE [System].[DeadLetterEvents] ADD ReplayedAtUtc DATETIME2(3) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('[System].[DeadLetterEvents]') AND name = 'ReplayedByUserId')
BEGIN
    ALTER TABLE [System].[DeadLetterEvents] ADD ReplayedByUserId UNIQUEIDENTIFIER NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE object_id = OBJECT_ID('[System].[DeadLetterEvents]') AND name = 'IX_DeadLetterEvents_CorrelationId')
BEGIN
    CREATE INDEX IX_DeadLetterEvents_CorrelationId
        ON [System].[DeadLetterEvents] (CorrelationId)
        INCLUDE (DeadLetterId, EventType, FaultedAt);
END
GO
