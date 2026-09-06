-- =============================================
-- Shared.EventBus
-- Script:        02-create-eventretrystate-table.sql
-- Purpose:       Bounded-retry-with-backoff bookkeeping (patterns.md §12 "Retry Strategy:
--                bounded retries with exponential backoff"), keyed per (ConsumerGroup,
--                EventId) so different consumer groups retry the same event independently.
--                NextAttemptAtUtc implements the backoff delay; AttemptCount drives both
--                the backoff calculation and the max-attempts -> dead-letter decision.
--
-- IDEMPOTENT / NON-DESTRUCTIVE / ADDITIVE-ONLY.
-- =============================================

IF OBJECT_ID(N'dbo.EventRetryState', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventRetryState
    (
        ConsumerGroup     nvarchar(100)    NOT NULL,
        EventId           uniqueidentifier NOT NULL,
        AttemptCount      int              NOT NULL CONSTRAINT DF_EventRetryState_AttemptCount DEFAULT (0),
        LastAttemptAtUtc  datetime2(3)     NULL,
        NextAttemptAtUtc  datetime2(3)     NOT NULL CONSTRAINT DF_EventRetryState_NextAttemptAtUtc DEFAULT (SYSUTCDATETIME()),
        LastError         nvarchar(2000)   NULL,

        CONSTRAINT PK_EventRetryState PRIMARY KEY CLUSTERED (ConsumerGroup, EventId)
    );
END
GO
