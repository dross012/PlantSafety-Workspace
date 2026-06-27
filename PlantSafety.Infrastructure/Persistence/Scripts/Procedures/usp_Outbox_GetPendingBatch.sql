-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Outbox_GetPendingBatch.sql
--  Created:        2026-06-17
--  Description:    Fetches up to @BatchSize unprocessed outbox messages.
-- ============================================================================

CREATE OR ALTER PROCEDURE [System].[usp_Outbox_GetPendingBatch]
    @BatchSize  INT = 50
AS
SET NOCOUNT ON;
BEGIN TRY

    SELECT TOP (@BatchSize)
        MessageId,
        EventType,
        SchemaVersion,
        TenantId,
        ActorId,
        CorrelationId,
        OccurredAt,
        Payload
    FROM [System].[OutboxMessages]
    WHERE Processed = 0
    ORDER BY OccurredAt ASC;

END TRY
BEGIN CATCH
    THROW;
END CATCH;
GO
