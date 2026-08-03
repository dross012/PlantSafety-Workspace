-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditTimeline
--  Layer:          Infrastructure -- SQL Scripts
--  File:           020-covering-index.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (SQL & Database Agent, Phase 5 hardening pass)
--  SchemaVersion:  2026.1
--  Description:    Widens the existing IX_ConcurrencyConflictRecord_Tenant_Entity index into a
--                  fully covering index for usp_ConcurrencyConflictRecord_GetForEntity (the
--                  AuditTimeline chronological-reconstruction read path, Handbook Phase 5 §6.1).
--                  The original index (TenantId, EntityType, EntityId, OccurredAtUtc DESC)
--                  already seeks/sorts correctly but does not INCLUDE ExpectedETag/ActualETag/
--                  AttemptedOperation/ActorId/CorrelationId, so every row still needs a key
--                  lookup back to the clustered index. Since the original index cannot be
--                  ALTERed in place, this adds a second index with the same key order plus the
--                  missing INCLUDE columns; the original narrower index is left in place
--                  untouched (no drops) per the deletion-discipline convention -- a future,
--                  separately-verified cleanup pass can drop the now-redundant original once
--                  confirmed there are no other callers depending on its specific name/shape.
--                  Idempotent (guarded by sys.indexes existence check); additive-only.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ConcurrencyConflictRecord_Tenant_Entity_Covering' AND object_id = OBJECT_ID('[Auditing].[ConcurrencyConflictRecord]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ConcurrencyConflictRecord_Tenant_Entity_Covering
        ON [Auditing].[ConcurrencyConflictRecord] (TenantId, EntityType, EntityId, OccurredAtUtc DESC)
        INCLUDE (ExpectedETag, ActualETag, AttemptedOperation, ActorId, CorrelationId);
END
GO
