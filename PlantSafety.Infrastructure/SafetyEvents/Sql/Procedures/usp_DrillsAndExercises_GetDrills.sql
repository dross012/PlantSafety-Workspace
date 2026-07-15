-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.DrillsAndExercises
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_DrillsAndExercises_GetDrills.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IDrillsAndExercisesRepository.GetDrillsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_DrillsAndExercises_GetDrills]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        DrillId,
        TenantId,
        SiteId,
        DrillType,
        Status,
        ConductedAtUtc,
        ConductedBy,
        ParticipantCount
    FROM [SafetyEvents].[DrillsAndExercises]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND ConductedAtUtc >= @FromUtc
      AND ConductedAtUtc <= @ToUtc
    ORDER BY ConductedAtUtc DESC;
END
GO
