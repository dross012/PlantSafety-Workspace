-- =============================================
-- Shared.EventBus
-- Script:        01-create-processedevents-table.sql
-- Purpose:       Idempotent-consumer ledger (project_plantsafety_patterns.md §4 "Idempotent
--                Consumer Pattern" / §12 "Consumer Hardening Rules"). One row per
--                (ConsumerGroup, EventId) pair that has been successfully handled. Checked
--                before dispatch; if present, the event is skipped (already processed).
--
-- IDEMPOTENT / NON-DESTRUCTIVE / ADDITIVE-ONLY.
-- =============================================

IF OBJECT_ID(N'dbo.ProcessedEvents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProcessedEvents
    (
        ConsumerGroup    nvarchar(100)    NOT NULL,
        EventId          uniqueidentifier NOT NULL,
        ProcessedAtUtc   datetime2(3)     NOT NULL CONSTRAINT DF_ProcessedEvents_ProcessedAtUtc DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ProcessedEvents PRIMARY KEY CLUSTERED (ConsumerGroup, EventId)
    );
END
GO
