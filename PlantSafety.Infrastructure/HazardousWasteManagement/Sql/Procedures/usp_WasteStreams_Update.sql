-- ============================================================================
--  Module: HazardousWasteManagement.WasteStreams | Layer: Infrastructure - SQL Scripts
--  File: usp_WasteStreams_Update.sql | Created: 2026-08-04
--  Description: Full-row update procedure for [HazardousWasteManagement].[WasteStreams],
--               backing IWasteStreamRepository.UpdateAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteStreams_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @WasteStreamId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(100),
    @EpaWasteCode NVARCHAR(20),
    @HazardClass NVARCHAR(100),
    @CharacterizationVersion INT
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [HazardousWasteManagement].[WasteStreams]
        SET
            [Status] = @Status,
            [EpaWasteCode] = @EpaWasteCode,
            [HazardClass] = @HazardClass,
            [CharacterizationVersion] = @CharacterizationVersion,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [WasteStreamId] = @WasteStreamId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
