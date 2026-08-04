-- ============================================================================
--  Module: HazardousWasteManagement.WasteStreams | Layer: Infrastructure - SQL Scripts
--  File: usp_WasteStreams_Create.sql | Created: 2026-08-04
--  Description: Insert procedure for [HazardousWasteManagement].[WasteStreams], backing
--               IWasteStreamRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteStreams_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @WasteStreamId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @StreamName NVARCHAR(400),
    @EpaWasteCode NVARCHAR(20),
    @HazardClass NVARCHAR(100),
    @GeneratorEpaId NVARCHAR(50),
    @CharacterizationVersion INT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [HazardousWasteManagement].[WasteStreams]
            ([WasteStreamId], [TenantId], [SiteId], [Status], [StreamName], [EpaWasteCode], [HazardClass], [GeneratorEpaId], [CharacterizationVersion], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@WasteStreamId, @TenantId, @SiteId, @Status, @StreamName, @EpaWasteCode, @HazardClass, @GeneratorEpaId, @CharacterizationVersion, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
