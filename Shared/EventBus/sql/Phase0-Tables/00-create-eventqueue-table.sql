-- =============================================
-- Shared.EventBus
-- Script:        00-create-eventqueue-table.sql
-- Purpose:       Canonical, idempotent CREATE TABLE script for dbo.EventQueue -- the
--                durable, cross-module SQL-backed transport for Shared.EventBus.
--                Publishers (e.g. PlantSafety's CrossModuleEventRelay) INSERT rows here;
--                consumer-specific subscribers (e.g. Reporting) poll and process them.
--                No message broker is configured anywhere in this solution today (no
--                RabbitMq/Kafka connection strings, and PlantSafety's own MassTransit +
--                Azure Service Bus wiring is scoped to PlantSafety-internal outbox
--                publishing only -- see PlantSafety.Infrastructure.Messaging.Publishers.
--                ServiceBusEventPublisher) -- so per project_plantsafety_patterns.md's
--                own Outbox pattern, a SQL queue table is the grounded, no-new-infra
--                choice for cross-module transport (checklist step 21 fallback option).
--
-- OWNERSHIP:     Shared.EventBus owns this table. Any module with a ProjectReference to
--                Shared.EventBus.Infrastructure may publish to / poll it.
--
-- IDEMPOTENT / NON-DESTRUCTIVE / ADDITIVE-ONLY: guarded by an OBJECT_ID existence check.
-- No ALTER/DROP against any existing table/column.
-- =============================================

IF OBJECT_ID(N'dbo.EventQueue', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventQueue
    (
        EventId         uniqueidentifier NOT NULL,
        EventType        nvarchar(200)    NOT NULL,
        SchemaVersion    int              NOT NULL,
        PayloadType      nvarchar(200)    NOT NULL,
        TenantId         nvarchar(128)    NOT NULL,
        CorrelationId    uniqueidentifier NOT NULL,
        CausationId      uniqueidentifier NOT NULL,
        OccurredAtUtc    datetime2(3)     NOT NULL,
        SourceModule     nvarchar(100)    NOT NULL,
        Payload          nvarchar(max)    NOT NULL,
        EnqueuedAtUtc    datetime2(3)     NOT NULL CONSTRAINT DF_EventQueue_EnqueuedAtUtc DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_EventQueue PRIMARY KEY CLUSTERED (EventId)
    );
END
GO

IF OBJECT_ID(N'dbo.IX_EventQueue_EventType_EnqueuedAtUtc', N'I') IS NULL
BEGIN
    CREATE NONCLUSTERED INDEX IX_EventQueue_EventType_EnqueuedAtUtc
        ON dbo.EventQueue (EventType, EnqueuedAtUtc);
END
GO
