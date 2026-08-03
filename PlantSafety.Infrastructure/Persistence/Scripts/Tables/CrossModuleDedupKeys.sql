-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           CrossModuleDedupKeys.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
--  SchemaVersion:  2026.1
--  Description:    Business-key dedup ledger for the Phase 5 §3 cross-module orchestration
--                  consumers (PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration).
--                  [System].[ProcessedEvents] already provides EventId-level idempotency for the
--                  outer MassTransit message envelope, but each consumer's real side-effect write
--                  (creating a CAPA, scheduling maintenance, updating KPIs, etc.) happens through
--                  MediatR -> an Application command handler -> its own repository -> its own
--                  SQL connection/transaction, which is NOT atomic with the ProcessedEvents marker
--                  write recorded by IdempotentConsumerBase.Consume. This table records, per
--                  consumer and per caller-supplied business key (e.g. the triggering alert id, the
--                  completed task id), that the side effect has already been performed, so a
--                  redelivered message is detected and skipped BEFORE the side effect re-runs, not
--                  merely after. See CrossModuleIdempotentConsumerBase.TryExecuteOnceAsync, which
--                  scopes the check-then-act sequence in an IDistributedLockProvider lock so two
--                  concurrent redeliveries of the same logical event cannot both pass the
--                  "not yet handled" check before either has committed. Mirrors ProcessedEvents'
--                  own established shape/precedent (raw parameterized SQL against a simple [System]
--                  ledger table, no stored procedure) rather than introducing a new pattern for this
--                  messaging-infrastructure concern. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'System')
BEGIN
    EXEC('CREATE SCHEMA [System]');
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'System' AND TABLE_NAME = 'CrossModuleDedupKeys')
BEGIN
    CREATE TABLE [System].[CrossModuleDedupKeys] (
        ConsumerName   NVARCHAR(200)    NOT NULL,
        BusinessKey    NVARCHAR(400)    NOT NULL,
        EventId        UNIQUEIDENTIFIER NOT NULL,
        ProcessedAtUtc DATETIME2(3)     NOT NULL CONSTRAINT DF_CrossModuleDedupKeys_ProcessedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_CrossModuleDedupKeys PRIMARY KEY (ConsumerName, BusinessKey)
    );

    CREATE INDEX IX_CrossModuleDedupKeys_ProcessedAtUtc
        ON [System].[CrossModuleDedupKeys] (ProcessedAtUtc);
END
GO
