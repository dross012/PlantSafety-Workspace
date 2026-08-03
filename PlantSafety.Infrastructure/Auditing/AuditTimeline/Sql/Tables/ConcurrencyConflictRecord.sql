-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditTimeline
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ConcurrencyConflictRecord.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the ConcurrencyConflictRecord aggregate root
--                  (IConcurrencyConflictRepository). Durably captures optimistic concurrency
--                  conflicts so they are never silently dropped and can be surfaced by
--                  AuditTimeline reconstruction (Handbook Phase 5 §6.1). Lives in the [Auditing]
--                  schema alongside AuditEngagement/AuditFinding. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Auditing')
BEGIN
    EXEC('CREATE SCHEMA [Auditing]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ConcurrencyConflictRecord' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    CREATE TABLE [Auditing].[ConcurrencyConflictRecord] (
        [ConcurrencyConflictRecordId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]                    UNIQUEIDENTIFIER NOT NULL,
        [EntityType]                  NVARCHAR(200)    NOT NULL,
        [EntityId]                    UNIQUEIDENTIFIER NOT NULL,
        [ExpectedETag]                NVARCHAR(64)     NOT NULL,
        [ActualETag]                  NVARCHAR(64)     NULL,
        [AttemptedOperation]          NVARCHAR(100)    NOT NULL,
        [ActorId]                     UNIQUEIDENTIFIER NULL,
        [OccurredAtUtc]                DATETIME2(3)    NOT NULL,
        [CorrelationId]                UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_ConcurrencyConflictRecord_Tenant_Entity
        ON [Auditing].[ConcurrencyConflictRecord] (TenantId, EntityType, EntityId, OccurredAtUtc DESC);
END
GO
