-- =============================================
-- Shared.EventBus
-- Script:        03-create-eventdeadletterqueue-table.sql
-- Purpose:       Dead-letter queue (patterns.md §12 "unresolvable messages move to DLQ with
--                full diagnostic context"). A row is inserted here, and the corresponding
--                dbo.EventRetryState row is removed, once AttemptCount exceeds the
--                configured MaxAttempts (see Shared.EventBus.Infrastructure.Options.
--                EventBusOptions.MaxDeliveryAttempts). Rows are never auto-deleted --
--                operator/backoffice re-drive is a manual, explicit action (out of scope
--                for this checklist item; the table exists so poison messages are captured
--                with full context rather than silently dropped or retried forever).
--
-- IDEMPOTENT / NON-DESTRUCTIVE / ADDITIVE-ONLY.
-- =============================================

IF OBJECT_ID(N'dbo.EventDeadLetterQueue', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventDeadLetterQueue
    (
        DeadLetterId      uniqueidentifier NOT NULL CONSTRAINT DF_EventDeadLetterQueue_DeadLetterId DEFAULT (NEWID()),
        ConsumerGroup     nvarchar(100)    NOT NULL,
        EventId           uniqueidentifier NOT NULL,
        EventType         nvarchar(200)    NOT NULL,
        PayloadType       nvarchar(200)    NOT NULL,
        TenantId          nvarchar(128)    NOT NULL,
        CorrelationId     uniqueidentifier NOT NULL,
        Payload           nvarchar(max)    NOT NULL,
        AttemptCount      int              NOT NULL,
        FailureReason     nvarchar(4000)   NOT NULL,
        IsPoisonMessage   bit              NOT NULL CONSTRAINT DF_EventDeadLetterQueue_IsPoison DEFAULT (0),
        DeadLetteredAtUtc datetime2(3)     NOT NULL CONSTRAINT DF_EventDeadLetterQueue_DeadLetteredAtUtc DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_EventDeadLetterQueue PRIMARY KEY CLUSTERED (DeadLetterId)
    );
END
GO

IF OBJECT_ID(N'dbo.IX_EventDeadLetterQueue_ConsumerGroup_EventId', N'I') IS NULL
BEGIN
    CREATE NONCLUSTERED INDEX IX_EventDeadLetterQueue_ConsumerGroup_EventId
        ON dbo.EventDeadLetterQueue (ConsumerGroup, EventId);
END
GO
