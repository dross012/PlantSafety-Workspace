-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_OutboxMessages_Insert.sql
--  Created:        2026-07-21 (Claude Code -- platform-wide outbox bug fix)
--  Description:    Inserts a single pending outbox envelope. Called once per envelope by
--                  OutboxWriter.FlushAsync (PlantSafety.Infrastructure.Persistence.UnitOfWork).
-- ============================================================================

-- NOTE: the tenant parameter is named @EnvelopeTenantId, NOT @TenantId. DapperCommandExecutor
-- performs RLS defense-in-depth session-context setup by inspecting any parameter literally named
-- "TenantId" and casting it to Guid; the outbox envelope's tenant identifier is a free-form string
-- (not necessarily a Guid), so the C# caller (OutboxWriter.FlushAsync) deliberately avoids that
-- literal name. [System].[OutboxMessages] carries no RLS predicate (cross-tenant system table).
CREATE OR ALTER PROCEDURE [System].[usp_OutboxMessages_Insert]
    @MessageId          UNIQUEIDENTIFIER,
    @EventType          NVARCHAR(200),
    @SchemaVersion      NVARCHAR(50),
    @EnvelopeTenantId   NVARCHAR(100),
    @ActorId            NVARCHAR(100),
    @CorrelationId      NVARCHAR(100),
    @OccurredAt         DATETIME2(3),
    @Payload            NVARCHAR(MAX)
AS
SET NOCOUNT ON;
BEGIN TRY

    INSERT INTO [System].[OutboxMessages]
        (MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload, Processed)
    VALUES
        (@MessageId, @EventType, @SchemaVersion, @EnvelopeTenantId, @ActorId, @CorrelationId, @OccurredAt, @Payload, 0);

END TRY
BEGIN CATCH
    THROW;
END CATCH;
GO
