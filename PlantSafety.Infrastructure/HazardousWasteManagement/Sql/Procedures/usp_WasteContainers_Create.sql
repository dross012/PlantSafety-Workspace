-- ============================================================================
--  Module: HazardousWasteManagement.WasteContainers | Layer: Infrastructure - SQL Scripts
--  File: usp_WasteContainers_Create.sql | Created: 2026-08-04
--  Description: Insert procedure for [HazardousWasteManagement].[WasteContainers]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_WasteContainers_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @WasteContainerId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AccumulationAreaId UNIQUEIDENTIFIER,
    @WasteStreamId UNIQUEIDENTIFIER,
    @ContainerLabel NVARCHAR(200),
    @CapacityGallons DECIMAL(12,2),
    @RegulatoryTimeLimitDays INT,
    @OpenedAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [HazardousWasteManagement].[WasteContainers]
            ([WasteContainerId], [TenantId], [SiteId], [Status], [AccumulationAreaId], [WasteStreamId], [ContainerLabel], [CapacityGallons], [RegulatoryTimeLimitDays], [OpenedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@WasteContainerId, @TenantId, @SiteId, @Status, @AccumulationAreaId, @WasteStreamId, @ContainerLabel, @CapacityGallons, @RegulatoryTimeLimitDays, @OpenedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
