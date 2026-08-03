-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_DeadLetterEvents_Insert.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
--  Description:    Inserts a single dead-lettered event row. Called once per faulted message by
--                  DeadLetterEventPersister, invoked from DeadLetterFaultObserver's
--                  ConsumeFault<T> (PlantSafety.Infrastructure.Messaging.Consumers.DeadLetter, an
--                  IConsumeObserver connected via cfg.ConnectConsumeObserver) after MassTransit's
--                  retry pipeline (cfg.UseMessageRetry) exhausts all attempts for a given
--                  IConsumer<T>. Mirrors usp_OutboxMessages_Insert's plain
--                  parameterised-procedure style.
-- ============================================================================

-- NOTE: the tenant parameter is named @EnvelopeTenantId, NOT @TenantId, for the same reason as
-- usp_OutboxMessages_Insert -- DapperCommandExecutor's RLS defense-in-depth session-context setup
-- inspects any parameter literally named "TenantId" and casts it to Guid; TenantId here is a
-- free-form string recovered from MassTransit headers, not guaranteed to be a Guid. [System].
-- [DeadLetterEvents] carries no RLS predicate (cross-tenant system table), so no session-context
-- call is required for this procedure.
CREATE OR ALTER PROCEDURE [System].[usp_DeadLetterEvents_Insert]
    @DeadLetterId       UNIQUEIDENTIFIER,
    @MessageId          UNIQUEIDENTIFIER = NULL,
    @EventType          NVARCHAR(200),
    @SchemaVersion      NVARCHAR(50),
    @EnvelopeTenantId   NVARCHAR(100) = NULL,
    @Payload            NVARCHAR(MAX) = NULL,
    @FaultReason        NVARCHAR(400),
    @ExceptionDetail    NVARCHAR(MAX) = NULL,
    @RetryCount         INT = 0,
    @FaultedAt          DATETIME2(3),
    @CorrelationId      UNIQUEIDENTIFIER = NULL,
    @CausationId        UNIQUEIDENTIFIER = NULL,
    @ActorId            NVARCHAR(100) = NULL,
    @ConsumerName       NVARCHAR(400) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY

    INSERT INTO [System].[DeadLetterEvents]
        (DeadLetterId, MessageId, EventType, SchemaVersion, TenantId, Payload, FaultReason,
         ExceptionDetail, RetryCount, FaultedAt, CorrelationId, CausationId, ActorId, ConsumerName)
    VALUES
        (@DeadLetterId, @MessageId, @EventType, @SchemaVersion, @EnvelopeTenantId, @Payload, @FaultReason,
         @ExceptionDetail, @RetryCount, @FaultedAt, @CorrelationId, @CausationId, @ActorId, @ConsumerName);

END TRY
BEGIN CATCH
    THROW;
END CATCH;
GO
