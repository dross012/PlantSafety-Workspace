-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.Observation
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_ObservationItems_GetObservations.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IObservationRepository.GetObservationsAsync. Named
--                  usp_ObservationItems_* (not usp_Observation_*) to avoid
--                  colliding with the unrelated procedures already referenced by
--                  the separate Features-tree ObservationRepository in this same
--                  module.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_ObservationItems_GetObservations]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ObservationId,
        TenantId,
        SiteId,
        ObservationType,
        Category,
        Status,
        ObservedAtUtc,
        ObservedBy
    FROM [SafetyEvents].[ObservationItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ObservedAtUtc >= @FromUtc
      AND ObservedAtUtc <= @ToUtc
    ORDER BY ObservedAtUtc DESC;
END
GO
