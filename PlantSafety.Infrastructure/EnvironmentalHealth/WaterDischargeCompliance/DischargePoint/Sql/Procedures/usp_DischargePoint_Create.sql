-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_DischargePoint_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure backing IDischargePointRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnvironmentalHealth].[usp_DischargePoint_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @DischargePointId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @DischargePermitId UNIQUEIDENTIFIER,
    @PointCode NVARCHAR(50),
    @PointName NVARCHAR(200),
    @ReceivingWaterBody NVARCHAR(200),
    @Outfall NVARCHAR(50),
    @Latitude DECIMAL(9,6) = NULL,
    @Longitude DECIMAL(9,6) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [EnvironmentalHealth].[DischargePoint]
            ([DischargePointId], [TenantId], [SiteId], [Status], [DischargePermitId], [PointCode], [PointName],
             [ReceivingWaterBody], [Outfall], [Latitude], [Longitude], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@DischargePointId, @TenantId, @SiteId, @Status, @DischargePermitId, @PointCode, @PointName,
             @ReceivingWaterBody, @Outfall, @Latitude, @Longitude, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO