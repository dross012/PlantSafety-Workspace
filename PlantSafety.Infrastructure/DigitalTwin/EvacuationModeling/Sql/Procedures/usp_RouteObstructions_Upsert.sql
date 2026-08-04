-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RouteObstructions_Upsert.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Idempotent MERGE upsert for a single [DigitalTwin].[RouteObstructions] row.
--                  Used by EvacuationRouteDapperRepository.UpdateAsync to persist the aggregate's
--                  in-memory obstruction history without separate insert/clear round-trips.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_RouteObstructions_Upsert]
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
        MERGE [DigitalTwin].[RouteObstructions] AS target
        USING (SELECT @RouteObstructionId AS RouteObstructionId) AS source
            ON target.[RouteObstructionId] = source.RouteObstructionId
        WHEN MATCHED THEN
            UPDATE SET [ClearedAtUtc] = @ClearedAtUtc
        WHEN NOT MATCHED THEN
            INSERT ([RouteObstructionId], [EvacuationRouteId], [TenantId], [Description], [Severity], [RecordedAtUtc], [ClearedAtUtc])
            VALUES (@RouteObstructionId, @EvacuationRouteId, @TenantId, @Description, @Severity, @RecordedAtUtc, @ClearedAtUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
