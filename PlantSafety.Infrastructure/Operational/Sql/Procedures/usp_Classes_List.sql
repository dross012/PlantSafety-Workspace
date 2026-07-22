-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Classes
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Classes_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[ClassesReadModel],
--                  backing IClassesRepository.GetClassesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Classes_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ClassId],
        [TenantId],
        [SiteId],
        [Title],
        [InstructorId],
        [Status],
        [ScheduledAtUtc]
FROM [Operational].[ClassesReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [ScheduledAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [ScheduledAtUtc] DESC;
GO
