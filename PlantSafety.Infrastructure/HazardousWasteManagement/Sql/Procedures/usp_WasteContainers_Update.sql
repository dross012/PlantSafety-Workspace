-- ============================================================================
--  Module: HazardousWasteManagement.WasteContainers | Layer: Infrastructure - SQL Scripts
--  File: usp_WasteContainers_Update.sql | Created: 2026-08-04
--  Description: Full-row update procedure for [HazardousWasteManagement].[WasteContainers]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @WasteContainerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(100),
    @ClosedAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [HazardousWasteManagement].[WasteContainers]
        SET
            [Status] = @Status,
            [ClosedAtUtc] = @ClosedAtUtc,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [WasteContainerId] = @WasteContainerId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
