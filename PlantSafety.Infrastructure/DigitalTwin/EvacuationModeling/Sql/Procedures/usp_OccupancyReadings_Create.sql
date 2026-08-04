-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_OccupancyReadings_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [DigitalTwin].[OccupancyReadings] (append-only history).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_OccupancyReadings_Create]
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
        INSERT INTO [DigitalTwin].[OccupancyReadings]
            ([OccupancyReadingId], [EvacuationZoneId], [TenantId], [SiteId], [HeadcountEstimate], [Source], [RecordedAtUtc], [ExceededThreshold])
        VALUES
            (@OccupancyReadingId, @EvacuationZoneId, @TenantId, @SiteId, @HeadcountEstimate, @Source, @RecordedAtUtc, @ExceededThreshold);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
