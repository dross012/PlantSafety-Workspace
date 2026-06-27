-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Outbox_MarkProcessed.sql
--  Created:        2026-06-17
--  Description:    Marks a single outbox message as processed.
-- ============================================================================

CREATE OR ALTER PROCEDURE [System].[usp_Outbox_MarkProcessed]
    @MessageId  UNIQUEIDENTIFIER,
    @NowUtc     DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        UPDATE [System].[OutboxMessages]
        SET    Processed   = 1,
               ProcessedAt = @NowUtc
        WHERE  MessageId   = @MessageId;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
