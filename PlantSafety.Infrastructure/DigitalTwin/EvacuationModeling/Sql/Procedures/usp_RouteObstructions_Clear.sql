-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RouteObstructions_Clear.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Marks the most recent open RouteObstructions row for a route as cleared.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_RouteObstructions_Clear]
    @RouteObstructionId UNIQUEIDENTIFIER,
    @ClearedAtUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [DigitalTwin].[RouteObstructions]
        SET [ClearedAtUtc] = @ClearedAtUtc
        WHERE [RouteObstructionId] = @RouteObstructionId AND [ClearedAtUtc] IS NULL;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
