-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.DigitalTwin
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Twin_Create.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Registers a new Digital Twin. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_Twin_Create]
(
    @TenantId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2(3),
    @TwinId        UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @AssetName     NVARCHAR(200),
    @AssetType     NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO [DigitalTwin].[Twin]
            ([TwinId], [TenantId], [SiteId], [AssetName], [AssetType], [Status],
             [LastSyncAtUtc], [TelemetryHealthScore], [CreatedAtUtc], [CorrelationId], [ActorId])
        VALUES
            (@TwinId, @TenantId, @SiteId, @AssetName, @AssetType, 'Provisioning',
             @NowUtc, 0, @NowUtc, @CorrelationId, @ActorId);

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
