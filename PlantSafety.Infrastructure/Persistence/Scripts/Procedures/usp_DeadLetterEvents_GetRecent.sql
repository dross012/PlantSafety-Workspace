-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_DeadLetterEvents_GetRecent.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
--  Description:    Paged dead-letter listing, most recently faulted first. Supports the DLQ
--                  dashboard (GetDeadLetterEventsSummaryQuery's sibling detail view) and general
--                  triage. Deterministic ordering/paging per the Deterministic Pagination Rule
--                  (§1.5): explicit ORDER BY, OFFSET/FETCH, stable secondary sort on DeadLetterId
--                  to break ties when multiple rows share the same FaultedAt.
-- ============================================================================

CREATE OR ALTER PROCEDURE [System].[usp_DeadLetterEvents_GetRecent]
    @TenantId       NVARCHAR(100) = NULL,
    @EventType      NVARCHAR(200) = NULL,
    @IncludeReplayed BIT          = 1,
    @Top            INT           = 50,
    @Skip           INT           = 0
AS
SET NOCOUNT ON;

SELECT
    DeadLetterId, MessageId, EventType, SchemaVersion, TenantId, Payload, FaultReason,
    ExceptionDetail, RetryCount, FaultedAt, CreatedAt, CorrelationId, CausationId, ActorId,
    ConsumerName, ReplayedAtUtc, ReplayedByUserId
FROM [System].[DeadLetterEvents]
WHERE (@TenantId IS NULL OR TenantId = @TenantId)
  AND (@EventType IS NULL OR EventType = @EventType)
  AND (@IncludeReplayed = 1 OR ReplayedAtUtc IS NULL)
ORDER BY FaultedAt DESC, DeadLetterId DESC
OFFSET @Skip ROWS FETCH NEXT @Top ROWS ONLY;
GO
