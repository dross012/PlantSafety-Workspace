-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.Ergonomics
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Ergonomics_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[ErgonomicsReadModel],
--                  backing IErgonomicsRepository.GetErgonomicsAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_Ergonomics_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [ErgonomicsAssessmentId],
        [TenantId],
        [SiteId],
        [WorkstationId],
        [RiskLevel],
        [Status],
        [AssessedAtUtc],
        [AssessedBy]
FROM [Operational].[ErgonomicsReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [AssessedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [AssessedAtUtc] DESC;
GO
