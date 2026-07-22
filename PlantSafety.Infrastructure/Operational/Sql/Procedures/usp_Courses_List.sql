-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Courses
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Courses_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[CoursesReadModel],
--                  backing ICoursesRepository.GetCoursesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Courses_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [CourseId],
        [TenantId],
        [SiteId],
        [Title],
        [Category],
        [Status],
        [PublishedAtUtc],
        [PublishedBy]
FROM [Operational].[CoursesReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [PublishedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [PublishedAtUtc] DESC;
GO
