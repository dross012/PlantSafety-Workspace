-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OccupancyReadings_Upsert.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Idempotent MERGE upsert for a single [DigitalTwin].[OccupancyReadings] row.
--                  Used by EvacuationZoneDapperRepository.UpdateAsync to persist the aggregate's
--                  in-memory occupancy-reading history. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_OccupancyReadings_Upsert]
    @OccupancyReadingId UNIQUEIDENTIFIER,
    @EvacuationZoneId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @HeadcountEstimate INT,
    @Source NVARCHAR(50),
    @RecordedAtUtc DATETIME2(3),
    @ExceededThreshold BIT
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        MERGE [DigitalTwin].[OccupancyReadings] AS target
        USING (SELECT @OccupancyReadingId AS OccupancyReadingId) AS source
            ON target.[OccupancyReadingId] = source.OccupancyReadingId
        WHEN MATCHED THEN
            UPDATE SET [HeadcountEstimate] = @HeadcountEstimate, [ExceededThreshold] = @ExceededThreshold
        WHEN NOT MATCHED THEN
            INSERT ([OccupancyReadingId], [EvacuationZoneId], [TenantId], [SiteId], [HeadcountEstimate], [Source], [RecordedAtUtc], [ExceededThreshold])
            VALUES (@OccupancyReadingId, @EvacuationZoneId, @TenantId, @SiteId, @HeadcountEstimate, @Source, @RecordedAtUtc, @ExceededThreshold);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
