-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RouteObstructions_Create.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [DigitalTwin].[RouteObstructions] (append-only history).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_RouteObstructions_Create]
    @RouteObstructionId UNIQUEIDENTIFIER,
    @EvacuationRouteId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Description NVARCHAR(1000),
    @Severity NVARCHAR(50),
    @RecordedAtUtc DATETIME2(3),
    @ClearedAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [DigitalTwin].[RouteObstructions]
            ([RouteObstructionId], [EvacuationRouteId], [TenantId], [Description], [Severity], [RecordedAtUtc], [ClearedAtUtc])
        VALUES
            (@RouteObstructionId, @EvacuationRouteId, @TenantId, @Description, @Severity, @RecordedAtUtc, @ClearedAtUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
