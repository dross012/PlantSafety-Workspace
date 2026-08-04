-- ============================================================================
--  Module: HazardousWasteManagement.AccumulationAreas | Layer: Infrastructure - SQL Scripts
--  File: usp_AccumulationAreas_Create.sql | Created: 2026-08-04
--  Description: Insert procedure for [HazardousWasteManagement].[AccumulationAreas]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @AccumulationAreaId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @AreaName NVARCHAR(400),
    @LocationDescription NVARCHAR(1000),
    @TimeLimitDays INT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [HazardousWasteManagement].[AccumulationAreas]
            ([AccumulationAreaId], [TenantId], [SiteId], [Status], [AreaName], [LocationDescription], [TimeLimitDays], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@AccumulationAreaId, @TenantId, @SiteId, @Status, @AreaName, @LocationDescription, @TimeLimitDays, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
