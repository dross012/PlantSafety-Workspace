-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_DeadLetterEvents_MarkReplayed.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
--  Description:    Marks a single dead-lettered event as replayed (ReplayedAtUtc/ReplayedByUserId)
--                  once ReplayDeadLetterEventCommandHandler has successfully re-published the
--                  original payload back onto the bus. Idempotent: re-invoking for an already
--                  replayed row is a no-op (returns @@ROWCOUNT = 0) rather than overwriting the
--                  original replay attribution, per the Domain Action Identity Rule (§1.8.3 --
--                  "Never overwrite historical actor attribution").
-- ============================================================================

CREATE OR ALTER PROCEDURE [System].[usp_DeadLetterEvents_MarkReplayed]
    @DeadLetterId       UNIQUEIDENTIFIER,
    @ReplayedByUserId   UNIQUEIDENTIFIER,
    @ReplayedAtUtc      DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        IF @ReplayedAtUtc IS NULL SET @ReplayedAtUtc = SYSUTCDATETIME();

        UPDATE [System].[DeadLetterEvents]
        SET ReplayedAtUtc = @ReplayedAtUtc,
            ReplayedByUserId = @ReplayedByUserId
        WHERE DeadLetterId = @DeadLetterId
          AND ReplayedAtUtc IS NULL;

        SELECT @@ROWCOUNT;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
