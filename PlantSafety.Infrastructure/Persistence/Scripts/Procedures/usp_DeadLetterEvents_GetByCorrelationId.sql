-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_DeadLetterEvents_GetByCorrelationId.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
--  Description:    Returns every dead-lettered event sharing a given CorrelationId, oldest first,
--                  for tracing a single failed cross-module business flow end-to-end (Handbook
--                  §12.9.1 Correlation ID Propagation / §2.17 Canonical Correlation & Causality
--                  Lineage Rule).
-- ============================================================================

CREATE OR ALTER PROCEDURE [System].[usp_DeadLetterEvents_GetByCorrelationId]
    @CorrelationId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
    DeadLetterId, MessageId, EventType, SchemaVersion, TenantId, Payload, FaultReason,
    ExceptionDetail, RetryCount, FaultedAt, CreatedAt, CorrelationId, CausationId, ActorId,
    ConsumerName, ReplayedAtUtc, ReplayedByUserId
FROM [System].[DeadLetterEvents]
WHERE CorrelationId = @CorrelationId
ORDER BY FaultedAt ASC, DeadLetterId ASC;
GO
