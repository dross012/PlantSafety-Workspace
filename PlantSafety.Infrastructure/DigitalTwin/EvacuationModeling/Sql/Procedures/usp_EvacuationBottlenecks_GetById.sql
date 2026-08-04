-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_EvacuationBottlenecks_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Retrieves a single EvacuationBottlenecks row by identity. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [DigitalTwin].[usp_EvacuationBottlenecks_GetById]
    @EvacuationBottleneckId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [EvacuationBottleneckId], [TenantId], [SiteId], [Status], [RouteId], [SimulationRunId],
    [BottleneckLocation], [DelaySeconds], [RecurrenceCount], [DetectedAtUtc], [CreatedAtUtc], [CreatedBy]
FROM [DigitalTwin].[EvacuationBottlenecks]
WHERE [EvacuationBottleneckId] = @EvacuationBottleneckId AND [IsDeleted] = 0;
GO
