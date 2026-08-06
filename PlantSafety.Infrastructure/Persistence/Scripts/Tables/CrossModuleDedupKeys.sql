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
--
--  FIX (2026-08-06): PK_CrossModuleDedupKeys was originally NVARCHAR(200) + NVARCHAR(400), a
--  clustered composite key of up to 1200 bytes -- over SQL Server's 900-byte clustered-index-key
--  limit (Msg 1946 at CREATE TABLE time as a warning, then a hard failure on any insert whose
--  combined ConsumerName+BusinessKey exceeds 900 bytes). ConsumerName is a consumer class name
--  (SqlCrossModuleDedupStore callers pass the consumer type name) and BusinessKey is a caller-
--  supplied id (e.g. an AlertId/TaskId Guid.ToString()) per this file's own description above --
--  neither needs anywhere near 200/400 characters in practice. Narrowed to NVARCHAR(128) +
--  NVARCHAR(300) (856 bytes combined), which keeps comfortable headroom under the 900-byte limit
--  while still being far larger than any real consumer name or business key seen in this codebase.
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
        ConsumerName   NVARCHAR(128)    NOT NULL,
        BusinessKey    NVARCHAR(300)    NOT NULL,
        EventId        UNIQUEIDENTIFIER NOT NULL,
        ProcessedAtUtc DATETIME2(3)     NOT NULL CONSTRAINT DF_CrossModuleDedupKeys_ProcessedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_CrossModuleDedupKeys PRIMARY KEY (ConsumerName, BusinessKey)
    );

    CREATE INDEX IX_CrossModuleDedupKeys_ProcessedAtUtc
        ON [System].[CrossModuleDedupKeys] (ProcessedAtUtc);
END
GO

-- Additive width fix for environments where the table was already created with the original
-- (too-wide) NVARCHAR(200)/NVARCHAR(400) columns. Idempotent: no-op once already narrowed.
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('[System].[CrossModuleDedupKeys]') AND name = 'ConsumerName' AND max_length > 256)
BEGIN
    ALTER TABLE [System].[CrossModuleDedupKeys] DROP CONSTRAINT PK_CrossModuleDedupKeys;
    ALTER TABLE [System].[CrossModuleDedupKeys] ALTER COLUMN ConsumerName NVARCHAR(128) NOT NULL;
    ALTER TABLE [System].[CrossModuleDedupKeys] ALTER COLUMN BusinessKey NVARCHAR(300) NOT NULL;
    ALTER TABLE [System].[CrossModuleDedupKeys] ADD CONSTRAINT PK_CrossModuleDedupKeys PRIMARY KEY (ConsumerName, BusinessKey);
END
GO
