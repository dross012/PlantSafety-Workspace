-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_ProcessedEvents_Purge.sql
--  Created:        2026-06-17
--  Description:    Purges processed events older than @RetentionDays (default 30).
-- ============================================================================

CREATE OR ALTER PROCEDURE [System].[usp_ProcessedEvents_Purge]
    @RetentionDays  INT          = 30,
    @NowUtc         DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;

        IF @NowUtc IS NULL SET @NowUtc = SYSUTCDATETIME();

        DELETE FROM [System].[ProcessedEvents]
        WHERE ProcessedAt < DATEADD(DAY, -@RetentionDays, @NowUtc);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
